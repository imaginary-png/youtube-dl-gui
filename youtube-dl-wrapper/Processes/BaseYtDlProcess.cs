using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using youtube_dl_gui_wrapper.Models;

namespace youtube_dl_gui_wrapper
{
    public abstract class BaseYtDlProcess : IYoutubeDownloadProcess
    {
        protected string Exe;
        protected string DefaultOutputFolder = @"%USERPROFILE%\Desktop\";
        protected string NamingScheme = "%(title)s-%(id)s-%(resolution)s.%(ext)s";

        /// <summary>
        /// Creates with an exe path.
        /// </summary>
        /// <param name="exe">Defaults to PATH</param>
        protected BaseYtDlProcess(string exe)
        {
            Exe = exe;
        }

        /// <summary>
        /// Creates with an output path and exe path.
        /// </summary>
        /// <param name="defaultOutputFolder">Defaults to Desktop</param>
        /// <param name="exe">Defaults to PATH</param>
        protected BaseYtDlProcess(string defaultOutputFolder, string exe) : this(exe)
        {
            if (Directory.Exists(defaultOutputFolder))
            {
                DefaultOutputFolder = defaultOutputFolder;
            }
        }

        public async Task<bool> StartDownload(VideoSource source, bool useHeight = false)
        {
            var result = false;
            var downloadFolder = Directory.Exists(source.OutputFolder) ? source.OutputFolder : DefaultOutputFolder;

            //start download with output delegate that updates the videoSource.DownloadInfo -- using helper methods to extract relevant data.
            var outputDel = new DataReceivedEventHandler((object sender, DataReceivedEventArgs args) =>
            {
                if (args.Data == null ||
                    !(args.Data.Contains("[download]") &&   //if line doesn't contain strings [download] and %, e.g.,
                      args.Data.Contains("% of"))) return;     //[download]   2.0% of ~629.58MiB at  1.04MiB/s ETA 09:53
                if (args.Data.Contains("in")) return;          //last line of finished download is: [download] 100% of 115.06MiB in 00:10, ignore.
                UpdateDownloadInfo(source.DownloadLog, args.Data);
            });

            var errorDel = new DataReceivedEventHandler((sender, args) =>
            { //streams seem to output via err, twitch livestreams contain "stream", youtube and twitch clips don't... so I guess this sorta works
                //not thoroughly tested.
                if (args.Data == null) return;
                if (!args.Data.Contains("stream") &&
                    !args.Data.Contains("Stream")) return;
                Trace.WriteLine("\n\n==============================================\nSTREM\n===============================\n\n");
                source.DownloadLog.IsLiveStream = true;
            });

            //currently hardcoded to assume selected format is a height value, not an actual format code. 
            string parameters;


            if (source.SelectedFormat == "audio")
            {
                parameters = @$"-o {downloadFolder}{NamingScheme} " + source.URL +
                             $" -f \"bestaudio\" --newline"; //bestaudio
                result = await Execute(parameters, outputDel, null, token: source.Token);
                return result;
            }
            else if (useHeight) //try with separate video+audio stream first, then try just with video, then throw?
            {
                try
                {
                    parameters = @$"-o {downloadFolder}{NamingScheme} " + source.URL +
                                 $" -f \"bestvideo[height={source.SelectedFormat}]+bestaudio\" --newline";
                    result = await Execute(parameters, outputDel, null, token: source.Token);
                    return result;
                }
                catch (ArgumentException e)
                {
                    //ignore, try args without audio stream
                }

                try
                {
                    //try without specifying video/audio stream.
                    parameters = @$"-o {downloadFolder}{NamingScheme} " + source.URL +
                                 $" -f \"best[height={source.SelectedFormat}]\" --newline";
                    result = await Execute(parameters, outputDel, errorDel, token: source.Token);
                    return result;
                }
                catch (ArgumentException e)
                {
                    //ignore and try with selected format
                }
            }

            //if not basing on height, try...
            parameters = @$"-o {downloadFolder}{NamingScheme} " + source.URL +
                         $" -f {source.SelectedFormat} --newline";

            #region old, ignore, delete later

            /* else if (useHeight && source.URL.Contains("twitch.tv")) //twitch does not have multiple streams, can't use bestvideo+bestaudio
             {
                 parameters = @$"-o {DefaultOutputFolder}{NamingScheme} " + source.URL +
                              $" -f \"bestvideo[height={source.SelectedFormat}]+bestaudio\" --newline";
             }
             else
             {
                 parameters = @$"-o {DefaultOutputFolder}{NamingScheme} " + source.URL +
                              $" -f {source.SelectedFormat} --newline";
             }*/

            #endregion

            return await Execute(parameters, outputDel, null, token: source.Token);
        }


        public async Task<List<VideoFormat>> GetFormats(string url)
        {
            var parameters = url + " -F";
            var formatOutputList = new List<string>();
            var playlist = false;

            var outputDel = new DataReceivedEventHandler((object sender, DataReceivedEventArgs args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data)) return;
                if (args.Data.Contains("playlist")) playlist = true;
                formatOutputList.Add(args.Data);
            });

            await Execute(parameters, outputDel);

            //ToDO
            //do something else if the URL is a playlist....

            var formats = ExtractInfoForFormats(formatOutputList);

            //formatOutputList.ForEach(Console.WriteLine);
            return formats;
        }

        public async Task<string> GetFileName(string url)
        {

            var parameters = url + " --get-filename";
            var filename = string.Empty;
            var outputDel = new DataReceivedEventHandler(((sender, args) =>
            {
                if (string.IsNullOrEmpty(filename)) filename += args.Data;
                Trace.WriteLine($"\n\n===========================\n" +
                                $"filename: {filename}\n" +
                                $"args.data: {args.Data}\n" +
                                $"================================");
            }));

            await Execute(parameters, outputDel);
            return filename;
        }

        public async Task<string> GetDuration(string url)
        {
            var parameters = url + " --get-duration";
            var duration = string.Empty;
            var outputDel = new DataReceivedEventHandler(((sender, args) =>
            {
                if (string.IsNullOrEmpty(duration)) duration += args.Data;
                Trace.WriteLine($"\n\n===========================\n" +
                                $"filename: {duration}\n" +
                                $"args.data: {args.Data}\n" +
                                $"================================");
            }));

            await Execute(parameters, outputDel);
            return duration;
        }

        /// <summary>
        /// Executes youtube-dl.exe with passed in parameters.
        ///
        /// Allows <see cref="DataReceivedEventHandler"/>s for output and error data received.  
        /// </summary>
        /// <param name="parameters">Arguments for youtube-dl</param>
        /// <param name="outputDel">Handler for OutputData events</param>
        /// <param name="errorDel">Handler for ErrorData events</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        protected async Task<bool> Execute(string parameters, DataReceivedEventHandler outputDel = null, DataReceivedEventHandler errorDel = null, CancellationToken token = default)
        {
            Trace.WriteLine($"\n\n======================================\n" +
                            $"Using these params: {parameters}\n" +
                            $"Using Exe Path: {Exe}\n" +
                            $"======================================");
            var errors = new List<string>();

            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = Exe,
                    Arguments = parameters
                };

                p.OutputDataReceived += (sender, args) =>
                {
                    /*  Console.ForegroundColor = ConsoleColor.Yellow;
                      Console.BackgroundColor = ConsoleColor.DarkGray;
                      Console.WriteLine($"Output: {args.Data}");
                      Console.ResetColor();*/
                    //Trace.WriteLine($"Output: {args.Data}");
                };
                p.OutputDataReceived += outputDel;


                p.ErrorDataReceived += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(args.Data)) return;
                    errors.Add(args.Data);

                    /* Console.ForegroundColor = ConsoleColor.Cyan;
                     Console.BackgroundColor = ConsoleColor.DarkGray;
                     Console.WriteLine($"Error: {args.Data}|");
                     Console.ResetColor();*/

                    Trace.WriteLine($"Error: {args.Data}|");
                };
                p.ErrorDataReceived += errorDel;

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                await p.WaitForExitAsync(token);

                var exitCode = p.ExitCode;

                if (errors.Count <= 0) return p.ExitCode == 0 ? true : false;

                string errMsg = "";
                errors.ForEach(s =>
                {
                    errMsg += s + "\n";
                });


                throw new ArgumentException(errMsg);
            }
        }

        /// <summary>
        /// Checks if the executable path exists.
        /// </summary>
        /// <param name="exePath"></param>
        /// <returns></returns>
        public static bool ValidateExePath(string exePath)
        {
            if (File.Exists(exePath)) return true;
            else return false;
        }

        /// <summary>
        /// Takes youtube-dl's -F output and extracts relevant data into a List of <see cref="VideoFormat"/>
        /// </summary>
        /// <param name="formatList">List of strings from youtube-dl -F</param>
        /// <returns>A List of <see cref="VideoFormat"/>s available</returns>
        protected abstract List<VideoFormat> ExtractInfoForFormats(List<string> formatList);

        /// <summary>
        /// Helper function for <see cref="ExtractInfoForFormats"/>  
        /// </summary>
        /// <param name="formatStringArr"></param>
        /// <returns>A <see cref="VideoFormat"/> Object</returns>
        protected abstract VideoFormat GetVideoFormatFromString(string formatStringArr);

        /// <summary>
        /// Helper function for <see cref="StartDownload"/> to extract download info from output
        /// </summary>
        /// <param name="toUpdate"><see cref="DownloadInfo"/> object to update</param>
        /// <param name="info">Log Line to extract download data from</param>
        protected abstract void UpdateDownloadInfo(DownloadInfo toUpdate, string info);
    }
}
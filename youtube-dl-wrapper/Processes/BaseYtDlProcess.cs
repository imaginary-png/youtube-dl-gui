using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Attempts to download the video.
        /// </summary>
        /// <param name="source">Video Source containing URL and Selected Format</param>
        /// <param name="useHeight">Defaults to false. Set whether Selected Format is actually height, not a format code.</param>
        /// <returns><see cref="bool"/> True if download process exited successfully, otherwise false</returns>
        /// <exception cref="ArgumentException"> Throws if invalid URL or Selected Format</exception>
        public async Task<bool> StartDownload(VideoSource source, bool useHeight = false)
        {
            var result = false;
            var downloadFolder = Directory.Exists(source.OutputFolder) ? source.OutputFolder : DefaultOutputFolder;

            //start download with output delegate that updates the videoSource.DownloadInfo -- using helper methods to extract relevant data.
            var outputDel = new DataReceivedEventHandler((object sender, DataReceivedEventArgs args) =>
            {   //example output:  [download]   2.0% of ~629.58MiB at  1.04MiB/s ETA 09:53
                if (args.Data == null ||
                    !(args.Data.Contains("[download]"))) return; //if line doesn't contain [download]   
                if (args.Data.Contains("in")) return;            //last line of finished download is: [download] 100% of 115.06MiB in 00:10, ignore.
                if (args.Data.Contains("% of")) UpdateDownloadInfo(source.DownloadLog, args.Data); //if line contains "% of", normal download progress

                //playlist "video 1 of 3", doesn't show with --newline arg...
                if (Regex.Match(args.Data, @"\d+ of \d+").Success) UpdatePlaylistInfo(source, args.Data); //else if its playlist progress
            });

            var errorDel = new DataReceivedEventHandler((sender, args) =>
            { //streams seem to output via err, twitch livestreams contain "stream", youtube and twitch clips don't... so I guess this sorta works
                //not thoroughly tested.
                if (args.Data == null) return;
                if (!args.Data.Contains("stream") &&
                    !args.Data.Contains("Stream")) return;
                source.DownloadLog.IsLiveStream = true;
            });

            string parameters;

            if (source.SelectedFormat == "audio")
            {
                parameters = @$"-o {downloadFolder}{NamingScheme} " + $"\"{source.URL}\"" +
                             $" -f \"bestaudio\" --newline"; //bestaudio
                result = await Execute(parameters, outputDel, errorDel, token: source.Token);
                return result;
            }
            //if format is 'best', let youtube-dl / yt-dlp choose best streams
            if (source.SelectedFormat == "best")
            {
                parameters = @$"-o {downloadFolder}{NamingScheme} " + $"\"{source.URL}\"" +
                             $" --newline";
                return await Execute(parameters, outputDel, errorDel, token: source.Token);
            }
            if (useHeight) //if attempting download based on height try with separate video+audio streams first, then best[based on height]
            {
                parameters = @$"-o {downloadFolder}{NamingScheme} " + $"\"{source.URL}\"" +
                             $" -f \"bestvideo[height={source.SelectedFormat}]+bestaudio/best[height={source.SelectedFormat}]/best\" --newline";
                result = await Execute(parameters, outputDel, errorDel, token: source.Token);
                return result;
            }
            //if not basing on height, try...
            parameters = @$"-o {downloadFolder}{NamingScheme} " + $"\"{source.URL}\"" +
                         $" -f {source.SelectedFormat} --newline";
            return await Execute(parameters, outputDel, errorDel, token: source.Token);
        }

        /// <summary>
        /// Gets available formats.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<VideoFormat>> GetFormats(string url)
        {
            var parameters = $"\"{url}\" -F";
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

            formatOutputList.ForEach(s => Trace.WriteLine(s));
            formatOutputList.ForEach(Console.WriteLine);
            return formats;
        }

        /// <summary>
        /// Gets video name.
        /// </summary>
        /// <param name="url">video url</param>
        /// <returns></returns>
        public async Task<string> GetFileName(string url)
        {

            var parameters = $"\"{url}\" --get-filename";
            var filename = string.Empty;
            var outputDel = new DataReceivedEventHandler(((sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data)) return;

                if (string.IsNullOrEmpty(filename)) filename += args.Data;
                Trace.WriteLine($"\n===========================\n" +
                                $"filename: {filename}\n" +
                                $"args.data: {args.Data}\n" +
                                $"================================");
            }));

            await Execute(parameters, outputDel);
            return filename;
        }

        /// <summary>
        /// Gets video duration.
        /// </summary>
        /// <param name="url">video url</param>
        /// <returns></returns>
        public async Task<string> GetDuration(string url)
        {
            var parameters = $"\"{url}\" --get-duration";
            var duration = string.Empty;
            var outputDel = new DataReceivedEventHandler(((sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data)) return;
                if (string.IsNullOrEmpty(duration)) duration += args.Data;
                Trace.WriteLine($"\n===========================\n" +
                                $"duration: {duration}\n" +
                                $"args.data: {args.Data}\n" +
                                $"================================");
            }));

            await Execute(parameters, outputDel);
            return duration;
        }

        /// <summary>
        /// Gets file name and duration.
        /// </summary>
        /// <param name="url">video url</param>
        /// <returns></returns>
        public async Task<List<string>> GetFileNameAndDuration(string url)
        {
            var parameters = $"\"{url}\" --get-filename --get-duration";
            var filenameAndDuration = new List<string>();
            var outputDel = new DataReceivedEventHandler(((sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data)) return;
                filenameAndDuration.Add(args.Data);
                Trace.WriteLine($"\n===========================\n" +
                                $"filename + duration\n" +
                                $"args.data: {args.Data}\n" +
                                $"================================");

            }));

            await Execute(parameters, outputDel);
            return filenameAndDuration;
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
                    if (!args.Data.Contains("is not a valid URL") &&
                        !args.Data.Contains("Requested format is not available")) return; //only handle errors for invalid urls or bad format

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
                Trace.WriteLine("\n\n=============================================\n" +
                                $"Exit Code: {exitCode}\n" +
                                $"================================================\n\n");
                if (errors.Count == 0) return p.ExitCode == 0 ? true : false;

                string errMsg = "";
                errors.ForEach(s =>
                {
                    errMsg += s + "\n";
                });

                throw new ArgumentException(errMsg);
            }
        }

        //does not work with --newline arg.
        private void UpdatePlaylistInfo(VideoSource source, string data)
        {
            // get \d+ of \d from string and update duration
            source.Duration = Regex.Match(data, @"\d+ of \d+").Groups[1].Value;
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
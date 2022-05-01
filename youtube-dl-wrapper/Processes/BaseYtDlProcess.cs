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
        protected string OutputFolder = @"%USERPROFILE%\Desktop\";
        protected string NamingScheme = "%(title)s-%(id)s.%(ext)s";

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
        /// <param name="outputFolder">Defaults to Desktop</param>
        /// <param name="exe">Defaults to PATH</param>
        protected BaseYtDlProcess(string outputFolder, string exe) : this(exe)
        {
            if (Directory.Exists(outputFolder))
            {
                OutputFolder = outputFolder;
            }
        }
        
        public async Task<bool> StartDownload(VideoSource source)
        {
            //start download with output delegate that updates the videoSource.DownloadInfo -- using helper methods to extract relevant data.
            var outputDel = new DataReceivedEventHandler((object sender, DataReceivedEventArgs args) =>
            {
                if (args.Data == null ||
                    !(args.Data.Contains("[download]") &&   //if line doesn't contain strings [download] and %, e.g.,
                      args.Data.Contains("% of"))) return;     //[download]   2.0% of ~629.58MiB at  1.04MiB/s ETA 09:53
                UpdateDownloadInfo(source.DownloadLog, args.Data);
            });
            //currently hardcoded to assume selected format is a height value, not an actual format code. 
            var parameters = @$"-o {OutputFolder}{NamingScheme} " + source.URL + $" -f \"bestvideo[width={source.SelectedFormat}]+ba\" --newline";
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
            var errors = new List<string>();

            using (var p = new Process())
            {
                p.StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "yt-dlp.exe", //load custom file location later... File.Exists()... from user config... etc.
                    Arguments = parameters
                };

                p.OutputDataReceived += (sender, args) =>
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"Output: {args.Data}");
                    Console.ResetColor();
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
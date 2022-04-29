﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using youtube_dl_gui_wrapper.Models;

namespace youtube_dl_gui_wrapper
{
    /// <summary>
    /// For use with yt-dlp <see cref="https://github.com/yt-dlp/yt-dlp"/>
    /// </summary>
    public class YtdlpProcess
    {
        private string _exe;
        private string _outputFolder;
        private string _namingScheme = "%(title)s-%(id)s.%(ext)s";

        public YtdlpProcess(string exe = "yt-dlp.exe", string outputFolder = @"%USERPROFILE%\Desktop\")
        {
            _exe = exe;
            _outputFolder = outputFolder;
        }
        public static async Task<bool> StartDownload(VideoSource source)
        {
            //start download with output delegate that updates the videoSource.DownloadInfo -- using helper methods to extract relevant data.
            var outputDel = new DataReceivedEventHandler((object sender, DataReceivedEventArgs args) =>
            {
                if (args.Data == null || 
                    !(args.Data.Contains("[download]") &&   //if line doesn't contain strings [download] and %, e.g.,
                      args.Data.Contains("% of"))) return;     //[download]   2.0% of ~629.58MiB at  1.04MiB/s ETA 09:53
                UpdateDownloadInfo(source.DownloadLog, args.Data);
            });
            var parameters = @"-o %USERPROFILE%\Desktop\%(title)s-%(id)s.%(ext)s " + source.URL + " --newline";
            return await Execute(parameters, outputDel, null, token: source.Token);
        }

        /// <summary>
        /// Returns a list of VideoFormat. Uses arg "youtube-dl -F"  
        /// Throws ArgumentException if invalid URL.
        /// </summary>
        /// <param name="url">The URL of the video</param>
        /// <returns>A List of available video formats</returns>
        public static async Task<List<VideoFormat>> GetFormats(string url)
        {
            var parameters = url + " -F";
            var formatOutputList = new List<string>();

            var outputDel = new DataReceivedEventHandler((object sender, DataReceivedEventArgs args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data)) return;
                formatOutputList.Add(args.Data);
            });

            await Execute(parameters, outputDel);

            var formats = ExtractInfoForFormats(formatOutputList);

            //formatOutputList.ForEach(Console.WriteLine);
            return formats;
        }


        //PUT THIS INTO ABSTRACT BASE CLASS? IT SHOULD BE THE SAME IN BOTH YOUTUBE-DL AND YT-DLP
        /// <summary>
        /// Executes youtube-dl.exe with passed in parameters.
        ///
        /// Allows DataRecievedEventHandlers for output and error data received.  
        /// </summary>
        /// <param name="parameters">Arguments for youtube-dl</param>
        /// <param name="outputDel">Handler for OutputData events</param>
        /// <param name="errorDel">Handler for ErrorData events</param>
        /// <param name="token">Cancellation token</param>
        /// <returns></returns>
        private static async Task<bool> Execute(string parameters, DataReceivedEventHandler outputDel = null, DataReceivedEventHandler errorDel = null, CancellationToken token = default)
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
                    Trace.WriteLine($"Output: {args.Data}");
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


        #region helpers

        #region available video format helpers

        /// <summary>
        /// Takes youtube-dl's -F output and extracts relevant data into a List of <see cref="VideoFormat"/>
        /// </summary>
        /// <param name="formatList">List of strings from youtube-dl -F</param>
        /// <returns>A List of <see cref="VideoFormat"/>s available</returns>
        private static List<VideoFormat> ExtractInfoForFormats(List<string> formatList)
        {
            var videoFormats = new List<VideoFormat>();
           
            var toRemove = formatList.FindIndex( s => s.Contains("RESOLUTION"));
            Console.WriteLine($"\nTO REMOVE {toRemove}\n");

            formatList.RemoveRange(0, toRemove+2); //remove lines that are not relevant.

            formatList.ForEach(Console.WriteLine);

            foreach (var str in formatList)
            {
                if (str.Contains("images")) continue;
                var vf = GetVideoFormatFromString(str);
                videoFormats.Add(vf);
            }

            return videoFormats;
            return null;
        }

        /// <summary>
        /// Helper function for <see cref="ExtractInfoForFormats"/>  
        /// </summary>
        /// <param name="formatStringArr"></param>
        /// <returns>A <see cref="VideoFormat"/> Object</returns>
        private static VideoFormat GetVideoFormatFromString(string formatStringArr)
        {
            var split = Regex.Replace(formatStringArr, @"\s+", " ").Split(" ");

            var formatCode = split[0];
            var ext = split[1];
            var resolution = split[2];
            var resolutionLabel = split[^2];
            var height = string.Empty;
            var width = string.Empty;
            var fps = resolution == "audio" ? "" : split[3] + "fps"; //if 'resolution' is audio, no fps avail.
            var size = split[^1] == "(best)" ? "Unknown" : split[^1];

            if (resolution.Contains("x"))
            {
                var heightxwidth = resolution.Split("x");
                height = heightxwidth[0];
                width = heightxwidth[1];
            }

            return new VideoFormat(formatCode, ext, resolution, resolutionLabel, height, width, fps, size);
        }
        #endregion

        #region Download info string helpers

        private static void UpdateDownloadInfo(DownloadInfo toUpdate, string info)
        {
            Console.WriteLine(info);
            //Example output:
            //[download]   0.2% of 151.34MiB at 83.58KiB/s ETA 30:50
            var infoArr = Regex.Replace(info, @"\s+", " ").Split(" "); //get rid of extra spaces, then split

            var percent = infoArr[1];
            var size = infoArr[3].Replace("~", "");
            var speed = infoArr[5];
            var eta = infoArr[7];


            toUpdate.FileSize = size;
            toUpdate.DownloadPercentage = percent;
            toUpdate.DownloadSpeed = speed;
            toUpdate.ETA = eta;
            //string stuff to get pieces of info
        }

        #endregion

        #endregion
    }
}
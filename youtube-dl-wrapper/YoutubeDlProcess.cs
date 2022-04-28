using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using youtube_dl_gui_wrapper.Models;

namespace youtube_dl_gui_wrapper
{
    public static class YoutubeDlProcess
    {


        public static void StartDownload(VideoSource source)
        {
            //start download with output delegate that updates the videoSource.DownloadInfo -- using helper methods to extract relevant data.
            var outputDel = new DataReceivedEventHandler((object sender, DataReceivedEventArgs args) =>
            {
                UpdateDownloadInfo(source.DownloadLog, args.Data);
            });
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
                formatOutputList.Add(args.Data);
            });

            await Execute(parameters, outputDel);

            var formats = ExtractInfoForFormats(formatOutputList);

            //formatOutputList.ForEach(Console.WriteLine);
            return formats;
        }

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
        private static async Task Execute(string parameters, DataReceivedEventHandler outputDel = null, DataReceivedEventHandler errorDel = null, CancellationToken token = default)
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
                    FileName = "youtube-dl.exe", //load custom file location later... File.Exists()... from user config... etc.
                    Arguments = parameters
                };

                p.OutputDataReceived += (sender, args) =>
                {/*
                    Console.WriteLine($"Output: {args.Data}");
                    Trace.WriteLine($"Output: {args.Data}");*/
                };
                p.OutputDataReceived += outputDel;


                p.ErrorDataReceived += (sender, args) =>
                {
                    if (string.IsNullOrEmpty(args.Data)) return;
                    errors.Add(args.Data);
                    //Console.WriteLine($"Error: {args.Data}|");
                    Trace.WriteLine($"Error: {args.Data}|");
                };
                p.ErrorDataReceived += errorDel;

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                await p.WaitForExitAsync(token);

                if (errors.Count <= 0) return;

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

            formatList.RemoveRange(0, 3); //remove the first 3 lines as they are not relevant.

            foreach (var str in formatList)
            {
                if (str == null) continue;
                var vf = GetVideoFormatFromString(str);
                videoFormats.Add(vf);
            }

            return videoFormats;
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
            var resolutionLabel = split[3];
            var height = string.Empty;
            var width = string.Empty;
            var fps = Regex.Match(formatStringArr, @"\d+fps").Groups[0].Value;

            if (resolution.Contains("x"))
            {
                var heightxwidth = resolution.Split("x");
                height = heightxwidth[0];
                width = heightxwidth[1];
            }

            return new VideoFormat(formatCode, ext, resolution, resolutionLabel, height, width, fps);
        }
        #endregion

        #region Download info string helpers

        private static void UpdateDownloadInfo(DownloadInfo toUpdate, string info)
        {
            //string stuff to get pieces of info
        }

        #endregion

        #endregion
    }
}
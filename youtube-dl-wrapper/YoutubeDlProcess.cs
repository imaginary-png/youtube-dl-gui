using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace youtube_dl_gui_wrapper
{
    public static class YoutubeDlProcess
    {



        /// <summary>
        /// Returns a list of VideoFormat. Uses arg "youtube-dl -F"  
        /// Throws ArgumentException if invalid URL.
        /// </summary>
        /// <param name="url">The URL of the video</param>
        /// <returns>A List of available video formats</returns>
        public static List<VideoFormat> GetFormats(string url)
        {
            var parameters = url + " -F";
            var formatOutputList = new List<string>();

            var outputDel = new DataReceivedEventHandler((object sender, DataReceivedEventArgs args) =>
            {
                formatOutputList.Add(args.Data);
            });

            try
            {
                Execute(parameters, outputDel, null);
            }
            catch (ArgumentException e)
            {
                //Console.WriteLine(">>|" + e.Message + "|<<");
                throw;
            }

            var formats = ExtractInfoForFormats(formatOutputList);

            //formatOutputList.ForEach(Console.WriteLine);
            return formats;
        }

        private static void Execute(string parameters, DataReceivedEventHandler outputDel, DataReceivedEventHandler errorDel)
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
                p.WaitForExit();

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

        #endregion
    }
}
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
        /// Returns a VideoFormats Object with list of available formats. youtube-dl -F  
        /// Throws ArgumentException if invalid URL.
        /// </summary>
        /// <param name="url"></param>
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
                Console.WriteLine(">>|" + e.Message + "|<<");
                throw;
            }

            var formats = ExtractInfoForFormats(formatOutputList);

            //formatOutputList.ForEach(Console.WriteLine);
            return formats;
        }

        private static void Execute(string parameters, DataReceivedEventHandler outputDel, DataReceivedEventHandler errorDel)
        {
            var errors = new List<string>();

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = false;

                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

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
                    Console.WriteLine($"Error: {args.Data}|");
                    Trace.WriteLine($"Error: {args.Data}|");
                };

                p.ErrorDataReceived += errorDel;

                p.StartInfo.FileName = "youtube-dl.exe";
                p.StartInfo.Arguments = parameters;

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

        private static List<VideoFormat> ExtractInfoForFormats(List<string> formatList)
        {
            var videoFormats = new List<VideoFormat>();

            formatList.RemoveRange(0, 3); //remove the first 3 lines as they are not relevant.


            for (int i = 0; i < formatList.Count; i++)
            {
                if (formatList[i] == null) continue;

                var split = Regex.Replace(formatList[i], @"\s+", " ").Split(" ");

                string fps = string.Empty;
                foreach (var s1 in split) //fps isn't in a fixed location, and may not exist if audio only.
                {
                    if (s1.Contains("fps")) fps = s1;
                }
                //formatList[i] = $"format code: {split[0]} | extension: {split[1]} | resolution: {split[2]} {split[3]} | fps: {fps}";
                var formatCode = split[0];
                var ext = split[1];
                var resolution = split[2];
                var resolutionLabel = split[3];
                var height = string.Empty;
                var width = string.Empty;

                if (resolution.Contains("x"))
                {
                    var heightxwidth = resolution.Split("x");
                    height = heightxwidth[0];
                    width = heightxwidth[1];
                }
                videoFormats.Add(new VideoFormat(formatCode, ext, resolution, resolutionLabel, height, width, fps));
            }

            return videoFormats;
        }

        #endregion
    }
}
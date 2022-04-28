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




        public static void GetFormats(string url)
        {
            var parameters = url + " -F";
            var formatList = new List<string>();

            var del = new DataReceivedEventHandler((object sender, DataReceivedEventArgs args) =>
            {
                formatList.Add(args.Data);
            });

            Execute(parameters, del, null);

            Console.WriteLine("OK \n\n");

            ExtractInfoForFormats(formatList);

            formatList.ForEach(s => Console.WriteLine(s));
        }

        private static void Execute(string parameters, DataReceivedEventHandler outputDel, DataReceivedEventHandler errorDel)
        {
            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = false;

                p.StartInfo.RedirectStandardOutput = true;

               /* p.OutputDataReceived += (sender, args) =>
                {
                    Console.WriteLine($"Output: {args.Data}");
                    Trace.WriteLine($"Output: {args.Data}");
                };*/
                p.OutputDataReceived += outputDel;
                
                p.StartInfo.RedirectStandardError = true;

             /*   p.ErrorDataReceived += (sender, args) =>
                {
                    Console.WriteLine($"Error: {args.Data}");
                    Trace.WriteLine($"Error: {args.Data}");
                };*/

                p.ErrorDataReceived += errorDel;

                p.StartInfo.FileName = "youtube-dl.exe";
                p.StartInfo.Arguments = parameters;

                p.Start();

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();

            }
        }


        #region helpers

        private static void ExtractInfoForFormats(List<string> formatList)
        {

            formatList.RemoveRange(0,3); //remove the first 3 lines as they are not relevant.
           

            for (int i = 0; i < formatList.Count; i++)
            {
                if (formatList[i] == null) continue;

                var split = Regex.Replace(formatList[i], @"\s+", " ").Split(" ");

                string fps = "";
                foreach (var s1 in split)
                {
                    if (s1.Contains("fps")) fps = s1;
                }

                Console.WriteLine(split[0]);

                formatList[i] = $"format code: {split[0]} | extension: {split[1]} | resolution: {split[2]} {split[3]} | fps: {fps}";
            }
        }

        #endregion
    }
}
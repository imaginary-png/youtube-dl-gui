using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using youtube_dl_gui_wrapper.Models;

namespace youtube_dl_gui_wrapper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
           var url = @"https://www.youtube.com/watch?v=eey91kzfOZs";
           var vs = new VideoSource(url);

           vs.DownloadLog.PropertyChanged += Updated;
           Start(vs);

           Thread.Sleep(3000);
           vs.Cancel();
           Thread.Sleep(1000);
            Start(vs);
            Console.ReadLine();
        }

        public static async Task Start1(List<VideoSource> vsl)
        {
            vsl.ForEach(async vs => await Start(vs));
        }
        public static async Task Start(VideoSource source)
        {
            try
            {
                source.SelectedFormat = "144";
                source.UseHeightForDownload = true;
                source.Formats.Add(new VideoFormat());
                source.Formats[0].Height = "144";

                await source.Download();

            }
            catch (ArgumentException e)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\n============================================================\n{e.Message}\n=========================================================================\n\n");
                Console.ResetColor();
            }
        }

        public static void Updated(object? sender, PropertyChangedEventArgs e)
        {
            var info = sender as DownloadInfo;

            Console.WriteLine($"{e.PropertyName} updated.\n" +
                              $"{info?.ToString()}\n\n");


        }
    }
}

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
            var url = @"https://www.youtube.com/watch?v=Xpt9TyFzwJA";

            var vsl = new List<VideoSource>();

            var vs1 = new VideoSource("https://www.youtube.com/watch?v=2VauFS071pg");
            var vs2 = new VideoSource("https://www.youtube.com/watch?v=FewJRam0g4I");
            var vs3 = new VideoSource("https://www.youtube.com/watch?v=9RJTQmK0YPI&list=PLlrATfBNZ98dudnM48yfGUldqGD0S4FFb&index=11");
            var vs4 = new VideoSource("https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm");

            vsl.Add(vs1);
            vsl.Add(vs2);
            vsl.Add(vs3);
            vsl.Add(vs4);


            vsl.ForEach(v =>
            {
                try
                {
                    v.GetVideoFormats();
                    Thread.Sleep(5000);
                }
                catch (ArgumentException e)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n\n============================================================\n{e.Message}\n=========================================================================\n\n");
                    Console.ResetColor();
                }
            });

            Thread.Sleep(60000);
        }

        public static async Task Start(VideoSource source)
        {
            try
            {
                await source.GetVideoFormats();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
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

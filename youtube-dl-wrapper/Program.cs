using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace youtube_dl_gui_wrapper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var url = @"https://www.youtube.com/watch?v=Xpt9TyFzwJ";


            var source = new VideoSource(url);

            //Start(source);

            source.URL = @"https://www.youtube.com/watch?v=Xpt9TyFzwJA";

            Start(source);

            Thread.Sleep(1500);

            source.Cancel();
            
            Thread.Sleep(1500);

            //source.Formats = YoutubeDlProcess.GetFormats(source.URL);

            source.Formats.ForEach(Console.WriteLine);
            
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
    }
}

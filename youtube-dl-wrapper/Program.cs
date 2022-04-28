using System;
using System.Threading.Channels;

namespace youtube_dl_gui_wrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var formats = YoutubeDlProcess.GetFormats(@"https://www.youtube.com/watch?v=Xpt9TyFzwJA");

            formats.ForEach(Console.WriteLine);
        }
    }
}

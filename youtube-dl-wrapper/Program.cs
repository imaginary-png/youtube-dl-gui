using System;

namespace youtube_dl_gui_wrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            YoutubeDlProcess.GetFormats(@"https://www.youtube.com/watch?v=Xpt9TyFzwJA");
        }
    }
}

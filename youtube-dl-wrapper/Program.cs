using System;
using System.Threading.Channels;

namespace youtube_dl_gui_wrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var url = @"https://www.youtube.com/watch?v=Xpt9TyFzwJ";


            var source = new VideoSource(url);

            try
            {
                source.GetVideoFormats();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }

            source.URL = @"https://www.youtube.com/watch?v=Xpt9TyFzwJA";
            source.GetVideoFormats();

            //source.Formats = YoutubeDlProcess.GetFormats(source.URL);

            source.Formats.ForEach(Console.WriteLine);
        }
    }
}

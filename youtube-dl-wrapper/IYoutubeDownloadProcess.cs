using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace youtube_dl_gui_wrapper
{
    public interface IYoutubeDownloadProcess
    {
         Task StartDownload(VideoSource source);
         Task<List<VideoFormat>> GetFormats(string url);
    }
}
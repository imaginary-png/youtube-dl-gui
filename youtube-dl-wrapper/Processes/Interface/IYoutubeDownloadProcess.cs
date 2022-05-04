using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace youtube_dl_gui_wrapper
{
    public interface IYoutubeDownloadProcess
    {
        /// <summary>
        /// Starts the youtube-dl download.
        /// </summary>
        /// <param name="source"><see cref="VideoSource"/> containing URL and selected format. If none, defaults to 'best'</param>
        /// <param name="useHeight">Determine whether to use format code or get best based on height resolution</param>
        /// <returns></returns>
        Task<bool> StartDownload(VideoSource source, bool useHeight = false);

        /// <summary>
        /// Returns a list of <see cref="VideoFormat"/>. Uses arg "-F"  
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <param name="url">The URL of the video</param>
        /// <returns>A List of available video formats</returns>
        Task<List<VideoFormat>> GetFormats(string url);

        /// <summary>
        /// Returns name of video. Uses arg "--get-filename"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<string> GetFileName(string url);

        /// <summary>
        /// Returns Duration of video. Uses arg "--get-filename"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<string> GetDuration(string url);
    }
}
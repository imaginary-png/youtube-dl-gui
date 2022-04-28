using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using youtube_dl_gui_wrapper.Annotations;
using youtube_dl_gui_wrapper.Models;

namespace youtube_dl_gui_wrapper
{
    public class VideoSource
    {
        private CancellationTokenSource _cancelToken;

        public string URL { get; set; }
        public List<VideoFormat> Formats { get; set; }
        public string SelectedFormat { get; set; }
        public DownloadInfo Log { get; private set; }

        public VideoSource(string url)
        {
            URL = url;
            Formats = new List<VideoFormat>();
            SelectedFormat = string.Empty;
            Log = new DownloadInfo();
            _cancelToken = new CancellationTokenSource();
        }


        /// <summary>
        /// Gets a list of available video formats.  
        /// Throws ArgumentException if invalid URL.
        /// </summary>
        public async Task GetVideoFormats()
        {
            Formats = await YoutubeDlProcess.GetFormats(URL);
        }

        public void Cancel()
        {
            _cancelToken.Cancel();
        }
    }
}

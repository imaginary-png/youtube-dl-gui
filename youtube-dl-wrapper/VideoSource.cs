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
        private bool _isDownloading = false;

        public string URL { get; set; }
        public List<VideoFormat> Formats { get; set; }
        public string SelectedFormat { get; set; }
        public DownloadInfo DownloadLog { get; private set; }

        public VideoSource(string url)
        {
            URL = url;
            Formats = new List<VideoFormat>();
            SelectedFormat = string.Empty;
            DownloadLog = new DownloadInfo();
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


        public void Start()
        {
            if (_isDownloading) return;
            _isDownloading = false;
            YoutubeDlProcess.StartDownload(this);
        }

        public void Cancel()
        {
            _isDownloading = true;
            _cancelToken.Cancel();
        }


    }
}

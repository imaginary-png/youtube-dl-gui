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
        private readonly CancellationTokenSource _cancelToken;
        private bool _isDownloading = false;
        private IYoutubeDownloadProcess _process;

        public string URL { get; set; }
        public List<VideoFormat> Formats { get; set; }
        public string SelectedFormat { get; set; }
        public DownloadInfo DownloadLog { get; private set; }
        public CancellationToken Token { get; private set; }

        

        public VideoSource(string url)
        {
            URL = url;
            Formats = new List<VideoFormat>();
            SelectedFormat = string.Empty;
            DownloadLog = new DownloadInfo();
            _cancelToken = new CancellationTokenSource();
            Token = _cancelToken.Token;
        }


        /// <summary>
        /// Gets a list of available video formats.  
        /// Throws ArgumentException if invalid URL.
        /// </summary>
        public async Task GetVideoFormats()
        {
            Formats = await YoutubeDlProcess.GetFormats(URL);
        }

        /// <summary>
        /// Starts youtube-dl download process
        /// </summary>
        public async Task Download()
        {
            if (_isDownloading) return;
            _isDownloading = true;
            await YoutubeDlProcess.StartDownload(this);
        }

        /// <summary>
        /// Cancels Download
        /// </summary>
        public void Cancel()
        {
            _isDownloading = false;
            _cancelToken.Cancel();
            Console.WriteLine($"\n\n" +
                              $"================================================================\n" +
                              $"Cancelling Download of {URL}\n" +
                              $"================================================================\n" +
                              $"\n\n");
        }

        public void ChangeYoutubeDlProcess(IYoutubeDownloadProcess process)
        {
            _process = process;
        }

    }
}

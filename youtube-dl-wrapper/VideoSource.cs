using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using youtube_dl_gui_wrapper.Models;

namespace youtube_dl_gui_wrapper
{
    public class VideoSource : ObservableObject
    {
        private CancellationTokenSource _cancelToken;
        private bool _isDownloading = false;
        private IYoutubeDownloadProcess _process;
        private List<VideoFormat> _formats;
        private string _selectedFormat;
        private string _fileName;
        private string _duration;

        public string ExePath { get; set; }
        public string URL { get; set; }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (value == _fileName) return;
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public string Duration
        {
            get => _duration;
            set
            {
                if (value == _duration) return;
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        public string OutputFolder { get; set; }
        /// <summary>
        /// If set true, SelectedFormat is assumed to be a height value
        /// </summary>
        public bool UseHeightForDownload { get; set; }

        public List<VideoFormat> Formats
        {
            get => _formats;
            set
            {
                if (Equals(value, _formats)) return;
                _formats = value;
                OnPropertyChanged(nameof(Formats));
            }
        }

        /// <summary>
        /// The format code to download, or the height if UseHeightForDownload true
        /// </summary>
        public string SelectedFormat
        {
            get => _selectedFormat;
            set
            {
                if (value == _selectedFormat) return;
                _selectedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
            }
        }

        public DownloadInfo DownloadLog { get; private set; }
        public CancellationToken Token { get; private set; }


        /// <summary>
        /// Defaults to using youtube-dl
        /// </summary>
        /// <param name="url">Video URL</param>
        /// <param name="outputFolder">Output folder to download to, defaults to desktop</param>
        /// <param name="useYoutubeDL">Use youtube-dl? otherwise, use yt-dlp</param>
        /// <param name="useHeightForDownload">Use height as basis for video download, not format code, for SelectedFormat</param>
        public VideoSource(string url, string outputFolder = "", bool useYoutubeDL = true, bool useHeightForDownload = true, string exePath = "")
        {
            URL = url;
            FileName = String.Empty;
            Duration = String.Empty;
            Formats = new List<VideoFormat>();
            SelectedFormat = string.Empty;
            DownloadLog = new DownloadInfo();
            _cancelToken = new CancellationTokenSource();
            Token = _cancelToken.Token;
            OutputFolder = outputFolder;
            UseHeightForDownload = useHeightForDownload;
            ExePath = exePath;

            if (useYoutubeDL) UseYoutubeDL();
            else UseYTDLP();
        }




        /// <summary>
        /// Gets a list of available video formats.  
        /// Throws ArgumentException if invalid URL.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public async Task GetVideoFormats()
        {
            Formats = await _process.GetFormats(URL);
        }

        public async Task GetFileName()
        {
            FileName = await _process.GetFileName(URL);
        }
        public async Task GetDuration()
        {
            Duration = await _process.GetDuration(URL);
        }
        public async Task GetFileNameAndDuration()
        {
            var list = await _process.GetFileNameAndDuration(URL);
            if (list.Count >= 2) //if more than 2, must be playlist, but just record the first vids details.
            {
                FileName = list[0];
                Duration = list[1];
            }
            else if (list.Count == 1)
            {
                FileName = list[0];
            }
        }


        /// <summary>
        /// Starts youtube-dl download process
        /// </summary>
        public async Task<bool> Download()
        {
            if (_isDownloading) return false;
            _isDownloading = true;
            var result = await _process.StartDownload(this, UseHeightForDownload);
            _isDownloading = false;
            return result;
        }


        /// <summary>
        /// Cancels Download
        /// </summary>
        public void Cancel()
        {
            Trace.WriteLine($"\n{_cancelToken.IsCancellationRequested}");
            _cancelToken.Cancel();

            Console.WriteLine($"================================================================\n" +
                              $"Cancelling Download of {URL}\n" +
                              $"================================================================\n" +
                              $"\n");
            Trace.WriteLine($"================================================================\n" +
                              $"Cancelling Download of {URL}\n" +
                              $"================================================================\n" +
                              $"\n");
            ResetCancellationToken();
            _isDownloading = false;
        }

        public void ChangeYoutubeDlProcess(IYoutubeDownloadProcess process)
        {
            _process = process;
        }

        /// <summary>
        /// Use yt-dlp.exe
        /// </summary>
        public void UseYTDLP() => _process = new YtdlpProcess(ExePath);

        /// <summary>
        /// use youtube-dl.exe
        /// </summary>
        public void UseYoutubeDL() => _process = new YoutubeDlProcess(ExePath);

        #region Format Selection 

        /// <summary>
        /// Sets the <see cref="SelectedFormat"/> based on resolution (highest Height) instead of using format code.
        /// </summary>
        public void SelectBestResolution()
        {
            SelectedFormat = "0";

            if (Formats == null) return;
            if (Formats.Count == 0) return;

            foreach (var videoFormat in Formats)
            {
                if (string.IsNullOrWhiteSpace(videoFormat.Height)) continue;
                try
                {
                    if (int.Parse(videoFormat.Height) > (int.Parse(SelectedFormat))) SelectedFormat = videoFormat.Height;
                }
                catch
                {
                    //should be safe to ignore, since it'll just skip it. no big deal.
                }
            }
        }
        #endregion

        #region Helpers

        private void ResetCancellationToken()
        {
            _cancelToken.Dispose();
            _cancelToken = new CancellationTokenSource();
            Token = _cancelToken.Token;
        }
        #endregion
    }
}

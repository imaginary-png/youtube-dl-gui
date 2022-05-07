using Newtonsoft.Json;
using System.ComponentModel;
using youtube_dl_gui_wrapper.Models;

namespace youtube_dl_gui.Models
{
    public class Settings : ObservableObject
    {
        private bool _useYoutubeDl;
        private bool _bulkDownload;
        private string _outputFolder;

        /// <summary>
        /// True uses youtube-dl, false uses yt-dlp. Defaults to False.
        /// </summary>
        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseYoutubeDL
        {
            get => _useYoutubeDl;
            set
            {
                if (value == _useYoutubeDl) return;
                _useYoutubeDl = value;
                OnPropertyChanged(nameof(UseYoutubeDL));
            }
        }

        /// <summary>
        /// True downloads concurrently, false downloads one-by-one. Defaults to True.
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool BulkDownload
        {
            get => _bulkDownload;
            set
            {
                if (value == _bulkDownload) return;
                _bulkDownload = value;
                OnPropertyChanged(nameof(BulkDownload));
            }
        }

        /// <summary>
        /// Output Folder for downloads. defaults to desktop
        /// </summary>
        [DefaultValue(@"%USERPROFILE%\Desktop\")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string OutputFolder
        {
            get => _outputFolder;
            set
            {
                if (value == _outputFolder) return;
                _outputFolder = value;
                OnPropertyChanged(nameof(OutputFolder));
            }
        }

        public Settings()
        {
            UseYoutubeDL = false;
            BulkDownload = true;
            OutputFolder = @"%USERPROFILE%\Desktop\";
        }

        public Settings(bool useYoutubeDl, bool bulkDownload, string outputFolder)
        {
            _useYoutubeDl = useYoutubeDl;
            _bulkDownload = bulkDownload;
            _outputFolder = outputFolder;
        }
    }
}
using System.ComponentModel;
using System.Runtime.CompilerServices;
using youtube_dl_gui_wrapper.Annotations;

namespace youtube_dl_gui_wrapper.Models
{
    public class DownloadInfo : INotifyPropertyChanged
    {
        private string _downloadPercentage;
        private string _downloadSpeed;
        private string _fileSize;
        private string _downloaded;

        public string DownloadPercentage
        {
            get => _downloadPercentage;
            set
            {
                if (value == _downloadPercentage) return;
                _downloadPercentage = value;
                OnPropertyChanged(nameof(DownloadPercentage));
            }
        }

        public string DownloadSpeed
        {
            get => _downloadSpeed;
            set
            {
                if (value == _downloadSpeed) return;
                _downloadSpeed = value;
                OnPropertyChanged(nameof(DownloadSpeed));
            }
        }

        public string FileSize
        {
            get => _fileSize;
            set
            {
                if (value == _fileSize) return;
                _fileSize = value;
                OnPropertyChanged(nameof(FileSize));
            }
        }

        public string Downloaded
        {
            get => _downloaded;
            set
            {
                if (value == _downloaded) return;
                _downloaded = value;
                OnPropertyChanged(nameof(Downloaded));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
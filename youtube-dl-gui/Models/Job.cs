using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Security.Cryptography.Pkcs;
using youtube_dl_gui_wrapper;
using youtube_dl_gui_wrapper.Annotations;
using youtube_dl_gui_wrapper.Models;

namespace youtube_dl_gui.Models
{
    public enum JobStatus
    {
        Waiting,
        Downloading,
        Success,
        Failed,
        Cancelled
    }

    public class Job : ObservableObject
    {
        private string _status;
        private bool _isNotDownloading;

        public string Status
        {
            get => _status;
            private set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged(nameof(Status));

                if (value == JobStatus.Downloading.ToString()) IsNotDownloading = false;
                else IsNotDownloading = true;
            }
        }

        public bool IsNotDownloading
        {
            get => _isNotDownloading;
            set
            {
                if (value == _isNotDownloading) return;
                _isNotDownloading = value;
                OnPropertyChanged(nameof(IsNotDownloading));
            }
        }

        public VideoSource Source { get; set; }


        public Job(VideoSource source)
        {
            Source = source;
            Status = JobStatus.Waiting.ToString();
            IsNotDownloading = true;

            Source.DownloadLog.PropertyChanged += UpdateJobStatus;
        }

        public void SetStatus(JobStatus status)
        {
            Status = status.ToString();
        }

        private void UpdateJobStatus([CanBeNull] object o, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Source.DownloadLog.DownloadPercentage))
            {
                if (Source.DownloadLog.DownloadPercentage == "100%") SetStatus(JobStatus.Success);
                else SetStatus(JobStatus.Downloading);
            }
        }
    }
}
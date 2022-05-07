using System.ComponentModel;
using System.Threading.Tasks;
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
        private bool _changeToWaiting = false;

        public string Status
        {
            get => _status;
            private set
            {
                if (value == _status) return;
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }


        public VideoSource Source { get; set; }


        public Job(VideoSource source)
        {
            Source = source;
            Status = JobStatus.Waiting.ToString();

            Source.DownloadLog.PropertyChanged += UpdateJobStatus;
            Source.PropertyChanged += UpdateJobStatus;
        }

        public void SetStatus(JobStatus status)
        {
            Status = status.ToString();
        }

        private async void UpdateJobStatus([CanBeNull] object o, PropertyChangedEventArgs e)
        {
            //THIS IF BLOCK CAN PROBABLY BE DELETED, JUST WANT TO TEST A BIT MORE AT SOME POINT. -- SAME NOW FUNCTIONALITY HANDLED IN DOWNLOADPAGEVIEWMODEL -> DOJOB();
            //update download log  
          /*  if (e.PropertyName == nameof(Source.DownloadLog.DownloadPercentage))
            {   //yt-dlp - no decimal, youtube-dl decimal... for 100%.
                if (Source.DownloadLog.DownloadPercentage == "100%" || Source.DownloadLog.DownloadPercentage == "100.0%") SetStatus(JobStatus.Success);
                else SetStatus(JobStatus.Downloading);
            }*/

            //if livestream, update to downloading...
            if (e.PropertyName == nameof(Source.DownloadLog.IsLiveStream))
            {
                if (Source.DownloadLog.IsLiveStream)
                {
                    SetStatus(JobStatus.Downloading);
                    Source.DownloadLog.FileSize = "Live Stream";
                }
            }
            //if selected format changes, set status ready for download
            else if (e.PropertyName == nameof(Source.SelectedFormat))
            {
                //if already going to change status to waiting, don't bother trying again.
                if (_changeToWaiting) return;
                UpdateJobStatusToWaitingAfterDownloadFinish();
            }
        }

        private async Task UpdateJobStatusToWaitingAfterDownloadFinish()
        {
            _changeToWaiting = true;

            while (Status == JobStatus.Downloading.ToString())
            {
                await Task.Delay(200);
            }
            SetStatus(JobStatus.Waiting);
            _changeToWaiting = false;
        }
    }
}
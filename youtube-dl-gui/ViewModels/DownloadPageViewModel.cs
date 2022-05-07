using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using youtube_dl_gui.Commands;
using youtube_dl_gui.Models;
using youtube_dl_gui_wrapper;

namespace youtube_dl_gui.ViewModels
{
    public class DownloadPageViewModel : BaseUserControlViewModel
    {
        //default executable file locations which should be located in the downloadable release.
        private readonly string DefaultYoutubeDlExePath;
        private readonly string DefaultYtDlpExePath;

        private string _urlInputText;
        private Settings _settings;


        public string URLInputText
        {
            get => _urlInputText;
            set
            {
                if (value == _urlInputText) return;
                _urlInputText = value;
                OnPropertyChanged(nameof(URLInputText));
            }
        }

        public ObservableCollection<Job> Jobs { get; set; }
        public List<string> URLS { get; set; }

        public ICommand AddURLsCommand { get; private set; }
        public ICommand ToggleDownloadCommand { get; private set; }
        public ICommand CancelRemoveJobCommand { get; private set; }


        public DownloadPageViewModel()
        {
            DefaultYoutubeDlExePath = Path.GetFullPath(@"Exe\youtube-dl.exe");
            DefaultYtDlpExePath = Path.GetFullPath(@"Exe\yt-dlp.exe");

            Jobs = new ObservableCollection<Job>();
            URLS = new List<string>();

            //use default settings
            _settings = new Settings();


            AddURLsCommand = new RelayCommand(async p => await AddURLs_Execute(), null);
            ToggleDownloadCommand = new RelayCommand(p => ToggleDownload_Execute(), null);
            CancelRemoveJobCommand = new RelayCommand(p => CancelRemoveJob_Execute((Job)p), null);
        }

        //update settings - this is called from MainViewModel
        public void UpdateSettings(Settings settings)
        {
            _settings = settings;
            UpdateOutPutFolder();
        }

        private void UpdateOutPutFolder()
        {
            foreach (var job in Jobs)
            {   //bother checking? or just assign either way?
                if (job.Source.OutputFolder == _settings.OutputFolder) continue;
                job.Source.OutputFolder = _settings.OutputFolder;
            }
        }
        private async Task AddURLs_Execute()
        {
            if (string.IsNullOrWhiteSpace(URLInputText)) return;

            var exePath = GetExePath();

            var splitInput = Regex.Replace(URLInputText, @"\s+", " ").Trim().Split(" ");

            //is clearing input before or after better ux? idk. after, but does it matter that much?
            URLInputText = "";
            var usedUrls = new List<string>();

            foreach (var s in splitInput)
            {
                if (Jobs.FirstOrDefault(j => j.Source.URL == s) != null) continue;
                if (usedUrls.Contains(s)) continue;
                if (s.Contains("youtube") && s.Contains("&list=")) //handle youtube playlists
                {
                    AddYoutubePlayList(s, exePath);
                    continue;
                }

                usedUrls.Add(s);

                // using height for video download to simplify GUI functionality.
                var videoSource = new VideoSource(s, _settings.OutputFolder, _settings.UseYoutubeDL, true, exePath);

                //awaiting here coz if more than 4-5 videos with 3 processes for getting name,duration,formats, it uses a lot of cpu
                await GetVideoData(videoSource);
            }

        }

        private void ToggleDownload_Execute()
        {
            if (_settings.BulkDownload) DownloadInBulk();
            else DownloadOneByOne();
        }

        private void CancelRemoveJob_Execute(Job job)
        {
            if (job.Status == JobStatus.Downloading.ToString())
            {
                job.Source.Cancel();
                job.SetStatus(JobStatus.Cancelled);
            }
            else
            {
                Jobs.Remove(job);
            }
        }

        #region Helpers

        #region Video from URL helpers

        private void AddYoutubePlayList(string url, string exePath)
        {
            var source = new VideoSource(url, _settings.OutputFolder, _settings.UseYoutubeDL, true, exePath)
            {
                SelectedFormat = "best",
                FileName = url + " playlist",
                DownloadLog = { FileSize = "playlist" }
            };
            var format = new VideoFormat("best", "best", "best", "best", "best", "best", "best"); //just default to 'best' for download..

            source.Formats.Add(format);
            Jobs.Add(new Job(source));
        }

        /// <summary>
        /// Helper for getting video information.
        /// </summary>
        /// <param name="videoSource"></param>
        /// <returns></returns>
        private async Task GetVideoData(VideoSource videoSource)
        {
            var errors = "";
            try
            {
                await Task.WhenAll(videoSource.GetFileName(),
                    videoSource.GetDuration(),
                    videoSource.GetVideoFormats());

                SimplifyFormatsToUniqueResolutionsOnly(videoSource);
                Jobs.Add(new Job(videoSource));
            }
            catch (ArgumentException e)
            {
                //errors += e.Message + "\n";
                errors += videoSource.URL + "\n";
            }

            if (errors.Length > 0)
            {
                MessageBox.Show($"Could not retrieve video information from the following:\n\n" +
                                $"{errors}",
                    "Error retrieving info from URLs",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Gets the path to the youtube-dl / yt-dlp executable
        /// </summary>
        /// <returns></returns>
        private string GetExePath()
        {
            if (_settings.UseYoutubeDL) return DefaultYoutubeDlExePath;
            else return DefaultYtDlpExePath;
        }

        /// <summary>
        /// Removes duplicate 'resolution labels', since to keep this program simple we will only allow selection of resolution for download, e.g. 720p, 1440p, etc.
        /// Instead of showing information for each resolution - bitrate - container, etc.
        /// </summary>
        /// <param name="source"></param>
        private void SimplifyFormatsToUniqueResolutionsOnly(VideoSource source)
        {
            /*if (source.URL.Contains("twitch.tv"))
            {
                SimplifyTwitchURL(source);
                return;
            }*/

            source.Formats.ForEach(s => Trace.WriteLine(s));

            var resolutions = new List<string>();

            //get a list of available resolutions based on height, e.g., 1920x1080 = 1080p, 2560x1440 = 1440p
            source.Formats.ForEach(f =>
            {
                if (!resolutions.Contains(f.Height))
                {
                    resolutions.Add(f.Height);
                }
            });
            //assign new available formats
            var tempList = new List<VideoFormat>();
            resolutions.ForEach(s =>
            {
                var vf = new VideoFormat
                {
                    Height = s == string.Empty ? "audio" : s,
                    Resolution = s,
                    ResolutionLabel = s == string.Empty ? "audio" : s + "p"
                };
                tempList.Add(vf);
            });
            tempList.Reverse();
            source.Formats = tempList;
            //set the default selected format to the largest resolution
            source.SelectedFormat = source.Formats[0].Height;
        }

        //just set any twitch links to "best" quality only, for simplicity.
        private void SimplifyTwitchURL(VideoSource source)
        {
            var format = new VideoFormat
            {
                ResolutionLabel = "best",
                Height = "best" //height is needed for the view's combobox default selection
            };
            source.Formats = new List<VideoFormat> { format };
            source.SelectedFormat = source.Formats[0].ResolutionLabel;
        }
        #endregion

        #region Download helpers

        private async Task DownloadOneByOne()
        {
            foreach (var j in Jobs)
            {
                if (j.Status == JobStatus.Success.ToString() || j.Status == JobStatus.Downloading.ToString()) continue; //skip if finished or currently downloading 
                Trace.WriteLine($"Starting Job: {j.Source.FileName}\n");

                /*var result = await j.Source.Download();

                if (result) j.SetStatus(JobStatus.Success);
                else j.SetStatus(JobStatus.Failed);*/

                await DoJob(j);

                Trace.WriteLine($"\nFinished Job: {j.Source.FileName}\n");
                //else something went wrong, fail? cancelled?
            }
        }

        private async Task DoJob(Job job)
        {
            job.SetStatus(JobStatus.Downloading);
            var result = await job.Source.Download();

            if (result) job.SetStatus(JobStatus.Success);
            else job.SetStatus(JobStatus.Failed);
        }

        private async Task DownloadInBulk()
        {
            Trace.WriteLine("===================================================\n" +
                            "Bulk Downloading...\n" +
                            "====================================================\n");
            var queueList = new List<Task>();

            foreach (var j in Jobs)
            {
                if (j.Status == JobStatus.Success.ToString() || j.Status == JobStatus.Downloading.ToString()) continue; //skip if finished or currently downloading 
                Trace.WriteLine("Adding Job " + j.Source.FileName);

                //queueList.Add(j.Source.Download());
                queueList.Add(DoJob(j));
            }

            while (queueList.Any())
            {
                var finTask = await Task.WhenAny(queueList);
                queueList.Remove(finTask);
            }


            Trace.WriteLine("===================================================\n" +
                            "Bulk Downloading... DONE!\n" +
                            "====================================================\n");
        }

        #endregion



        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualBasic.CompilerServices;
using youtube_dl_gui.Commands;
using youtube_dl_gui.Models;
using youtube_dl_gui_wrapper;
using youtube_dl_gui_wrapper.Annotations;

namespace youtube_dl_gui.ViewModels
{
    public class DownloadPageViewModel : BaseUserControlViewModel
    {
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

            var splitInput = Regex.Replace(URLInputText, @"\s+", " ").Trim().Split(" ");
            //maybe use a boolean on the textbox and button to set isHitTestVisible = false; while processing?
            //is clearing input before or after better ux? idk. after, but does it matter that much?
            URLInputText = "";
            var usedUrls = new List<string>();

            foreach (var s in splitInput)
            {
                if (Jobs.FirstOrDefault(j => j.Source.URL == s) != null) continue;
                if (usedUrls.Contains(s)) continue;
                if (s.Contains("youtube") && s.Contains("&list=")) //handle youtube playlists
                {
                    AddYoutubePlayList(s);
                    continue;
                }

                usedUrls.Add(s);

                // using yt-dlp as default, since youtube-dl has slow dl for youtube.
                // using height for video download to simplify GUI functionality.
                var videoSource = new VideoSource(s, _settings.OutputFolder ,_settings.UseYoutubeDL, true);

                //awaiting here coz if more than 4-5 videos with 3 processes for getting name,duration,formats
                //it uses a lot of cpu and mem
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

        private void AddYoutubePlayList(string url)
        {
            var source = new VideoSource(url, _settings.OutputFolder, _settings.UseYoutubeDL, true)
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
        /// Removes duplicate 'resolution labels', since to keep this program simple we will only allow selection of resolution for download, e.g. 720p, 1440p, etc.
        /// Instead of showing information for each resolution - bitrate - container, etc.
        /// </summary>
        /// <param name="source"></param>
        private void SimplifyFormatsToUniqueResolutionsOnly(VideoSource source)
        {
            if (source.URL.Contains("twitch.tv"))
            {
                SimplifyTwitchURL(source);
                return;
            }

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
                await j.Source.Download();
                Trace.WriteLine($"\n{j.Source.FileName} Finished\n");
                //else something went wrong, fail? cancelled?
            }
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
                Trace.WriteLine("Adding Job" + j.Source.FileName);
                queueList.Add(j.Source.Download());
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
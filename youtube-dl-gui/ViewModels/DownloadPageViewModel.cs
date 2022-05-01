using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualBasic.CompilerServices;
using youtube_dl_gui.Commands;
using youtube_dl_gui_wrapper;

namespace youtube_dl_gui.ViewModels
{
    public class DownloadPageViewModel : BaseUserControlViewModel
    {
        private bool UseYoutubeDL = false;

        public string InputText { get; set; }
        public ObservableCollection<VideoSource> Sources { get; set; }
        public List<string> URLS { get; set; }

        public ICommand TestCommand { get; set; }
        public ICommand AddURLsCommand { get; set; }

        public DownloadPageViewModel()
        {
            Sources = new ObservableCollection<VideoSource>();
            URLS = new List<string>();

            TestCommand = new RelayCommand(p => TestCommand_Execute(), null);
            AddURLsCommand = new RelayCommand( p => AddURLs_Execute(), null);


            Sources.Add(new VideoSource("URL")
            {
                DownloadLog =
                {
                    Downloaded = "222220", DownloadPercentage = "2220", DownloadSpeed = "2220", ETA = "30224s",
                    FileSize = "2274MiB"
                },
                Formats = new List<VideoFormat>
                {
                    new VideoFormat("1", "m2p4", "72250x2103", "1080p", "1080", "1920", "30222fps"),
                    new VideoFormat("2", "m2p4", "72250x2103", "720p", "720", "1280", "30222fps"),
                    new VideoFormat("3", "m2p4", "72250x2103", "360p", "360", "480", "30222fps")
                },
                SelectedFormat = "360",
                URL = "LOL1"

            });


        }

        private async Task AddURLs_Execute()
        {
            if (string.IsNullOrWhiteSpace(InputText)) return;
            var errors = "";

            var splitInput = InputText.Split(" ");


            foreach (var s in splitInput)
            {
                if (Sources.FirstOrDefault(v => v.URL == s) != null) continue;

                var videoSource = new VideoSource(s);
                try
                {
                    await videoSource.GetVideoFormats();
                    Sources.Add(videoSource);
                }
                catch (ArgumentException e)
                {
                    errors += e.Message+"\n";
                }
            }

            if (errors.Length > 0)
            {
                MessageBox.Show($"Could not retrieve video information from the following:\n\n" +
                                $"{errors}",
                    "Error retrieving info from URLs",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            InputText = "";

        }
        private void TestCommand_Execute()
        {
            Trace.WriteLine($"\n\n{InputText}\n\n");
            /*
            Sources.Add(new VideoSource("URL")
            {
                DownloadLog =
                {
                    Downloaded = "450", DownloadPercentage = "88", DownloadSpeed = "1200", ETA = "524s",
                    FileSize = "500"
                },
                Formats = new List<VideoFormat>
                {
                    new VideoFormat("1", "m2p4", "1080x1920", "1080p", "1080", "1920", "30ps"),
                    new VideoFormat("2", "m2p4", "720x1280", "720p", "720", "1280", "30fps"),
                    new VideoFormat("3", "m2p4", "360x480", "360p", "360", "480", "22fps")
                },
                SelectedFormat = "360",
                URL = "LOL1"

            });

            Sources[0].URL += "5";*/
        }
    }
}
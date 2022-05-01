using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using youtube_dl_gui.Commands;
using youtube_dl_gui_wrapper;
using youtube_dl_gui_wrapper.Annotations;

namespace youtube_dl_gui.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private BaseUserControlViewModel _currentView;

        public BaseUserControlViewModel CurrentView
        {
            get => _currentView;
            set
            {
                if (Equals(value, _currentView)) return;
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        public BaseUserControlViewModel DownloadsPage { get; set; }
        public BaseUserControlViewModel SettingsPage { get; set; }

        public ICommand ButtonCommand { get; set; }
        public ICommand FontUpCommand { get; set; }

        public ObservableCollection<VideoSource> Sources { get; set; }
        public List<string> URLS { get; set; }


        public MainWindowViewModel()
        {
            DownloadsPage = new DownloadPageViewModel();
            SettingsPage = new SettingsViewModel();
            CurrentView = DownloadsPage;

            Sources = new ObservableCollection<VideoSource>();
            URLS = new List<string>();

            ButtonCommand = new RelayCommand(p=>ChangeView((BaseUserControlViewModel)p), p => p is BaseUserControlViewModel);
        }



        private void ChangeView(BaseUserControlViewModel viewModel)
        {
            Trace.WriteLine(viewModel == DownloadsPage);
            if (viewModel == CurrentView) return;

            if (viewModel == DownloadsPage)
            {
                CurrentView = DownloadsPage;
                return;
            }

            if (viewModel == SettingsPage)
            {
                CurrentView = SettingsPage;
                return;
            }
        }
        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
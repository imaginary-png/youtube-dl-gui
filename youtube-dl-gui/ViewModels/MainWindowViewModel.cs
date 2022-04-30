using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Input;
using youtube_dl_gui.Commands;
using youtube_dl_gui_wrapper.Annotations;

namespace youtube_dl_gui.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private BaseUserControlViewModel _currentView;
        public string Text { get; set; }

        private List<BaseUserControlViewModel> UserControls { get; set; }

        public BaseUserControlViewModel CurrentView
        {
            get => _currentView;
            set
            {
                if (Equals(value, _currentView)) return;
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public BaseUserControlViewModel DownloadsPage { get; set; }
        public BaseUserControlViewModel SettingsPage { get; set; }

        public ICommand ButtonCommand { get; set; }

        public MainWindowViewModel()
        {
            Text = "hello";

            UserControls = new List<BaseUserControlViewModel>();

            DownloadsPage = new DownloadPageViewModel();
            SettingsPage = new SettingsViewModel();
            CurrentView = DownloadsPage;

            UserControls.Add(DownloadsPage);
            UserControls.Add(SettingsPage);

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
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
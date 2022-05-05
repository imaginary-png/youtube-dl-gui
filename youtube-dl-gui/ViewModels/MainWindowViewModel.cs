using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using youtube_dl_gui.Commands;
using youtube_dl_gui.Models;
using youtube_dl_gui_wrapper.Models;

namespace youtube_dl_gui.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private BaseUserControlViewModel _currentView;
        private string _text;

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

        public ICommand ChangeViews { get; set; }
        public ICommand FontUpCommand { get; set; } //ToDO

        public string Text
        {
            get => _text;
            set
            {
                if (value == _text) return;
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public MainWindowViewModel()
        {
            DownloadsPage = new DownloadPageViewModel();
            SettingsPage = new SettingsViewModel();
            
            var settingsPage = SettingsPage as SettingsViewModel;
            settingsPage.SettingsManager.PropertyChanged += SettingsUpdated;
            SettingsUpdated(settingsPage.SettingsManager, null); //manually update settings on startup.

            CurrentView = DownloadsPage;

            ChangeViews = new RelayCommand(p => ChangeView((BaseUserControlViewModel)p), p => p is BaseUserControlViewModel);
        }

        private void SettingsUpdated(object sender, PropertyChangedEventArgs e)
        {
            var settingsManager = sender as SettingsManager;
            var downloadsPage = DownloadsPage as DownloadPageViewModel;
            downloadsPage?.UpdateSettings(settingsManager.UserSettings);
        }

        private void ChangeView(BaseUserControlViewModel viewModel)
        {
            if (viewModel == CurrentView) return;

            if (viewModel == DownloadsPage)
            {
                CurrentView = DownloadsPage;
                return;
            }

            if (viewModel == SettingsPage)
            {
                CurrentView = SettingsPage;
            }
        }
    }
}
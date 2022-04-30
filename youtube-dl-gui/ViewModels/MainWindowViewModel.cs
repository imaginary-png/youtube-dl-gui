using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using youtube_dl_gui.Commands;
using youtube_dl_gui_wrapper.Annotations;

namespace youtube_dl_gui.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private BaseUserControlViewModel _currentView;
        private int _fontSize;
        public string Text { get; set; }

        public int FontSize
        {
            get => _fontSize;
            set
            {
                if (value == _fontSize) return;
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        private SolidColorBrush _brush;
        public SolidColorBrush BrushColor
        {
            get => _brush ?? new SolidColorBrush(Colors.Bisque);
            set
            {
                _brush = value;
                OnPropertyChanged(nameof(BrushColor));
            }
        }
        //

        private List<BaseUserControlViewModel> UserControls { get; set; }

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
        

        public MainWindowViewModel()
        {
            Text = "hello";
            FontSize = 20;

            UserControls = new List<BaseUserControlViewModel>();

            DownloadsPage = new DownloadPageViewModel();
            SettingsPage = new SettingsViewModel();
            CurrentView = DownloadsPage;

            UserControls.Add(DownloadsPage);
            UserControls.Add(SettingsPage);

            ButtonCommand = new RelayCommand(p=>ChangeView((BaseUserControlViewModel)p), p => p is BaseUserControlViewModel);
            FontUpCommand = new RelayCommand(FontUp);

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

        private void FontUp([CanBeNull] object o)
        {
            FontSize+=2;
            BrushColor = new SolidColorBrush(Colors.Azure);
            Application.Current.Resources["DynamicColor"] = new SolidColorBrush(Colors.DarkRed);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
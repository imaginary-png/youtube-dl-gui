using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using youtube_dl_gui.Commands;
using youtube_dl_gui.Models;
using youtube_dl_gui_wrapper.Annotations;

namespace youtube_dl_gui.ViewModels
{
    public class SettingsViewModel : BaseUserControlViewModel
    {
        public string Text { get; set; }

        public string SettingsText
        {
            get => _settingsText;
            set
            {
                if (value == _settingsText) return;
                _settingsText = value;
                OnPropertyChanged(nameof(SettingsText));
            }
        }

        public SettingsManager SettingsManager { get; private set; }
        private string _settingsText;

        public ICommand SaveCommand { get; private set; }


        public SettingsViewModel()
        {
            SettingsManager = new SettingsManager();
            SaveCommand = new RelayCommand(p => SaveSettings_Execute(), null);
        }

        private void SaveSettings_Execute() => SettingsManager.SaveSettings();




    }
}
using System.Windows.Forms;
using System.Windows.Input;
using youtube_dl_gui.Commands;
using youtube_dl_gui.Models;

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
        public ICommand OpenDirectoryCommand { get; private set; }


        public SettingsViewModel()
        {
            SettingsManager = new SettingsManager();
            SaveCommand = new RelayCommand(p => SaveSettings_Execute(), null);
            OpenDirectoryCommand = new RelayCommand(p => OpenDirectory_Execute(), null);
        }

        //used for save settings button -- removed for more fluid design but kept in case.
        private void SaveSettings_Execute() => SettingsManager.SaveSettings();


        private void OpenDirectory_Execute()
        {
            //https://stackoverflow.com/questions/4007882/select-folder-dialog-wpf
            //have to use winforms FolderBrowseDialog because for some reason, after 10+ years
            //wpf still doesn't have a folder directory selection dialog.

            using var dialog = new FolderBrowserDialog
            {
                Description = "Select a folder",
                UseDescriptionForTitle = true,
                /*SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                               + Path.DirectorySeparatorChar,*/
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SettingsManager.UserSettings.OutputFolder = dialog.SelectedPath + "\\";
            }
        }
    }
}

/* xaml for old save button.
 *    <Button Grid.Row="6" 
                    Grid.Column="1"
                    VerticalAlignment="Top"
                    HorizontalAlignment="Right"
                    Margin="0 80 0 0"
                    Width="50"
                    Height="30"
                    Command="{Binding SaveCommand}">
                <TextBlock HorizontalAlignment="Center"
                           VerticalAlignment="Center">Save</TextBlock>

                <Button.Resources>
                    <Style TargetType="{x:Type Border}">
                        <Setter Property="CornerRadius" Value="4 4 4 4"/>
                    </Style>
                </Button.Resources>
            </Button>
 */
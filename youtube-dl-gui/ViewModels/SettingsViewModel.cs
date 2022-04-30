namespace youtube_dl_gui.ViewModels
{
    public class SettingsViewModel : BaseUserControlViewModel   
    {
        public string Text { get; set; }

        public SettingsViewModel()
        {
            Text = "This is the Settings Page!";
        }
    }
}
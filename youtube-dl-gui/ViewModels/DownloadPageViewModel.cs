namespace youtube_dl_gui.ViewModels
{
    public class DownloadPageViewModel : BaseUserControlViewModel
    {
        public string Text { get; set; }

        public DownloadPageViewModel()
        {
            Text = "This is the Download Page!";
        }
    }
}
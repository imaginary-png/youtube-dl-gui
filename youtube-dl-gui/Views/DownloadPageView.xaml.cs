using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace youtube_dl_gui.Views
{
    /// <summary>
    /// Interaction logic for DownloadPageView.xaml
    /// </summary>
    public partial class DownloadPageView : UserControl
    {

        public DownloadPageView()
        {
            InitializeComponent();
            var tb = InputTextBox;
            DataObject.AddPastingHandler(tb, OnPaste);
        }

        //add a space to the input text, if pasted.
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;

            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;

            if (text == null) return;
            if (text.EndsWith(" ")) return;
            text += " ";
            DataObject d = new DataObject();
            d.SetData(DataFormats.Text, text);
            e.DataObject = d;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var dg = VideoSourceDataGrid;
            dg.UnselectAll();
        }

        private void VideoSourceDataGrid_OnMouseMove(object sender, MouseEventArgs e)
        {
            var dg = VideoSourceDataGrid;
            dg.UnselectAll();
        }
    }
}

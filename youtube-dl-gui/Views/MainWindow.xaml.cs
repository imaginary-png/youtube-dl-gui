using System.Collections;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace youtube_dl_gui.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _sidePanelFocusColour = "#202225";
        private string _sidePanelUnfocusColour = "#2f3136";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximizebutton_OnClick(object sender, RoutedEventArgs e)
        {
            AdjustSize();
        }


        private void AdjustSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DownloadPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsPageSelectedBar.Visibility = Visibility.Hidden;
            DownloadPageSelectedBar.Visibility = Visibility.Visible;
        }

        private void SettingsPageButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadPageSelectedBar.Visibility = Visibility.Hidden;
            SettingsPageSelectedBar.Visibility = Visibility.Visible;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.FileProperties;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaCenter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer dtimer = new DispatcherTimer();
        public static Frame TransitionFrame;
        public static double width,height;
        public MainPage()
        {
            dtimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            dtimer.Tick += dtimer_Tick;
            width = Window.Current.Bounds.Width;
            height = Window.Current.Bounds.Height;
            this.InitializeComponent();
            TransitionFrame = App.rootFrame;
        }

        void dtimer_Tick(object sender, object e)
        {
            if (Constants.loadingactivated)
                    LoadingBorder.Visibility = Visibility.Visible;
                else
                    LoadingBorder.Visibility = Visibility.Collapsed;
                LoadingText.Text = Constants.loadingtext;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (Window.Current.Bounds.Width < MainPage.width)
            {
                snapped.Visibility = Visibility.Visible;
            }
            else
                if (Window.Current.Bounds.Width == MainPage.width)
                {
                    snapped.Visibility = Visibility.Collapsed;
                }

        }
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            dtimer.Start();
        }
        private void AudioPlayer()
        {
            dtimer.Stop();
            TransitionFrame.Navigate(typeof(BlankPage1));
        }
        private void VideoPlayer()
        {
            dtimer.Stop();
            TransitionFrame.Navigate(typeof(BlankPage2));
        }
        private void BorderPointerTapped(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name == "AudioBorder")
                AudioPlayer();
            else if ((sender as Button).Name == "VideoBorder")
                VideoPlayer();
        }

    }
}
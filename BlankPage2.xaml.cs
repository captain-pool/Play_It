using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;
using Windows.UI.Notifications;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.Storage.FileProperties;
using System.Diagnostics;
using Windows.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MediaCenter;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
//Video Player
namespace MediaCenter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage2 : Page
    {
        DisplayRequest drequest;
        double originalheight;
        double originalwidth;
        bool sliderpressed, isdrequestsent = false;
        DispatcherTimer timer;
        DispatcherTimer dtimer, volumetimer, volumeminustimer, forwardtimer, backwardtimer;
        MessageDialog mdialog;
        Constants constants;
        static BlankPage2 curr;
        StorageFile sfile;
        VideoProperties vproperties;
        bool isplaying = false, ispaused = false;
        public BlankPage2()
        {
            curr = this;
            constants = new Constants();
            drequest = new DisplayRequest();
            dtimer = new DispatcherTimer();
            volumetimer = new DispatcherTimer();
            volumeminustimer = new DispatcherTimer();
            volumeminustimer.Tick += volumeminustimer_Tick;
            forwardtimer = new DispatcherTimer();
            forwardtimer.Tick += forwardtimer_Tick;
            backwardtimer = new DispatcherTimer();
            backwardtimer.Tick += backwardtimer_Tick;
            volumetimer.Interval = TimeSpan.FromSeconds(3);
            dtimer.Interval = TimeSpan.FromMilliseconds(200);
            volumeminustimer.Interval = TimeSpan.FromSeconds(3);
            forwardtimer.Interval = TimeSpan.FromSeconds(3);
            backwardtimer.Interval = TimeSpan.FromSeconds(3);
            this.InitializeComponent();
            timer = new DispatcherTimer();
            dtimer.Tick += dtimer_Tick;
            volume.Value = 50;
            Window.Current.SizeChanged += Current_SizeChanged;
            Window.Current.VisibilityChanged += Current_VisibilityChanged;
        }

        void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            if (e.Visible)
                Page_GotFocus_1(new object(), new RoutedEventArgs());
            else
                Page_LostFocus_1(new object(), new RoutedEventArgs());
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (Window.Current.Bounds.Width < MainPage.width)
            {
                snapped.Visibility = Visibility.Visible;
                if (isplaying)
                    PlayPause_Click_1(new object(), new RoutedEventArgs());
            }
            else
                if (Window.Current.Bounds.Width == MainPage.width)
                {
                    snapped.Visibility = Visibility.Collapsed;
                    if (ispaused)
                        PlayPause_Click_1(new object(), new RoutedEventArgs());
                }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        /// 
        bool isnavigationplay = false;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Constants.invideoplayer = true;
            if (Constants.isvideofileactivated)
                InitializeHandles();
            if (e.Parameter != null)
            {
                try
                {
                    sfile = ((List<StorageFile>)e.Parameter)[0];
                    isnavigationplay = true;
                    load(sfile);
                    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                    if (isplaying)
                        Stop_Click_1(new object(), new RoutedEventArgs());
                    isplaying = false;
                    playpause();
                    fullscreentoggle();
                    media.AutoPlay = true;
                    isnavigationplay = true;
                }
                catch (Exception ex)
                {
                    NotifyUser(ex);
                }
            }
        }
        private void InitializeHandles()
        {
            FirstInstanceProperty.VideoPlayer.count++;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.StopPressed += MediaControl_StopPressed;
        }

        private void volume_ValueChanged_1(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (media.IsMuted)
                Mute_Click_1(sender, e);
            media.Volume = (e.NewValue / 100);
        }

        private void Mute_Click_1(object sender, RoutedEventArgs e)
        {
            if (!media.IsMuted)
            {
                Mute.Content = "";
                media.IsMuted = true;
            }
            else
            {
                Mute.Content = "";
                media.IsMuted = false;
            }
        }

        private void PlayPause_Click_1(object sender, RoutedEventArgs e)
        {
            playpause();
        }

        private void playpause()
        {
            if (!isplaying)
            {
                media.DefaultPlaybackRate = 1.0;
                try
                {
                    drequest.RequestActive();
                    isdrequestsent = true;
                    media.Play();
                }
                catch (Exception ex)
                {
                    NotifyUser(ex);
                }
                isplaying = true;
                endposition.Text = ((media.NaturalDuration.TimeSpan.Hours < 10) ? ("0" + media.NaturalDuration.TimeSpan.Hours.ToString()) : media.NaturalDuration.TimeSpan.Hours.ToString()) + ":" + ((media.NaturalDuration.TimeSpan.Minutes < 10) ? ("0" + media.NaturalDuration.TimeSpan.Minutes.ToString()) : (media.NaturalDuration.TimeSpan.Minutes.ToString())) + ":" + ((media.NaturalDuration.TimeSpan.Seconds < 10) ? ("0" + media.NaturalDuration.TimeSpan.Seconds.ToString()) : (media.NaturalDuration.TimeSpan.Seconds.ToString()));
                timer.Tick += timer_Tick;
                timer.Start();
                ispaused = false;
                PlayPause.Content = (char)0xE103;
                Stop.IsEnabled = true;
                MediaControl.IsPlaying = isplaying;
                MediaControl.TrackName = sfile.DisplayName;
            }
            else
            {
                try
                {
                    media.Pause();
                    isplaying = false;
                    ispaused = true;
                    PlayPause.Content = (char)0xE102;
                }
                catch (Exception ex)
                {
                    NotifyUser(ex);
                }
                MediaControl.IsPlaying = isplaying;
                MediaControl.TrackName = sfile.DisplayName;
            }
            UpdateBadge(isplaying, ispaused);
            UpdateTile(sfile);
        }
        void UpdateTile(StorageFile sfile)
        {
            try
            {
                string videoname = sfile.DisplayName;
                string xmlstring = "<tile>"
                    + "<visual>"
                    + "<binding template=\"TileWideText09\">"
                    + "<text id=\"1\">" + "Video Player" + "</text>"
                    + "<text id=\"2\">" + videoname + "</text>"
                    + "</binding>"
                    + "</visual>"
                    + "</tile>";
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xmlstring);
                TileNotification tile = new TileNotification(xmldoc);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tile);
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
        }
        private async void NotifyUser(Exception e)
        {
            mdialog = new MessageDialog(e.Message, "Error!");
            await mdialog.ShowAsync();
        }
        private void Stop_Click_1(object sender, RoutedEventArgs e)
        {
            isplaying = false;
            seekbar.Value = 0.0;
            try
            {
                media.Stop();
                MediaControl.IsPlaying = false;
                ispaused = false;
                timer.Stop();
                currentposition.Text = "00:00:00";
                MediaControl.TrackName = sfile.DisplayName;
                PlayPause.Content = (char)0xE102;
                Stop.IsEnabled = false;
                if (isdrequestsent)
                    drequest.RequestRelease();
                UpdateBadge(false, false);
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
        }

        private void seekbar_PointerPressed_1(object sender, PointerRoutedEventArgs e)
        {
            sliderpressed = true;
        }
        BitmapImage bitimage = new BitmapImage();

        private void seekbar_PointerReleased_1(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                media.Position = TimeSpan.FromSeconds(seekbar.Value);
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
        }

        private void Back_Click_1(object sender, RoutedEventArgs e)
        {
            int j = 0;
            try
            {
                if (isplaying)
                    media.Stop();
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
            try
            {
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                DispatcherTimer dtimer = new DispatcherTimer();
                dtimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
                Constants.loadingactivated = false;
                dtimer.Start();
                dtimer.Tick += (args1, args2) =>
                {
                    if (j == 0)
                    {
                        Windows.UI.Xaml.Controls.Grid ExitingFrame = new Windows.UI.Xaml.Controls.Grid();
                        ExitingFrame.Margin = new Thickness(0, 0, 0, 0);
                        ExitingFrame.Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
                        ProgressRing pring = new ProgressRing();
                        pring.IsActive = true;
                        pring.Margin = new Thickness(589, 338, 571, 281);
                        pring.Height = 149;
                        pring.Width = 206;
                        pring.Foreground = new SolidColorBrush(Colors.White);
                        TextBlock pleasewait = new TextBlock();
                        pleasewait.Text = "Please Wait . . .";
                        pleasewait.FontSize = 30;
                        pleasewait.Margin = new Thickness(589, 500, 571, 181);
                        pleasewait.Foreground = new SolidColorBrush(Colors.White);
                        ExitingFrame.Children.Add(pleasewait);
                        ExitingFrame.Children.Add(pring);
                        MainGrid.Children.Add(ExitingFrame);
                        MainGrid.UpdateLayout();
                        j++;
                    }
                    else
                        dtimer.Stop();
                    if (!dtimer.IsEnabled)
                    {
                        Constants.invideoplayer = false;
                        FirstInstanceProperty.VideoPlayer.count = 0;
                        RemoveHandles();
                        MainPage.TransitionFrame.Navigate(typeof(MainPage));
                        Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                        Windows.UI.Notifications.BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
                    }
                };
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }

        }
        public static void RemoveHandles()
        {
            MediaControl.PlayPauseTogglePressed -= MediaControl_PlayPauseTogglePressed;
            MediaControl.StopPressed -= MediaControl_StopPressed;
            MediaControl.PlayPressed -= MediaControl_PlayPressed;
            MediaControl.PausePressed -= MediaControl_PausePressed;
            if (curr.isdrequestsent)
                curr.drequest.RequestRelease();
        }
        private void togglefullscreen_Click_1(object sender, RoutedEventArgs e)
        {
            fullscreentoggle();
        }
        int value = 0;
        bool isfullscreen = false;
        void fullscreentoggle(int val = -1)
        {
            try
            {
                //Since there are 2 return keys on the keyboard 
                //the following mechanism is used to take input 
                //of only one
                if (val > 0)
                {
                    value = 0;
                    return;
                }
                if (!isfullscreen)
                {
                    ControlBox.Visibility = Visibility.Collapsed;
                    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Custom, 101);
                    KeyShortcutGrid.Visibility = Visibility.Collapsed;
                    media.Width = Window.Current.Bounds.Width;
                    media.Height = Window.Current.Bounds.Height;
                    isfullscreen = true;
                    togglefullscreen.Content = (char)0xE1D8;
                    constants.Text = "Exit FullScreen Mode";
                    //gstop2.Color = Color.FromArgb(127, 0, 0, 0);
                    gstop1.Color = Color.FromArgb(127, 19, 18, 18);
                    pointerentered = false;
                }
                else
                {
                    isfullscreen = false;
                    togglefullscreen.Content = (char)0xE1D9;
                    constants.Text = "Enter FullScreen Mode";
                    ControlBox.Visibility = Visibility.Visible;
                    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                    KeyShortcutGrid.Visibility = Visibility.Visible;
                    media.Width = originalwidth;
                    media.Height = originalheight;
                    //gstop2.Color = Color.FromArgb(255, 15, 15, 15); 
                    gstop1.Color = Color.FromArgb(255, 19, 18, 18);
                }
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
        }

        private void media_DoubleTapped_1(object sender, DoubleTappedRoutedEventArgs e)
        {
            fullscreentoggle();
        }

        private void Page_KeyDown_1(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                if (e.Key == Windows.System.VirtualKey.Space)
                {
                    PlayPause_Click_1(sender, e);
                }
                else if (e.Key == Windows.System.VirtualKey.Enter)
                    fullscreentoggle(value++);
                else if (e.Key == Windows.System.VirtualKey.Back)
                {
                    MessageDialog md = new MessageDialog("Exiting Video Player... Continue?", "Video Player");
                    md.Commands.Add(new UICommand("Yes", (command) =>
                        {
                            Back_Click_1(new object(), new RoutedEventArgs());
                        }));
                    md.Commands.Add(new UICommand("No"));
                    md.DefaultCommandIndex = 1;
                    md.ShowAsync();
                }
                else if (e.Key == Windows.System.VirtualKey.Up)
                {
                    if (volume.Value < 100)
                        volume1();
                }
                else if (e.Key == Windows.System.VirtualKey.Down)
                {
                    if (volume.Value > 0)
                        volumeminus1();
                }
                else if (e.Key == Windows.System.VirtualKey.Right)
                {
                    if ((seekbar.Value + 5) < media.NaturalDuration.TimeSpan.TotalSeconds)
                        forward5();
                }
                else if (e.Key == Windows.System.VirtualKey.Left)
                    if ((seekbar.Value - 5) > 0)
                        backward5();
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
        }

        private void volumeminus1()
        {
            volume.Value -= 1;
            keyshortcuts.Text = "Volume - 1 (" + volume.Value + "/100)";
            KeyShortcutGrid.Visibility = Visibility.Visible;
            volumeminustimer.Start();
        }

        void volumeminustimer_Tick(object sender, object e)
        {
            keyshortcuts.Text = name;
            if (isfullscreen)
                KeyShortcutGrid.Visibility = Visibility.Collapsed;
            volumeminustimer.Stop();
        }

        private void volume1()
        {
            volume.Value += 1;
            KeyShortcutGrid.Visibility = Visibility.Visible;
            keyshortcuts.Text = "Volume + 1 (" + volume.Value + "/100)";
            volumetimer.Tick += volumetimer_Tick;
            volumetimer.Start();
        }

        void volumetimer_Tick(object sender, object e)
        {
            keyshortcuts.Text = name;
            if (isfullscreen)
                KeyShortcutGrid.Visibility = Visibility.Collapsed;
            volumetimer.Stop();
        }

        private void backward5()
        {
            try
            {
                media.Position = TimeSpan.FromSeconds((seekbar.Value - 5));
                KeyShortcutGrid.Visibility = Visibility.Visible;
                keyshortcuts.Text = " 5 Seconds Backward (" + currentposition.Text + "/" + endposition.Text + ")";
                backwardtimer.Start();
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
        }

        void backwardtimer_Tick(object sender, object e)
        {
            keyshortcuts.Text = name;
            if (isfullscreen)
                KeyShortcutGrid.Visibility = Visibility.Collapsed;
            backwardtimer.Stop();
        }

        private void forward5()
        {
            media.Position = TimeSpan.FromSeconds((seekbar.Value + 5));
            KeyShortcutGrid.Visibility = Visibility.Visible;
            keyshortcuts.Text = "5 Seconds Forward (" + currentposition.Text + "/" + endposition.Text + ")";
            forwardtimer.Start();
        }
        void forwardtimer_Tick(object sender, object e)
        {
            keyshortcuts.Text = name;
            if (isfullscreen)
                KeyShortcutGrid.Visibility = Visibility.Collapsed;
            forwardtimer.Stop();
        }
        bool visible = false, pointerentered = false, playpausevisible = false;
        private void ControlBox_Pointer_Moved(object sender, PointerRoutedEventArgs e)
        {
            if (isfullscreen)
            {
                ControlBox.Visibility = Visibility.Visible;
                KeyShortcutGrid.Visibility = Visibility.Visible;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                visible = true;
                dtimer.Start();
            }
            PlayPause.Visibility = Visibility.Visible;
            playpausevisible = true;
            if (!dtimer.IsEnabled)
                dtimer.Start();
        }

        void dtimer_Tick(object sender, object e)
        {

            if (playpausevisible && (!playpausepointerentered))
            {
                playpausevisible = false;
                PlayPause.Visibility = Visibility.Collapsed;
                if (dtimer.IsEnabled)
                    dtimer.Stop();
                if ((visible == true) && (pointerentered == false) && (isfullscreen == true))
                {
                    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Custom, 101);
                    ControlBox.Visibility = Visibility.Collapsed;
                    KeyShortcutGrid.Visibility = Visibility.Collapsed;
                    visible = false;
                }
            }
        }

        private void OpenFile_Click_1(object sender, RoutedEventArgs e)
        {
            open();
        }
        async void open()
        {
            try
            {
                FileOpenPicker fop = new FileOpenPicker();
                fop.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                fop.FileTypeFilter.Add(".mp4");
                fop.FileTypeFilter.Add(".wmv");
                fop.FileTypeFilter.Add(".avi");
                StorageFile sf = await fop.PickSingleFileAsync();
                if (sf != null)
                {
                    load(sf);
                    if (isnavigationplay)
                    {
                        media.AutoPlay = false;
                        isnavigationplay = false;
                        Stop_Click_1(new object(), new RoutedEventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }

        }
        async void load(StorageFile sf)
        {
            try
            {
                if (sf != null)
                {
                    if (media.Source != null)
                    {
                        isplaying = false;
                        seekbar.Value = 0.0;
                        try
                        {
                            Stop_Click_1(new object(), new RoutedEventArgs());
                        }
                        catch (Exception ex)
                        {
                            NotifyUser(ex);
                        }
                    }
                    loading.IsActive = true;
                    IRandomAccessStream ir = await sf.OpenAsync(FileAccessMode.Read);
                    media.SetSource(ir, sf.ContentType);
                    PlayPause.IsEnabled = true;
                    PlayPause.Content = (char)0xE102;
                    Stop.IsEnabled = false;
                    isplaying = false;
                    originalheight = 652;
                    originalwidth = 1360;
                    name = sf.DisplayName;
                    UpdateTile(sf);
                    sfile = sf;
                }
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
        }

        private async void media_MediaOpened_1(object sender, RoutedEventArgs e)
        {
            try
            {
                endposition.Text = ((media.NaturalDuration.TimeSpan.Hours < 10) ? ("0" + media.NaturalDuration.TimeSpan.Hours.ToString()) : media.NaturalDuration.TimeSpan.Hours.ToString()) + ":" + ((media.NaturalDuration.TimeSpan.Minutes < 10) ? ("0" + media.NaturalDuration.TimeSpan.Minutes.ToString()) : (media.NaturalDuration.TimeSpan.Minutes.ToString())) + ":" + ((media.NaturalDuration.TimeSpan.Seconds < 10) ? ("0" + media.NaturalDuration.TimeSpan.Seconds.ToString()) : (media.NaturalDuration.TimeSpan.Seconds.ToString()));
                vproperties = await sfile.Properties.GetVideoPropertiesAsync();
                if (FirstInstanceProperty.VideoPlayer.count == 0 && !Constants.isvideofileactivated)
                    InitializeHandles();
                MediaControl.AlbumArt = new Uri("ms-appx:///Assets/video.jpg");
                MediaControl.ArtistName = vproperties.Publisher.ToString();
                setuptimer();
                seekbar.Maximum = Math.Round(media.NaturalDuration.TimeSpan.TotalSeconds, MidpointRounding.AwayFromZero);
                seekbar.StepFrequency = CalculateStepFrequency(media.NaturalDuration.TimeSpan);
                PlayPause.IsEnabled = true;
                PlayPause.Visibility = Visibility.Collapsed;
                Mute.Visibility = Visibility.Visible;
                seekbar.IsEnabled = true;
                togglefullscreen.IsEnabled = true; ;
                keyshortcuts.Text = name;
                MediaControl.IsPlaying = isplaying;
                MediaControl.TrackName = sfile.DisplayName;
                loading.IsActive = false;
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
        }

        async static void MediaControl_PausePressed(object sender, object e)
        {
            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    curr.playpause();
                    MediaControl.IsPlaying = curr.isplaying;
                    MediaControl.TrackName = curr.sfile.DisplayName;
                }
            );
        }

        async static void MediaControl_PlayPressed(object sender, object e)
        {
            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    curr.playpause();
                    MediaControl.IsPlaying = curr.isplaying;
                    MediaControl.TrackName = curr.sfile.DisplayName;
                }
            );
        }

        async static void MediaControl_StopPressed(object sender, object e)
        {

            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (curr.isplaying || curr.ispaused)
                    {
                        curr.Stop_Click_1(sender, (RoutedEventArgs)e);
                        MediaControl.IsPlaying = false;
                    }
                }
            );

        }

        async static void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    curr.playpause();
                    MediaControl.IsPlaying = curr.isplaying;
                    MediaControl.TrackName = curr.sfile.DisplayName;

                }
            );
        }
        private double CalculateStepFrequency(TimeSpan timevalue)
        {
            double stepFrequency;
            stepFrequency = seekbar.Maximum / timevalue.TotalSeconds;
            return stepFrequency;
        }
        private void media_MediaEnded_1(object sender, RoutedEventArgs e)
        {
            seekbar.Value = 0.0;
            timer.Tick -= timer_Tick;
            media.Position = new TimeSpan(0, 0, 0, 0, 0);
            endposition.Text = "";
            Stop_Click_1(sender, e);
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            ControlBox.Visibility = Visibility.Visible;
            KeyShortcutGrid.Visibility = Visibility.Visible;
            PlayPause.Visibility = Visibility.Visible;
        }
        void setuptimer()
        {
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, object e)
        {
            currentposition.Text = ((media.Position.Hours < 10) ? ("0" + media.Position.Hours.ToString()) : media.Position.Hours.ToString()) + ":" + ((media.Position.Minutes < 10) ? ("0" + media.Position.Minutes.ToString()) : (media.Position.Minutes.ToString())) + ":" + ((media.Position.Seconds < 10) ? ("0" + media.Position.Seconds.ToString()) : (media.Position.Seconds.ToString()));
            if (sliderpressed == false)
                seekbar.Value = media.Position.TotalSeconds;
        }

        private void ControlBox_PointerEntered_1(object sender, PointerRoutedEventArgs e)
        {
            pointerentered = true;
        }

        private void ControlBox_PointerExited_1(object sender, PointerRoutedEventArgs e)
        {
            pointerentered = false;
        }

        private void seekbar_PointerEntered_1(object sender, PointerRoutedEventArgs e)
        {
            sliderpressed = true;
        }

        private void seekbar_PointerExited_1(object sender, PointerRoutedEventArgs e)
        {
            sliderpressed = false;
        }
        private void UpdateBadge(bool playing, bool paused)
        {
            try
            {
                string xmlstring;
                if (playing && !paused)
                    xmlstring = "<badge value=\"playing\"/>";
                else
                    if (paused)
                        xmlstring = "<badge value=\"paused\"/>";
                    else
                        xmlstring = "<badge value=\"stopped\"/>";
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(xmlstring);
                BadgeNotification bnotification = new BadgeNotification(xdoc);
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(bnotification);
            }
            catch (Exception ex)
            {
                NotifyUser(ex);
            }
        }

        private async void media_MediaFailed_1(object sender, ExceptionRoutedEventArgs e)
        {
            mdialog = new MessageDialog(e.ErrorMessage, "Error");
            await mdialog.ShowAsync();
        }
        bool lostfocusplaying = false;
        private void Page_LostFocus_1(object sender, RoutedEventArgs e)
        {
            if (isplaying)
            {
                media.Pause();
                lostfocusplaying = true;
            }
        }

        private void Page_GotFocus_1(object sender, RoutedEventArgs e)
        {
            if (lostfocusplaying)
            {
                media.Play();
                lostfocusplaying = false;
            }
        }
        bool playpausepointerentered = false;
        private void PlayPause_PointerEntered_1(object sender, PointerRoutedEventArgs e)
        {
            playpausepointerentered = true;
        }

        private void PlayPause_PointerExited_1(object sender, PointerRoutedEventArgs e)
        {
            playpausepointerentered = false;
        }

        internal string name = string.Empty;
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using System.Diagnostics;
using Windows.System;
using Windows.UI.Popups;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml;
using Windows.UI.Notifications;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Media;
using Windows.Media.Playlists;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.FileProperties;
using MediaCenter;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
//Audio Player

namespace MediaCenter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage1 : Page
    {
        #region global variables
        MessageDialog mdialog;
        static BlankPage1 curr;
        MusicProperties mproperties;
        internal List<StorageFile> sfile = new List<StorageFile>();
        bool isplaying = false, playlistended = false, ingridfocus = true, isplaylistused = false;
        int i = 0;
        Playlist playlist = new Playlist();
        StorageItemThumbnail thumbn = null;
        BitmapImage bimage = new BitmapImage();
        string mp3name = "";
        internal static BlankPage1 obj = new BlankPage1();
        StorageFile Playlistname;
        DispatcherTimer timer;
        object CurrentListViewObject = null;
        bool sliderpressed = false, ispaused = false;
        Dictionary<string, string> timelengths = new Dictionary<string, string>();
        #endregion
        public BlankPage1()
        {
            this.InitializeComponent();
            curr = this;
            timer = new DispatcherTimer();
            Play_text.Text = "Create Playlist";
            slider.Value = 50;
            random.Visibility = Visibility.Collapsed;
            backward.IsEnabled=false;
            forward.IsEnabled=false;
            Window.Current.SizeChanged += Current_SizeChanged;
            Window.Current.VisibilityChanged += Current_VisibilityChanged;
        }
        bool infocus = false;
        void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            infocus = e.Visible;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (Window.Current.Bounds.Width < MainPage.width)
            {
                snapped.Visibility = Visibility.Visible;
                if (isplaying)
                    playpause();
            }
            else
                if (Window.Current.Bounds.Width == MainPage.width)
                {
                    snapped.Visibility = Visibility.Collapsed;
                    if (ispaused)
                        playpause();
                }
        }
        #region functions
        async static void MediaControl_PreviousTrackPressed(object sender, object e)
        {
            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (curr.backward.IsEnabled)
                {
                    curr.prev();
                }
            });
        }

        async static void MediaControl_NextTrackPressed(object sender, object e)
        {
            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (curr.forward.IsEnabled)
                    curr.next();
            });
        }

        async static void MediaControl_StopPressed(object sender, object e)
        {
            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (curr.isplaying || curr.ispaused)
                {
                    curr.stop_Click_1(sender, (RoutedEventArgs)e);
                    MediaControl.IsPlaying = curr.isplaying;
                }
            }
            );
        }

        async static void MediaControl_PausePressed(object sender, object e)
        {
            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaControl.IsPlaying = curr.isplaying;
            });
        }

        async static void MediaControl_PlayPressed(object sender, object e)
        {
            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaControl.IsPlaying = curr.isplaying;
            });
        }

        async static void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            await curr.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                curr.PlayPauseButton_Click_1(sender, (RoutedEventArgs)e);
                MediaControl.IsPlaying = curr.isplaying;
            }
            );
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        /// 

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Constants.ismusicfileactivated)
            {
                InitializeHandles();
            }
            LoadingBorder.Visibility = Visibility.Collapsed;
            try
            {
                if (e.Parameter != null)
                {
                    List<StorageFile> music = new List<StorageFile>();
                    Constants.inmusicplayer = true;
                    object[] array = (object[])e.Parameter;
                    List<StorageFile> incomingplaylist = new List<StorageFile>();
                    LoadingBorder.Visibility = Visibility.Visible;
                    if (((List<StorageFile>)array[1]).Count > 0)
                    {
                        music = (List<StorageFile>)array[1];
                        LoadingText.Text = (music.Count) == 1 ? "Loading Music" : "Loading Music Files";
                        sfile.Clear();
                        if (music.Count > 0)
                        {
                            replace(music.ToArray(), ref sfile);
                            load(sfile,(lvd!=null)?lvd.File:null);
                            fileopen();
                        }
                    }
                    if (((List<StorageFile>)array[0]).Count > 0)
                    {
                        LoadingText.Text = "Loading Playlist";
                        foreach (StorageFile s in ((List<StorageFile>)array[0]))
                        {
                            try
                            {
                                incomingplaylist.Add(s);
                            }
                            catch (Exception ex)
                            {
                               NotifyUser(ex.Message);
                            }
                        }
                        play = incomingplaylist.ToArray();
                        playlist = await Playlist.LoadAsync(play[play.Length - 1]);
                        openedplaylist = play[play.Length - 1];
                        isplaylistused = true;
                        sfile.Clear();
                        this.i = 0;
                        foreach (StorageFile sf in playlist.Files)
                        {
                            try
                            {
                                sfile.Add(sf);
                            }
                            catch (Exception ex)
                            {
                                NotifyUser(ex.Message);
                            }
                            this.i++;
                        }
                        load(sfile,(lvd!=null)?lvd.File:null);
                        if (sfile.Count == 1)
                        {
                            PlayPauseButton.Content = (char)0xE103;
                            stop.IsEnabled = true;
                            isplaying = true;
                            ispaused = false;
                        }
                        fileopen();
                        seek.IsEnabled = true;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                NotifyUser("Access Denied!");
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
            LoadingBorder.Visibility = Visibility.Collapsed;
        }

        private void replace(StorageFile[] music, ref List<StorageFile> sfile)
        {
            int i = 0;
            foreach (StorageFile mus in music)
            {
                try
                {
                    sfile.Add(mus);
                }
                catch (Exception ex)
                {
                    NotifyUser(ex.Message);
                }
                i++;
            }
        }

        private bool checkplaylistextension(StorageFile sf)
        {
            string[] playlistExtensions = new string[] { ".m3u", ".wpl", ".zpl" };
            foreach (string str in playlistExtensions)
            {
                if (sf.FileType.Equals(str, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private async void fileopen(bool shuffle=false)
        {
            i = 0;
            thumbgrid.Visibility = Visibility.Visible;
            if (!(shuffle && (isplaying || ispaused)))
            {
                stop_Click_1(new object(), new RoutedEventArgs());
                IRandomAccessStream ir = null;
                ir = await sfile[i].OpenAsync(FileAccessMode.Read);
                media.SetSource(ir, sfile[i].ContentType);
                playpause();
                PlayPauseButton.IsEnabled = true;
                mproperties = await sfile[i].Properties.GetMusicPropertiesAsync();
                thumbn = await sfile[i].GetThumbnailAsync(ThumbnailMode.SingleItem, 600, ThumbnailOptions.UseCurrentScale);
                if (thumbn != null)
                {
                    bimage.SetSource(thumbn);
                    thumbnail.Source = bimage;
                }
                seek.IsEnabled = true;
                media.AutoPlay = true;
                MediaControl.IsPlaying = isplaying;
            }
            CurrentListViewObject = lv.ItemsSource = ListViewData.LoadMusic(sfile, sfile[i]);
            No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
            lvd = ListViewData.CurrentObject;
            lv.ScrollIntoView(lvd);
            lv.UpdateLayout();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            open(true);
        }
        private async void NotifyUser(String e)
        {
            try
            {
                if (infocus)
                {

                    mdialog = new MessageDialog(e, "Error");
                    await mdialog.ShowAsync();
                }
                else
                    NotifyByToast2(e);
            }
            catch (Exception ex)
            {
                NotifyByToast(ex);
            }
        }

        private void NotifyByToast2(string e)
        {
            string xmlstring = "<toast>"
                           + "<visual version='1'>"
                           + "<binding template='ToastImageAndText02'>"
                           + "<text id='1'>Error Occured!</text>"
                           + "<text id='2'>" + e + "</text>"
                           + "<image id='1' src='Assets/music.png'/>"
                           + "</binding>"
                           + "</visual>"
                           + "<audio src='ms-winsoundevent:Notification.IM'/>"
                           + "</toast>";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlstring);
            ToastNotification toast = new ToastNotification(xmldoc);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        
        private void NotifyByToast(Exception ex)
        {
            string xmlstring = "<toast>"
                           + "<visual version='1'>"
                           + "<binding template='ToastImageAndText02'>"
                           + "<text id='1'>Now This Is Embarassing!</text>"
                           + "<text id='2'>" + ex.Message + "</text>"
                           + "<image id='1' src='Assets/music.png'/>"
                           + "<audio src='ms-winsoundevent:Notification.IM'/>"
                           + "</binding>"
                           + "</visual>"
                           + "</toast>";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlstring);
            ToastNotification toast = new ToastNotification(xmldoc);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        private void NotifyByToast3(string e)
        {
            string xmlstring = "<toast>"
                           + "<visual version='1'>"
                           + "<binding template='ToastImageAndText02'>"
                           + "<text id='1'>Information</text>"
                           + "<text id='2'>" + e + "</text>"
                           + "<image id='1' src='Assets/music.png'/>"
                           + "</binding>"
                           + "</visual>"
                           + "<audio src='ms-winsoundevent:Notification.IM'/>"
                           + "</toast>";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlstring);
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(xmldoc));
        }
        async void open(bool type)
        {
            try
            {
                i = 0;
                if (playlistview)
                    back_Click_1(new object(), new RoutedEventArgs());
                if(type)
                sfile.Clear();
                FileOpenPicker fop = new FileOpenPicker();
                fop.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                fop.FileTypeFilter.Add(".mp3");
                fop.FileTypeFilter.Add(".wma");
                fop.FileTypeFilter.Add(".aac");
                fop.FileTypeFilter.Add(".adt");
                fop.FileTypeFilter.Add(".adts");
                fop.FileTypeFilter.Add(".wav");
                int count = 0;
                LoadingBorder.Visibility = Visibility.Visible;
                IReadOnlyList<StorageFile> list = await fop.PickMultipleFilesAsync();
                if (list.Count > 0)
                {
                    foreach (StorageFile sf in list)
                    {
                        try
                        {
                            if (!type)
                            {
                                if(sfile.contains(sf))
                                count++;

                                else
                                    sfile.Add(sf);
                            }
                            else
                                sfile.Add(sf);
                        }
                        catch (Exception e)
                        {
                            NotifyUser(e.Message);
                        }
                        i++;
                    }
                    if (count > 1)
                        await new MessageDialog((count.ToString() + " Files are already present in the playlist"), "PlayIt-->MusicPlayer").ShowAsync();
                    else
                        if (count == 1)
                            await new MessageDialog("1 File Already Present so It's Avoided From Adding", "PlayIt-->MusicPlayer").ShowAsync();
                    isplaylistused = false;
                    if (lvd != null)
                        load(sfile, lvd.File);
                    else
                        load(sfile);
                }
                LoadingBorder.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }
        bool medialoaded = false;
        public void load(List<StorageFile> sfile,StorageFile file=null)
        {
            bool b = false;
            try
            {
                if (sfile.Count > 0)
                {
                    emptynote.Visibility = Visibility.Collapsed;
                    CurrentListViewObject = lv.ItemsSource = ListViewData.LoadMusic(sfile, file);
                    No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
                    lvd = ListViewData.CurrentObject;
                    if (!isplaylistused)
                    {
                        createplaylist.IsEnabled = true;
                        delete.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        delete.Visibility = Visibility.Visible;
                    }
                    if (file != null)
                    {
                        for (int j = 0; j < sfile.Count; j++)
                        {
                            if (sfile[j].Path.Equals(file.Path, StringComparison.OrdinalIgnoreCase))
                            {
                                i = j;
                                b = true;
                                break;
                            }
                        }
                        if (!b)
                        {
                            i = -1;
                            forward.IsEnabled = false;
                            backward.IsEnabled = false;
                        }
                        if (medialoaded && b)
                        {
                            if (sfile.Count <= 1)
                            {
                                backward.IsEnabled = false;
                                forward.IsEnabled = false;
                            }
                            else
                            {
                                backward.IsEnabled = true;
                                forward.IsEnabled = true;
                                if ((i + 1) >= sfile.Count)
                                    forward.IsEnabled = false;
                                if ((i - 1) < 0)
                                    backward.IsEnabled = false;
                            }
                        }
                    }
                    playlistwindowopen = false;
                }
                if (sfile.Count == 0)
                {
                    lv.Visibility = Visibility.Collapsed;
                }
                LoadingBorder.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }
        
        private async void prev()
        {
            try
            {
                if (sfile.Count > 1)
                {
                    if (i > 0)
                    {
                        i--;
                        IRandomAccessStream ir = await sfile[i].OpenAsync(FileAccessMode.Read);
                        thumbn = await sfile[i].GetThumbnailAsync(ThumbnailMode.SingleItem, 1000, ThumbnailOptions.ResizeThumbnail);
                        if (thumbn != null)
                        {
                            bimage.SetSource(thumbn);
                            thumbnail.Source = bimage;
                            if (toggleswitch.IsOn)
                                thumbgrid.Visibility = Visibility.Visible;
                        }
                        timer.Tick -= timer_Tick;
                        seek.Value = 0;
                        media.Position = new TimeSpan(0, 0, 0, 0, 0);
                        CurrentListViewObject = ListViewData.LoadMusic(sfile, sfile[i]);
                        lvd = ListViewData.CurrentObject;
                        if (!playlistwindowopen)
                        {
                            lv.ItemsSource = CurrentListViewObject;
                            No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
                            lv.UpdateLayout();
                        }
                        media.SetSource(ir, sfile[i].ContentType);
                        if (media.AutoPlay == false && isplaying)
                            media.AutoPlay = true;
                        mproperties = await sfile[i].Properties.GetMusicPropertiesAsync();
                        MediaControl.ArtistName = mproperties.Artist;
                        MediaControl.TrackName = mproperties.Title;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }

        private void PlayPauseButton_Click_1(object sender, RoutedEventArgs e)
        {
            playpause();
        }
        void playpause()
        {
            if (playlistended == false)
            {
                if (isplaying)
                {
                    media.Pause();
                    media.AutoPlay = false;
                    PlayPauseButton.Content = (char)0xE102;
                    isplaying = false;
                    ispaused = true;
                }
                else
                {
                    PlayPauseButton.Content = (char)0xE103;
                    media.Play();
                    media.AutoPlay = true;
                    stop.IsEnabled = true;
                    isplaying = true;
                    ispaused = false;
                }
                UpdateBadge(isplaying, ispaused);
                UpdateTile();
            }
            else
            {
                if(i-->=sfile.Count-1)
                i = -1;
                playlistended = false;
                next();
                playpause();
            }
        }
        bool playlistview = false;
        private async void lv_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            lv.ScrollIntoView(e.ClickedItem as ListViewData);
            if (!playlistview)
            {
                try
                {
                    IRandomAccessStream ir = null;
                    for (i = 0; i < this.sfile.Count; i++)
                    {
                        if (((ListViewData)e.ClickedItem).File.Path.Equals(sfile[i].Path,StringComparison.OrdinalIgnoreCase))
                        {
                            if (sfile.Count > 1 && isplaying == false && ispaused == false)
                            {
                                media.AutoPlay = false;
                                stop_Click_1(sender, args);
                            }
                            else
                                if (sfile.Count == 1)
                                {
                                    PlayPauseButton.Content = (char)0xE103;
                                    stop.IsEnabled = true;
                                    isplaying = true;
                                    ispaused = false;
                                }
                            CurrentListViewObject = lv.ItemsSource = ListViewData.LoadMusic(sfile, sfile[i]);
                            No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
                            lvd = ListViewData.CurrentObject;
                            ir = await sfile[i].OpenAsync(FileAccessMode.Read);
                            media.SetSource(ir, sfile[i].ContentType);
                            PlayPauseButton.IsEnabled = true;
                            seek.Value = 0;
                            thumbn = await sfile[i].GetThumbnailAsync(ThumbnailMode.SingleItem, 600, ThumbnailOptions.UseCurrentScale);
                            if (thumbn != null)
                            {
                                bimage.SetSource(thumbn);
                                thumbnail.Source = bimage;
                                if (toggleswitch.IsOn)
                                    thumbgrid.Visibility = Visibility.Visible;
                                if (sfile.Count == 1)
                                    media.Play();
                            }
                            seek.IsEnabled = true;
                            break;
                        }
                    }
                    lvd = (ListViewData)(e.ClickedItem);
                }
                catch (Exception ex)
                {
                    NotifyUser(ex.Message);
                }
            }
            else
            {
                loadplaylist(((ListViewData)e.ClickedItem).File);
            }
        }

        private async void loadplaylist(StorageFile storageFile)
        {
            try
            {
                for (int j = 0; j < play.Length; j++)
                {
                    if (storageFile.Path.Equals(play[j].Path,StringComparison.OrdinalIgnoreCase))
                    {
                        LoadingBorder.Visibility = Visibility.Visible;
                        playlist = await Playlist.LoadAsync(storageFile);
                        openedplaylist = storageFile;
                        isplaylistused = true;
                        LoadingBorder.Visibility = Visibility.Collapsed;
                    }
                }
                sfile.Clear();
                i = 0;
                foreach (StorageFile sf in playlist.Files)
                {
                    try
                    {
                        sfile.Add(sf);
                    }
                    catch (Exception ex)
                    {
                        NotifyUser(ex.Message);
                    }
                }
                grid2textbox.Text = "Loaded Music Files";
                playlistview = false;
                back.Visibility = Visibility.Collapsed;
                if (sfile.Count > 0)
                {
                    isplaylistused = true;
                    if (lvd == null)
                        load(sfile);
                    else
                        load(sfile, lvd.File);
                    emptynote.Visibility = Visibility.Collapsed;
                }
                else
                {
                    lv.Visibility = Visibility.Collapsed;
                    emptynote.Visibility = Visibility.Visible;
                    createplaylist.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                LoadingBorder.Visibility = Visibility.Collapsed;
                NotifyUser("Error Loading Playlist");
            }
        }

        StorageFile openedplaylist;
        private void slider_ValueChanged_1(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                if (media.IsMuted && e.NewValue > e.OldValue)
                    MuteUnMute_Click_1(sender, e);
                media.Volume = (e.NewValue / 100);
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }

        private void media_MediaEnded_1(object sender, RoutedEventArgs e)
        {
            albumart.Source = null;
            media.Position = new TimeSpan(0, 0, 0, 0, 0);
            currentposition.Text = "00:00:00";
            if (sfile.Count > 1 && i >= 0)
                next();
            else
                stop_Click_1(new object(), new RoutedEventArgs());
        }
        ListViewData lvd=null;
        public static void RemoveHandles()
        {
            MediaControl.PlayPauseTogglePressed -= BlankPage1.MediaControl_PlayPauseTogglePressed;
            MediaControl.PlayPressed -= BlankPage1.MediaControl_PlayPressed;
            MediaControl.PausePressed -= BlankPage1.MediaControl_PausePressed;
            MediaControl.StopPressed -= BlankPage1.MediaControl_StopPressed;
            MediaControl.NextTrackPressed -= BlankPage1.MediaControl_NextTrackPressed;
            MediaControl.PreviousTrackPressed -= BlankPage1.MediaControl_PreviousTrackPressed;
        }
        private void MuteUnMute_Click_1(object sender, RoutedEventArgs e)
        {
            if (!media.IsMuted)
            {
                MuteUnMute.Content = "";
                media.IsMuted = true;
            }
            else
            {
                MuteUnMute.Content = "";
                media.IsMuted = false;
            }
        }

        private void NavigateBack_Click_1(object sender, RoutedEventArgs e)
        {
            if (isplaying)
                stop_Click_1(new object(), new RoutedEventArgs());
            int j = 0;
            Constants.loadingactivated = false;
            DispatcherTimer dtimer = new DispatcherTimer();
            dtimer.Interval = new TimeSpan(0, 0, 0, 0, 5);
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
                    TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                    BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
                    Constants.inmusicplayer = false;
                    FirstInstanceProperty.MusicPlayer.count = 0;
                    MediaControl.PlayPauseTogglePressed -= MediaControl_PlayPauseTogglePressed;
                    MediaControl.PlayPressed -= MediaControl_PlayPressed;
                    MediaControl.PausePressed -= MediaControl_PausePressed;
                    MediaControl.StopPressed -= MediaControl_StopPressed;
                    MediaControl.NextTrackPressed -= MediaControl_NextTrackPressed;
                    MediaControl.PreviousTrackPressed -= MediaControl_PreviousTrackPressed;
                    MainPage.TransitionFrame.Navigate(typeof(MainPage));
                }
            };
        }

        private void seek_PointerPressed_1(object sender, PointerRoutedEventArgs e)
        {
            sliderpressed = true;
        }

        private async void media_MediaOpened_1(object sender, RoutedEventArgs e)
        {
            medialoaded = true;
            if (sfile.Count == 1)
                media.AudioCategory = AudioCategory.BackgroundCapableMedia;
            random.Visibility = Visibility.Visible;
            mproperties = await sfile[i].Properties.GetMusicPropertiesAsync();
            mp3name = mproperties.Title;
            if (!playlistwindowopen)
            {
                lv.ScrollIntoView(lvd);
                lv.UpdateLayout();
            }
            if (FirstInstanceProperty.MusicPlayer.count==0 && !Constants.ismusicfileactivated)
            {
                InitializeHandles();
            }
            setalbumarturi();
            Constants c = new Constants(mproperties);
            Details.DataContext = c;
            Details.UpdateLayout();
            if (sfile.Count <= 1)
            {
                backward.IsEnabled = false;
                forward.IsEnabled = false;
            }
            else
            {
                backward.IsEnabled = true;
                forward.IsEnabled = true;
                if ((i + 1) >= sfile.Count)
                {
                    forward.IsEnabled = false;
                }
                if ((i - 1) < 0)
                {
                    backward.IsEnabled = false;
                }
            }
            seek.Maximum = media.NaturalDuration.TimeSpan.TotalSeconds;
            seek.StepFrequency = CalculateSliderFrequency(media.NaturalDuration.TimeSpan);
            endposition.Text = ((media.NaturalDuration.TimeSpan.Hours < 10) ? ("0" + media.NaturalDuration.TimeSpan.Hours.ToString()) : media.NaturalDuration.TimeSpan.Hours.ToString()) + ":" + ((media.NaturalDuration.TimeSpan.Minutes < 10) ? ("0" + media.NaturalDuration.TimeSpan.Minutes.ToString()) : (media.NaturalDuration.TimeSpan.Minutes.ToString())) + ":" + ((media.NaturalDuration.TimeSpan.Seconds < 10) ? ("0" + media.NaturalDuration.TimeSpan.Seconds.ToString()) : (media.NaturalDuration.TimeSpan.Seconds.ToString()));
            setuptimer();
            UpdateTile();
            UpdateBadge(isplaying, ispaused);
            MediaControl.ArtistName = mproperties.Artist;
            MediaControl.TrackName = mproperties.Title;
        }

        private void InitializeHandles()
        {
            FirstInstanceProperty.MusicPlayer.count++;
                MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
                MediaControl.PlayPressed += MediaControl_PlayPressed;
                MediaControl.PausePressed += MediaControl_PausePressed;
                MediaControl.StopPressed += MediaControl_StopPressed;
                MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
                MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
                MediaControl.SoundLevelChanged += MediaControl_SoundLevelChanged;
        }

        async void MediaControl_SoundLevelChanged(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var state = MediaControl.SoundLevel;
                if (state == SoundLevel.Muted && isplaying)
                    media.Pause();
                else
                    if (isplaying)
                        media.Play();
            });
        }

        private async void setalbumarturi()
        {
            try
            {
                Uri cons = new Uri("ms-appdata:///local/ThumbnailFolderTemp/" + "AlbumArt.png");
                using (StorageItemThumbnail storageItemThumbnail = await sfile[i].GetThumbnailAsync(ThumbnailMode.SingleItem, 100, ThumbnailOptions.ResizeThumbnail))
                using (IInputStream inputStreamAt = storageItemThumbnail.GetInputStreamAt(0))
                using (var dataReader = new DataReader(inputStreamAt))
                {
                    uint u = await dataReader.LoadAsync((uint)storageItemThumbnail.Size);
                    IBuffer readBuffer = dataReader.ReadBuffer(u);
                    StorageFolder tempFolder = null;
                    try
                    {
                        tempFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("ThumbnailFolderTemp");
                    }
                    catch (Exception)
                    {
                    }
                    StorageFile imageFile = null;
                    if (tempFolder == null)
                        tempFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ThumbnailFolderTemp", CreationCollisionOption.ReplaceExisting);
                    try
                    {
                        imageFile = await tempFolder.GetFileAsync("AlbumArt.png");
                        await imageFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch (Exception)
                    { }
                    imageFile = await tempFolder.CreateFileAsync("AlbumArt.png", CreationCollisionOption.ReplaceExisting);
                    using (IRandomAccessStream randomAccessStream = await imageFile.OpenAsync(FileAccessMode.ReadWrite))
                    using (IOutputStream outputStreamAt = randomAccessStream.GetOutputStreamAt(0))
                    {
                        await outputStreamAt.WriteAsync(readBuffer);
                        await outputStreamAt.FlushAsync();
                    }
                }
                MediaControl.AlbumArt = cons;
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
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
                NotifyUser(ex.Message);
            }
        }
        private void seek_PointerReleased_1(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                media.Position = TimeSpan.FromSeconds(seek.Value);
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }
        void setuptimer()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 0, 0);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        private double CalculateSliderFrequency(TimeSpan timevalue)
        {
            double stepFrequency;
            stepFrequency = seek.Maximum/timevalue.TotalSeconds;
            return stepFrequency;
        }
        void timer_Tick(object sender, object e)
        {
            if (sliderpressed == false && isplaying)
            {
                seek.Value = media.Position.TotalSeconds;
            }
            currentposition.Text = ((media.Position.Hours < 10) ? ("0" + media.Position.Hours.ToString()) : media.Position.Hours.ToString()) + ":" + ((media.Position.Minutes < 10) ? ("0" + media.Position.Minutes.ToString()) : (media.Position.Minutes.ToString())) + ":" + ((media.Position.Seconds < 10) ? ("0" + media.Position.Seconds.ToString()) : (media.Position.Seconds.ToString()));
        }
        private void stop_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                seek.Value = 0.0;
                isplaying = false;
                ispaused = false;
                media.Stop();
                media.AutoPlay = false;
                PlayPauseButton.Content = (char)0xE102;
                stop.IsEnabled = false;
                UpdateBadge(false, false);
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }

        private void backward_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            prev();
        }

        private async void next()
        {
            object sender = new object();
            RoutedEventArgs e = new RoutedEventArgs();
            try
            {
                i++;
                if (sfile.Count > 1 && i < sfile.Count)
                {
                    IRandomAccessStream ir = await sfile[i].OpenAsync(FileAccessMode.Read);
                    thumbn = await sfile[i].GetThumbnailAsync(ThumbnailMode.SingleItem, 600, ThumbnailOptions.UseCurrentScale);
                    if (thumbn != null)
                    {
                        bimage.SetSource(thumbn);
                        if (toggleswitch.IsOn)
                            thumbgrid.Visibility = Visibility.Visible;
                        thumbnail.Source = bimage;
                    }
                    timer.Tick -= timer_Tick;
                    seek.Value = 0;
                    mproperties = await sfile[i].Properties.GetMusicPropertiesAsync();
                    MediaControl.ArtistName = mproperties.Artist;
                    MediaControl.TrackName = mproperties.Title;
                    CurrentListViewObject = ListViewData.LoadMusic(sfile, sfile[i]);
                    lvd = ListViewData.CurrentObject;
                    if (!playlistwindowopen)
                    {
                        lv.ItemsSource = CurrentListViewObject;
                        No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
                        lv.UpdateLayout();
                    }
                    media.SetSource(ir, sfile[i].ContentType);
                    if (media.AutoPlay == false && isplaying)
                        media.AutoPlay = true;
                }
                else
                {
                    i--;
                    stop_Click_1(sender, e);
                    playlistended = true;
                }
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }

        void UpdateTile()
        {
            try
            {
                string xmlstring = "<tile>"
                    + "<visual>"
                    + "<binding template=\"TileWideText09\">"
                    + "<text id=\"1\">" + "Music Player" + "</text>"
                    + "<text id=\"2\">" + mp3name + "</text>"
                    + "</binding>"
                    + "</visual>"
                    + "</tile>";
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xmlstring);
                TileNotification tile = new TileNotification(xmldoc);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tile);
            }
            catch (Exception)
            {
                NotifyUser("Error Occured While Updating Tile!");
            }
        }

        private void media_CurrentStateChanged_1(object sender, RoutedEventArgs e)
        {
        }

        private void seek_PointerEntered_1(object sender, PointerRoutedEventArgs e)
        {
            sliderpressed = true;
        }

        private void seek_PointerExited_1(object sender, PointerRoutedEventArgs e)
        {
            sliderpressed = false;
        }

        private void createplaylist_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sfile.Count > 0)
                {
                    playlist = new Playlist();
                    foreach (StorageFile storage in sfile)
                        playlist.Files.Add(storage);
                    Play_text.Text = "Create Playlist";
                    playlistgrid.Visibility = Visibility.Visible;
                    ingridfocus = false;
                    playlistname.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                }
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }
        StorageFile[] play;
        private RoutedEventArgs args = new RoutedEventArgs();
        bool playlistwindowopen = false;
        private async void OpenPlaylist_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                int pos = 0;
                StorageFolder sf = ApplicationData.Current.RoamingFolder;
                IReadOnlyList<StorageFile> ilist = await sf.GetFilesAsync();
                play = new StorageFile[ilist.Count];
                foreach (StorageFile storagefile in ilist)
                {
                    play[pos] = storagefile;
                    pos++;
                }
                grid2textbox.Text = "Available Playlists";
                playlistwindowopen = true;
                lv.Visibility = Visibility.Visible;
                lv.ItemsSource = ListViewData.LoadPlaylists(play);
                No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
                back.Visibility = Visibility.Visible;
                playlistview = true;
                emptynote.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }
        private void write_time_tofile()
        {

        }
        private void playlistname_KeyDown_1(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                string name = playlistname.Text;
                if (e.Key == Windows.System.VirtualKey.Enter)
                    save_Click_1(sender, e);
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }
        private async void save_Click_1(object sender, RoutedEventArgs e)
        {
            bool success = true;
            try
            {
                string name = playlistname.Text;
                if (InfoPage.Visibility == Visibility.Visible && PlaylistInfo.Visibility==Visibility.Visible)
                {
                     if (!name.Equals(""))
                    {
                        try
                        {
                            
                            await SelectedPlaylist.File.RenameAsync(name+".zpl", NameCollisionOption.FailIfExists);
                        }
                        catch (Exception)
                        {
                            success = false;
                            NotifyByToast3("Please use a different name");
                            playlistname.Text = string.Empty;
                        }
                        if (data != null)
                        {
                            data.FileName = playlistname.Text;
                        }
                        if (success)
                        {
                            OpenPlaylist_Click_1(new object(), new RoutedEventArgs());
                            playlistgrid.Visibility = Visibility.Collapsed;
                            InfoPage.Visibility = Visibility.Visible;
                            Play_text.Text = "Create Playlist";
                        }
                    }
                }
                else
                if (!name.Equals(""))
                {
                    Playlistname = null;
                    Playlistname = await playlist.SaveAsAsync(ApplicationData.Current.RoamingFolder, name, NameCollisionOption.ReplaceExisting, PlaylistFormat.Zune);
                    if (Playlistname != null)
                    {
                        playlistgrid.Visibility = Visibility.Collapsed;
                        ingridfocus = true;
                        playlistname.Text = "";
                    }
                }
                else
                    await new MessageDialog("Please Enter A Name Of The Playlist", "PlayIt->Music").ShowAsync();
            }
            catch (Exception ex)
            {
                NotifyUser(ex.Message);
            }
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            playlistname.Text = "";
            playlistgrid.Visibility = Visibility.Collapsed;
            ingridfocus = true;
        }

        private void Grid_PointerReleased_1(object sender, PointerRoutedEventArgs e)
        {
            if (!ingridfocus)
                Cancel_Click_1(new object(), new RoutedEventArgs());
        }

        private void back_Click_1(object sender, RoutedEventArgs e)
        {
            lv.ItemsSource=null;
            No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
            grid2textbox.Text = "Loaded Music Files";
            playlistview = false;
            back.Visibility = Visibility.Collapsed;
            if (CurrentListViewObject != null)
            {
                lv.ItemsSource = CurrentListViewObject;
                No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
                lv.ScrollIntoView(lvd);
                return;
            }
            if (sfile.Count > 0)
                load(sfile,lvd.File);

        }

        private void toggleswitch_Toggled_1(object sender, RoutedEventArgs e)
        {
            if (thumbnail != null)
            {

                if (toggleswitch.IsOn)
                {
                    thumbgrid.Visibility = Visibility.Visible;
                    thumbnail.Visibility = Visibility.Visible;
                }
                else
                {
                    thumbgrid.Visibility = Visibility.Collapsed;
                    thumbnail.Visibility = Visibility.Collapsed;
                }
            }
        }

        void mix_random(ref List<StorageFile> sfile)
        {
            List<StorageFile> temp = new List<StorageFile>();
            Random rand = new Random();
            int i = -1;
            if (isplaying || ispaused)
            {
                temp.Add(sfile[this.i]);
                sfile.RemoveAt(this.i);
            }
            while(sfile.Count>0)
            {
                i = rand.Next(0, sfile.Count - 1);
                temp.Add(sfile[i]);
                for (int idx = i; idx < sfile.Count - 1; idx++)
                {
                    sfile[idx] = sfile[idx + 1];
                }
                sfile.RemoveAt(sfile.Count - 1);
            }
            sfile.Clear();
            foreach (StorageFile sf in temp)
                sfile.Add(sf);
            this.i = 0;
        }

        private async void random_Click_1(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => mix_random(ref sfile));
            load(sfile,lvd.File);
            fileopen(true);
            if (media.AutoPlay == false)
                media.AutoPlay = true;

        }
        ListViewData SelectedPlaylist=null;
        PlaylistInfoData data = null;
        private async void lv_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            SelectedPlaylist = null;
            delete.Visibility = Visibility.Visible;
            BitmapImage bi = new BitmapImage();
            if (e.AddedItems.Count >0 && sfile.Count > 0 && !playlistview)
            {
                PlaylistInfo.Visibility = Visibility.Collapsed;
                Music_Info.Visibility = Visibility.Visible;
                bi.SetSource(await ((ListViewData)e.AddedItems[0]).File.GetThumbnailAsync(ThumbnailMode.SingleItem, 600, ThumbnailOptions.UseCurrentScale));
                InfoPage.Visibility = Visibility.Visible;
                mproperties = await ((ListViewData)e.AddedItems[0]).File.Properties.GetMusicPropertiesAsync();
                Constants c=new Constants(mproperties);
                Music_Info_.DataContext = c;
                Music_Info_.UpdateLayout();
                albumart.Source = bi;
                for (int j = 0; j < sfile.Count; j++)
                {
                    if (((ListViewData)e.AddedItems[0]).File.Path.Equals(sfile[j].Path,StringComparison.OrdinalIgnoreCase))
                    {
                        index = j;
                    }
                }
            }
            else if (playlistview&& e.AddedItems.Count>0)
            {
                Playlist_info_s.DataContext = new PlaylistInfoData(String.Empty, string.Empty, string.Empty,string.Empty);
                InfoPage.Visibility = Visibility.Visible;
                PlaylistInfo.Visibility = Visibility.Visible;
                Music_Info.Visibility = Visibility.Collapsed;
                LoadingBorder.Visibility = Visibility.Visible;
                string x = LoadingText.Text;
                LoadingText.Text = "Loading Information";
                StorageFile playlist_File = (e.AddedItems[0] as ListViewData).File;
                Playlist pl=null;
                try
                {
                     pl= await Playlist.LoadAsync(playlist_File);
                }
                catch (Exception)
                { }
                SelectedPlaylist = new ListViewData { File = playlist_File, DisplayName = playlist_File.Name, playing =false };
                data = new PlaylistInfoData(playlist_File.DisplayName, (pl!=null)?pl.Files.Count.ToString()+" Items Present":"0 - Corrupted File", "00:00:00",playlist_File.Path);
               Playlist_info_s.DataContext = data;
               Playlist_info_s.UpdateLayout();
                LoadingBorder.Visibility = Visibility.Collapsed;
                LoadingText.Text = x;
            }
            if (e.RemovedItems.Count > 0)
            {
                InfoPage.Visibility = Visibility.Collapsed;
            }

        }

        int index = 0;

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
                try
                {
                    string x = LoadingText.Text;
                    LoadingText.Text = "Removing File . . .";
                    LoadingBorder.Visibility = Visibility.Visible;
                    if (isplaylistused)
                    {
                        playlist.Files.RemoveAt(index);
                        InfoPage.Visibility = Visibility.Collapsed;
                        if (playlist.Files.Count == 0)
                        {
                            await openedplaylist.DeleteAsync(StorageDeleteOption.PermanentDelete);
                            lv.Visibility = Visibility.Collapsed;
                            emptynote.Visibility = Visibility.Visible;
                            createplaylist.IsEnabled = false;
                            LoadingBorder.Visibility = Visibility.Collapsed;
                            LoadingText.Text = x;
                        }
                        else
                        {
                            await playlist.SaveAsync();
                            sfile = (await Playlist.LoadAsync(openedplaylist)).Files.ToList<StorageFile>();
                            LoadingBorder.Visibility = Visibility.Collapsed;
                            LoadingText.Text = x;
                            if (i < index)
                            {
                                CurrentListViewObject = lv.ItemsSource = ListViewData.LoadMusic(sfile, (lvd != null) ? lvd.File : null);
                                lvd = ListViewData.CurrentObject;
                            }
                            else
                            if (i == index)
                            {
                                if (i-- <= sfile.Count - 1)
                                    next();
                                else
                                {
                                    CurrentListViewObject = lv.ItemsSource = ListViewData.LoadMusic(sfile, (lvd != null) ? lvd.File : null);
                                    lvd = ListViewData.CurrentObject;
                                    stop_Click_1(new object(), new RoutedEventArgs());
                                    Constants c = new Constants(true);
                                    endposition.Text = "";
                                    currentposition.Text = "";
                                    Details.DataContext = c;
                                    Details.UpdateLayout();
                                    playlistended = true;
                                    thumbnail.Source = null;
                                }
                            }
                            No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
                        }
                    }
                    else
                    {
                        sfile.RemoveAt(index);
                        LoadingBorder.Visibility = Visibility.Collapsed;
                        LoadingText.Text = x;
                        if (i != index)
                        {
                            CurrentListViewObject = lv.ItemsSource = ListViewData.LoadMusic(sfile, (lvd != null) ? lvd.File : null);
                            lvd = ListViewData.CurrentObject;
                        }
                        else
                        if (i == index)
                        {
                            if (i-- <= sfile.Count - 1)
                                next();
                            else
                            {
                                CurrentListViewObject = lv.ItemsSource = ListViewData.LoadMusic(sfile, null);
                                lvd = ListViewData.CurrentObject;
                                stop_Click_1(new object(), new RoutedEventArgs());
                                Constants c = new Constants(true);
                                Details.DataContext = c;
                                Details.UpdateLayout();
                                playlistended = true;
                                thumbnail.Source = null;
                            }
                        }
                        No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
                    }
                }
                catch (Exception ex)
                {
                    NotifyUser(ex.Message);
                }
        }

        private void Grid_PointerReleased_2(object sender, PointerRoutedEventArgs e)
        {
            if (!inbound)
                InfoPage.Visibility = Visibility.Collapsed;
        }
        bool inbound = false;
        private void media_MediaFailed_1(object sender, ExceptionRoutedEventArgs e)
        {
            if (!e.ErrorMessage.Contains("ENGINE"))
            {
                NotifyUser(e.ErrorMessage);
            }
        }

        private void forward_Click_1(object sender, RoutedEventArgs e)
        {
            next();
        }

        private void backward_Click_1(object sender, RoutedEventArgs e)
        {
            prev();
        }

        private void InfoPage_PointerEntered_1(object sender, PointerRoutedEventArgs e)
        {
            inbound = true;
        }

        private void InfoPage_PointerExited_1(object sender, PointerRoutedEventArgs e)
        {
            inbound = false;
        }

        private async void Add_Click_1(object sender, RoutedEventArgs e)
        {
            if (isplaylistused)
                try
                {
                    string x = LoadingText.Text;
                    LoadingText.Text = "Loading File(s)";
                    LoadingBorder.Visibility = Visibility.Visible;
                    FileOpenPicker fop = new FileOpenPicker();
                    fop.FileTypeFilter.Add(".");
                    fop.FileTypeFilter.Add(".mp3");
                    fop.FileTypeFilter.Add(".wma");
                    fop.FileTypeFilter.Add(".aac");
                    fop.FileTypeFilter.Add(".adt");
                    fop.FileTypeFilter.Add(".adts");
                    fop.FileTypeFilter.Add(".wav");
                    fop.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                    IReadOnlyList<StorageFile> ilist = await fop.PickMultipleFilesAsync();
                    int count = 0;
                    foreach (StorageFile sf in ilist)
                    {
                        if (!present(playlist, sf))
                            playlist.Files.Add(sf);
                        else
                            count++;
                    }
                    if (ilist.Count > 0)
                    {
                        playlist.SaveAsync();
                        sfile = playlist.Files.ToList<StorageFile>();
                        CurrentListViewObject = lv.ItemsSource = ListViewData.LoadMusic(sfile, (lvd != null) ? lvd.File : null);
                        No_f.Text = "      " + lv.Items.Count.ToString() + " Files Loaded";
                        lvd = ListViewData.CurrentObject;
                    }
                    LoadingBorder.Visibility = Visibility.Collapsed;
                    LoadingText.Text = x; 
                    if (count > 1)
                        await new MessageDialog((count.ToString() + " Files are already present in the playlist"), "PlayIt-->MusicPlayer").ShowAsync();
                    else
                        if (count == 1)
                            await new MessageDialog("1 File Already Present so It's Avoided From Adding", "PlayIt-->MusicPlayer").ShowAsync();
                }
                catch (Exception ex)
                {
                    NotifyUser(ex.Message);
                }
            else
                open(false);
        }

        private bool present(Playlist playlist, StorageFile sf)
        {
            bool b = false;
            foreach (StorageFile ts in playlist.Files)
            {
                if (ts.Path.Equals(sf.Path, StringComparison.CurrentCultureIgnoreCase))
                {
                    b = true;
                    break;
                }
            }
            return b;
        }
        
        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (SelectedPlaylist != null)
            {
                LoadingBorder.Visibility = Visibility.Visible;
                string x = LoadingText.Text;
                LoadingText.Text = "Please Wait...";
                await SelectedPlaylist.File.DeleteAsync(StorageDeleteOption.PermanentDelete);
                LoadingBorder.Visibility = Visibility.Collapsed;
                LoadingText.Text = x;
                OpenPlaylist_Click_1(new object(), new RoutedEventArgs());
            }
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (SelectedPlaylist != null)
            {
                Play_text.Text = "Rename Playlist";
                playlistname.Text = data.FileName;
                playlistgrid.Visibility = Visibility.Visible;
            }
        }

        private void lv_LayoutUpdated_1(object sender, object e)
        {
            if (lv.Items.Count > 0)
            {
                emptynote.Visibility = Visibility.Collapsed;
                
                if (playlistwindowopen && sfile.Count == 0)
                    Add.IsEnabled = false;
                else
                    Add.IsEnabled = true;
            }
            else
            {
                emptynote.Visibility = Visibility.Visible;
                Add.IsEnabled = false;
            }
            
        }
    }
        #endregion
    internal class ListViewData
    {
        internal bool playing = false;
        static ListViewData lvd1=null,lvd2=null;
       public static List<ListViewData> LoadMusic(List<StorageFile> array, StorageFile s)
        {
            List<ListViewData> list = new List<ListViewData>();
            foreach (StorageFile file in array)
            {
                lvd1=new ListViewData { File = file, DisplayName = file.DisplayName, playing = (s!=null)?(s.Path.Equals(file.Path,StringComparison.OrdinalIgnoreCase)):false };
                if (lvd1.playing)
                    lvd2 = lvd1;
                list.Add(lvd1);
            }
            return list;
        }
       public static ListViewData CurrentObject { get { return lvd2; } }
       public StorageFile File { get; set; }
        public string DisplayName { get; set; }
        public Visibility isPlayingIcon
        {
            get
            {
                if (playing)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }
        internal static object LoadPlaylists(StorageFile[] play)
        {
            List<ListViewData> pl=new List<ListViewData>();
            foreach (StorageFile f in play)
                pl.Add(new ListViewData { DisplayName = f.DisplayName, File = f, playing = false });
            return pl;
        }
    }
    public class TooltipConverter : IValueConverter
    {
        TimeSpan tsp = new TimeSpan();
        public object Convert(object value, Type targetType, object parameter, string
language)
        {
            if (value is double)
            {
                tsp = new TimeSpan(0, 0,(int)Math.Round((double)value, MidpointRounding.AwayFromZero));
                return tsp.ToString();
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                return ((TimeSpan)value).TotalSeconds;
            }
            return value;
        }
    }
    public class PlaylistInfoData
    {
        string time_length = "00:00:00";
        public PlaylistInfoData(string name,string count,string time,string path)
        {
            this.FileName = name;
            this.No_Files = count;
            time_length = time;
            FilePath = path;
        }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string No_Files { get; set; }
        public string TimeLength
        {
            get
            {
                return time_length;
            }
        }
    }
}
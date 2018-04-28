using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.Media.Playlists;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MediaCenter;
// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227
namespace MediaCenter
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        Playlist pl = new Playlist();
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            Constants.ismusicfileactivated = false;
            Constants.isvideofileactivated = false;
        }
        bool running = false;
        internal static Frame rootFrame;
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            rootFrame = Window.Current.Content as Frame;
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    await SuspensionManager.RestoreAsync();
                }
                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
            running = true;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
            Windows.UI.Notifications.TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            Windows.UI.Notifications.BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
        }

        List<StorageFile>playlist = new List<StorageFile>();
        List<StorageFile> music = new List<StorageFile>();
        protected async override void OnFileActivated(FileActivatedEventArgs e)
        {

            Debug.WriteLine("In File Activated");
            int i = 0;
            StorageFile[] sfile = new StorageFile[e.Files.Count];
            music.Clear();
            playlist.Clear();
            List<StorageFile> video = new List<StorageFile>();
            if (e.Verb == "open")
            {
                if (e.Files.Count > 1)
                    Constants.loadingtext = "Loading Files . . .";
                else
                    Constants.loadingtext = "Loading File . . .";
                Constants.loadingactivated = true;
                foreach (IStorageItem sf in e.Files)
                {
                    if (ismediacenterextention((StorageFile)sf))
                    {
                        sfile[i] = (StorageFile)sf;
                        i++;
                    }
                }
                sortbyfileextention(sfile, ref music, ref video);
                foreach (StorageFile sf in sfile)
                {
                    if (isplaylist(sf))
                    {
                        try
                        {
                            playlist.Add(sf);
                        }
                        catch (Exception ex)
                        {
                            NotifyUser(ex);
                        }
                    }
                }

                if (!running)
                {
                    rootFrame = Window.Current.Content as Frame;
                    // Do not repeat app initialization when the Window already has content,
                    // just ensure that the window is active
                    if (rootFrame == null)
                    {
                        // Create a Frame to act as the navigation context and navigate to the first page
                        rootFrame = new Frame();
                        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                        {
                            await SuspensionManager.RestoreAsync();
                        }
                        // Place the frame in the current Window
                        Window.Current.Content = rootFrame;
                    }
                    if (rootFrame.Content == null)
                    {
                        // When the navigation stack isn't restored navigate to the first page,
                        // configuring the new page by passing required information as a navigation
                        // parameter
                        if (!rootFrame.Navigate(typeof(MainPage)))
                        {
                            throw new Exception("Failed to create initial page");
                        }
                    }
                    // Ensure the current window is active
                    Window.Current.Activate();
                    running = true;
                }
                object[] mus = { playlist, music };
                if (music.Count > 0 || playlist.Count > 0)
                {
                    Constants.ismusicfileactivated = true;
                    if (FirstInstanceProperty.MusicPlayer.count > 0)
                        BlankPage1.RemoveHandles();
                    rootFrame.Navigate(typeof(BlankPage1), mus);
                }
                else
                    if (video.Count > 0)
                    {
                        Constants.isvideofileactivated = true;
                        if (FirstInstanceProperty.VideoPlayer.count > 0)
                            BlankPage2.RemoveHandles();
                        rootFrame.Navigate(typeof(BlankPage2), video);
                    }
            }

        }
        private void NotifyUser(Exception ex)
        {
            string logo = ((Constants.inmusicplayer) ? "'Assets/music.png'" : ((Constants.invideoplayer) ? "'Assets/video.jpg'" : "'Assets/Logo.png'"));
            string xmlstring = "<toast>"
                           + "<visual version='1'>"
                           + "<binding template='ToastImageAndText02'>"
                           + "<text id='1'>Error Occured!</text>"
                           + "<text id='2'>" + ex.Message.ToString() + "</text>"
                           + "<image id='1' src=" + logo + "/>"
                           + "</binding>"
                           + "</visual>"
                           + "<audio src='ms-winsoundevent:Notification.IM'/>"
                           + "</toast>";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(xmlstring);
            ToastNotification toast = new ToastNotification(xmldoc);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
        private void sortbyfileextention(StorageFile[] sfile, ref List<StorageFile> music, ref List<StorageFile> video)
        {
            foreach (StorageFile sf in sfile)
            {
                foreach (string vextention in videoExtentions)
                {
                    if (sf.FileType.Equals(vextention, StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            video.Add(sf);
                        }
                        catch (Exception ex)
                        {
                            NotifyUser(ex);
                        }
                    }
                }
                foreach (string mextentions in musicExtentions)
                    if (sf.FileType.Equals(mextentions, StringComparison.CurrentCultureIgnoreCase))
                    {
                        try
                        {
                            music.Add(sf);
                        }
                        catch (Exception ex)
                        {
                            NotifyUser(ex);
                        }
                    }

            }
        }
        string[] musicExtentions = { ".mp3", ".aac", ".wav", ".adt", ".adts", ".wma" };
        string[] videoExtentions = { ".wmv", ".avi", ".mp4" };
        string[] playlistExtentions = { ".m4a", ".wpl", ".zpl" };
        bool ismediacenterextention(StorageFile sf)
        {
            foreach (string ext in musicExtentions)
                if (sf.FileType.Equals(ext, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            foreach (string ext in videoExtentions)
                if (sf.FileType.Equals(ext, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            foreach (string ext in playlistExtentions)
                if (sf.FileType.Equals(ext, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            return false;
        }
        bool isplaylist(StorageFile sf)
        {
            foreach (string ext in playlistExtentions)
                if (sf.FileType.Equals(ext, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            return false;
        }
    }
}

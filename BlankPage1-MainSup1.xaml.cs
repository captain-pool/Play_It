using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.Storage.FileProperties;
using Windows.UI;
using Windows.Media.Playlists;
using Windows.Storage.Search;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaCenter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage1_MainSup1 : Page
    {
        ObservableCollection<Music> o = new ObservableCollection<Music>();
        List<String> fileTypeFilter = new List<String>();
        QueryOptions query;
        public BlankPage1_MainSup1()
        {
            this.InitializeComponent();
            fileTypeFilter.Add("*");
            query = new QueryOptions(CommonFileQuery.DefaultQuery, fileTypeFilter);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            StorageFolder sfolder=null;
            try
            {
                sfolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Settings-Music_Player");
            }
            catch (Exception)
            {
            }
            if(sfolder==null)
                sfolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Settings-Music_Player");
            StorageFile settings=null;
            try
            {
                settings = await sfolder.GetFileAsync("MusicPlayerSettings.usersettings");
            }
            catch (Exception)
            {
            }
            IReadOnlyList<StorageFile> ir;
            if (settings == null)
                settings = await sfolder.CreateFileAsync("MusicPlayerSettings.usersettings");
            string text=string.Empty;
            try
            {
                text=await FileIO.ReadTextAsync(settings);
            }
            catch (Exception)
            {
            }
            if (text == string.Empty)
            {
                await FileIO.WriteTextAsync(settings, @"E:\songs\Music\Hindi songs\Nf");
                text = await FileIO.ReadTextAsync(settings);
            }
            else
            {
                ir = await ((await StorageFolder.GetFolderFromPathAsync(text)).CreateFileQueryWithOptions(query).GetFilesAsync());
                AddToMasterPlaylist(sfolder, ir);
                int i = 0;
                StorageItemThumbnail[] st = new StorageItemThumbnail[ir.Count];
                st = await getThumbnails(ir);
                
                MusicProperties mp = null;
                foreach (StorageFile sf in ir)
                {
                    bi = new BitmapImage();
                    bi.SetSource(st[i]);
                     mp = await sf.Properties.GetMusicPropertiesAsync();
                    o.Add(new Music{Thumbnail=bi,Duration=((mp.Duration.Hours < 10) ? ("0" + mp.Duration.Hours.ToString()) : mp.Duration.Hours.ToString()) + ":" + ((mp.Duration.Minutes < 10) ? ("0" + mp.Duration.Minutes.ToString()) : (mp.Duration.Minutes.ToString())) + ":" + ((mp.Duration.Seconds < 10) ? ("0" + mp.Duration.Seconds.ToString()) : (mp.Duration.Seconds.ToString())),Title=mp.Title,Album=mp.Album,CurrentFile=sf});
                   
                    i++;
                }
                ObservableCollection<Music> og = new ObservableCollection<Music>();
                
                Music[] mus = await getByAlbum(ir.ToArray());
                foreach (Music m in mus)
                {
                    og.Add(m);
                }
                MainGrid.ItemsSource = og;
                MainGrid.UpdateLayout();
                p_ring.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<Music[]> getByAlbum(StorageFile[] ir)
        {
            List <Music> list = new List<Music>();
            List<StorageFile>[] l=await Sort(ir);
            MusicProperties mp = null;
            StorageItemThumbnail st=null;
            for (int i = 0; i < ir.Length; i++)
            {
                st =await (l[i])[0].GetThumbnailAsync(ThumbnailMode.MusicView, 200, ThumbnailOptions.UseCurrentScale);
                mp =await (l[i])[0].Properties.GetMusicPropertiesAsync();
                bi = new BitmapImage();
                bi.SetSource(st);
                list.Add(new Music { Thumbnail = bi, Album = mp.Album });
            }
            return list.ToArray<Music>();
        }
        
        private async Task<List<StorageFile>[]> Sort(StorageFile[] ir)
        {
            bool val = false;
            MusicProperties mp1 = null, mp2 = null;
            List<StorageFile>[] sf=new List<StorageFile>[ir.Length];
            for (int i = 0; i < sf.Length; i++)
            {
                sf[i] = new List<StorageFile>();
                sf[i].Add(null);
            }
            for (int i = 0; i < ir.Length; i++)
            {
                mp1 = await ir[i].Properties.GetMusicPropertiesAsync();
                for (int j = 0; j < ir.Length; j++)
                {
                    mp2 = await ir[j].Properties.GetMusicPropertiesAsync();
                    if (mp1.Album==mp2.Album)
                    {
                        val = false;
                        for (int k = 0; k < sf.Length; k++)
                        {
                            if (sf[k][0] == ir[j])
                            {
                                val = true;
                                break;
                            }
                        }
                        if (!val)
                        {
                            sf[i].RemoveAt(0);
                            sf[i].Add(ir[j]);
                        }
                        else
                            continue;
                    }
                    
                }
            }
            
            return sf;
        }

        private async Task<StorageItemThumbnail[]> getThumbnails(IReadOnlyList<StorageFile> ir)
{
    StorageItemThumbnail[] st = new StorageItemThumbnail[ir.Count];
    for(int i=0;i<ir.Count;i++)
    {
        st[i] = await ir[i].GetThumbnailAsync(ThumbnailMode.MusicView, 200, ThumbnailOptions.UseCurrentScale);
    }
    return st;
}
Playlist masterPlaylist = new Playlist();
        private async void AddToMasterPlaylist(StorageFolder sfolder, IReadOnlyList<StorageFile> ir)
        {
            StorageFolder storageFolder = null;
            try
            {
                storageFolder =await sfolder.GetFolderAsync("MasterPlaylistSection");
            }
            catch (Exception)
            {
            }
            if (storageFolder == null)
            {
                storageFolder = await sfolder.CreateFolderAsync("MasterPlaylistSection");
            }
            foreach (StorageFile sf in ir)
            {
                masterPlaylist.Files.Add(sf);
            }
            await masterPlaylist.SaveAsAsync(storageFolder, "MasterPlaylist", NameCollisionOption.ReplaceExisting, PlaylistFormat.Zune);

        }
    
      BitmapImage bi=new BitmapImage();
       
        private void MainGrid_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            ObservableCollection<Music> obv=new ObservableCollection<Music>();
            Music mus = e.ClickedItem as Music;
            foreach (Music m in o)
            {
                if (m.Album.Equals(mus.Album))
                    obv.Add(m);
            }
            ByFiles.ItemsSource = obv;
            MainGrid.Visibility = Visibility.Collapsed;
            ByFiles.Visibility = Visibility.Visible;
        }
    }
    public class Music
    {
        public ImageSource Thumbnail{get;set;}
        public string Duration { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public StorageFile CurrentFile { get; set; }
    }
  
}


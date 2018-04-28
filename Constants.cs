using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;
using Windows.Storage;
namespace MediaCenter
{
    public  struct FirstInstanceProperty
    {
        public struct MusicPlayer
        {
            public static int count = 0;
        }
        public struct VideoPlayer
        {
            public static int count =0;
        }
    }
    public static class Extensions
    {
        public static bool contains(this List<StorageFile> file, StorageFile sf)
        {
            bool ret=false;
            foreach (StorageFile s in file)
            {
                if (s.Path.Equals(sf.Path, StringComparison.OrdinalIgnoreCase))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        public static string toString(this IList<string> ilist)
        {
            string str = string.Empty;
            foreach (string s in ilist)
                str += s + ";";
            return str;
        }
    }
    public class Constants
    {
        MusicProperties m = null;
        public string Text { get; set; }
        private string songname,albumname, duration, genre, year, artistname, albumartist;
        public Constants(MusicProperties props)
        {
            Properties = props;
            SongName = ArtistName=AlbumArtist = AlbumName = Year = Genre = Duration = string.Empty;
        }
        bool b = false;
        public Constants(bool b=false)
        {
            if (b)
            {
                this.b = b;
                SongName = AlbumName = Duration = Genre = Year = ArtistName = AlbumArtist = string.Empty;
            }
        }
        static internal bool inmusicplayer=false,invideoplayer=false;
        public string SongName { get { return songname; } 
            set { songname = (!b) ? ((m != null) ? m.Title : "No Title") : value; } }
        public string AlbumName { get { return albumname; } set { albumname = (!b) ? ((m != null) ? m.Album : "No Album") : value; } }
        public string Duration { set { duration = (!b) ? ((m != null) ? m.Duration.ToString() : "No Duration") : value ; } get { return duration; } }
        public string Genre { set { genre = (!b) ? ((m != null) ? string.Join(" ", m.Genre.ToArray()) : "No Genre") : value; } get { return genre; } }
        public string Year { set { year = (!b) ? ((m != null) ? m.Year.ToString() : "No Year") : value; } get { return year; } }
        public string ArtistName { 
            set { artistname = (!b) ? ((m != null) ? m.Artist : "No Artist") : value; } 
            get { return artistname; } }
        public string AlbumArtist { set {albumartist=(!b)?((m!=null)?m.AlbumArtist:"No Album Artist"):value; } get{return albumartist;} }
        MusicProperties Properties { get { return m; } set { m = value; }}
        public String[] item={"Delete Playlist","Rename Playlist"};
        public string[] Item { get { return item; } }
        static internal bool ismusicfileactivated { get; set; }
        static internal bool isvideofileactivated { get; set; }
        static internal bool loadingactivated = false;
        static internal string loadingtext = "";
        static internal bool isplaylistused = false;
    }
}

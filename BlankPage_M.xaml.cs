using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaCenter
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BlankPage1_M : Page
    {
        StorageFolder temp;
        IReadOnlyList<StorageFile> ilist;
        ListViewItem listitem = new ListViewItem();
        public BlankPage1_M()
        {
            this.InitializeComponent();
            listitem.Drop += listitem_Drop;
        }



        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            temp = ApplicationData.Current.RoamingFolder;
            ilist = await temp.GetFilesAsync();
            foreach (StorageFile sf in ilist)
            {
                playlist.Items.Add(CreateListItem(sf));
            }
            GridFrame.Navigate(typeof(BlankPage1_MainSup1));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void PlaylistBox_Drop_1(object sender, DragEventArgs e)
        {
            StorageFile[] o = (StorageFile[])e.OriginalSource;
        }

        private ListViewItem CreateListItem(StorageFile sf)
        {
            listitem = new ListViewItem();
            listitem.Background = new SolidColorBrush(Colors.CornflowerBlue);
            listitem.Content = sf.DisplayName;
            listitem.Padding = new Thickness(0, 10, 0, 10);
            listitem.FontSize = 20;
            listitem.Margin = new Thickness(0);
            return listitem;
        }

        void listitem_Drop(object sender, DragEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void playlist_ItemClick_1(object sender, ItemClickEventArgs e)
        {

        }

        private void PlaylistBox_ItemClick_1(object sender, ItemClickEventArgs e)
        {

        }
    }
}

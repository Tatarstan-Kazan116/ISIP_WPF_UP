using System.Data.Entity;
using System.Linq;
using System.Windows.Controls;

namespace UP_New.Pages
{
    public class ReadingListDisplay
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
    }

    public partial class ReadingListsPage : Page
    {
        public ReadingListsPage() => InitializeComponent();

        private void TcLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TcLists.SelectedItem is TabItem tab)
            {
                string status = tab.Header.ToString();
                var list = Core.Context.ReadingLists
                    .Where(rl => rl.UserID == UserSession.UserId && rl.Status == status)
                    .Select(rl => new ReadingListDisplay
                    {
                        BookID = rl.BookID,
                        Title = rl.Books.Title,
                        Status = rl.Status
                    }).ToList();
                DgLists.ItemsSource = list;
            }
        }

        private void ChangeStatus_Click(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.SelectedItem is ComboBoxItem item)
            {
                int bookId = (int)cb.Tag;
                string newStatus = item.Content.ToString();
                var rl = Core.Context.ReadingLists.First(x => x.UserID == UserSession.UserId && x.BookID == bookId);
                rl.Status = newStatus;
                rl.AddedDate = System.DateTime.Now;
                Core.Context.SaveChanges();
            }
        }
    }
}
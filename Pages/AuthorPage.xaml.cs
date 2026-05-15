using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UP_New.Pages
{
    public class AuthorBookDisplay
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public Visibility ShowAppeal { get; set; }
        public Visibility ShowEditBox { get; set; }
    }

    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            LoadMyBooks();
        }

        private void LoadMyBooks()
        {
            var data = Core.Context.Books
                .Where(b => b.AuthorID == UserSession.UserId)
                .Select(b => new AuthorBookDisplay
                {
                    BookID = b.BookID,
                    Title = b.Title,
                    Status = b.IsFrozen == true ? "Заморожена" : "Активна", // ИСПРАВЛЕНО
                    ShowAppeal = b.IsFrozen == true ? Visibility.Visible : Visibility.Collapsed,
                    ShowEditBox = Visibility.Collapsed
                })
                .ToList();
            DgMyBooks.ItemsSource = data;
        }

        private void AddNewBook_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewTitle.Text) || TxtNewTitle.Text == "Название книги") return;
            Core.Context.Books.Add(new Books
            {
                Title = TxtNewTitle.Text,
                Content = TxtNewContent.Text,
                AuthorID = UserSession.UserId,
                PublishDate = System.DateTime.Now,
                IsFrozen = false
            });
            Core.Context.SaveChanges();
            LoadMyBooks();
            MessageBox.Show("Книга опубликована!");
        }

        private void EditBook_Click(object s, RoutedEventArgs e)
        {
            int id = (int)((Button)s).Tag;
            var row = FindParent<DataGridRow>((Button)s);
            if (row != null)
            {
                var txtBox = FindChild<TextBox>(row);
                if (txtBox != null)
                {
                    var book = Core.Context.Books.First(b => b.BookID == id);
                    book.Content = txtBox.Text;
                    Core.Context.SaveChanges();
                    LoadMyBooks();
                    MessageBox.Show("Изменения сохранены.");
                }
            }
        }

        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private void AppealBookFreeze_Click(object s, RoutedEventArgs e)
        {
            int bookId = (int)((Button)s).Tag;
            Core.Context.UnfreezeRequests.Add(new UnfreezeRequests
            {
                UserID = UserSession.UserId,
                TargetType = "Book",
                TargetID = bookId,
                Reason = "Оспаривание заморозки",
                RequestDate = System.DateTime.Now,
                Status = "На рассмотрении"
            });
            Core.Context.SaveChanges();
            MessageBox.Show("Заявка отправлена.");
        }
    }
}
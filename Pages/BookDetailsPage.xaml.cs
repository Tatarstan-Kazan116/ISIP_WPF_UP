using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UP_New.Pages
{
    public class ReviewDisplay
    {
        public int ReviewID { get; set; }
        public string User { get; set; }
        public int Rating { get; set; }
        public string ReviewText { get; set; }
    }

    public partial class BookDetailsPage : Page
    {
        private int _bookId;
        public BookDetailsPage(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadBook();
            LoadReviews();
            if (UserSession.IsAdmin)
            {
                BtnFreezeBook.Visibility = Visibility.Visible;
                ColAdminReview.Visibility = Visibility.Visible;
            }
        }

        private void LoadBook()
        {
            var b = Core.Context.Books.FirstOrDefault(x => x.BookID == _bookId);
            if (b != null)
            {
                TbTitle.Text = b.Title;
                TbMeta.Text = $"Автор: {b.Users?.DisplayName} | Жанры: {string.Join(", ", b.Genres.Select(g => g.GenreName))}"; TbContent.Text = b.Content;
            }
        }

        private void LoadReviews()
        {
            DgReviews.ItemsSource = Core.Context.Reviews
                .Where(r => r.BookID == _bookId)
                .Select(r => new ReviewDisplay
                {
                    ReviewID = r.ReviewID,
                    User = r.Users.DisplayName,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText
                }).ToList();
        }

        private void ComplainBook_Click(object s, RoutedEventArgs e) => SendComplaint("Book", _bookId, TxtBookComplaint.Text);

        private void ComplainAuthor_Click(object s, RoutedEventArgs e)
        {
            var authorId = Core.Context.Books.First(x => x.BookID == _bookId).AuthorID;
            SendComplaint("User", authorId, TxtBookComplaint.Text);
        }

        private void ComplainReview_Click(object s, RoutedEventArgs e)
        {
            var btn = (Button)s;
            var row = FindParent<DataGridRow>(btn);
            if (row != null)
            {
                var txtBox = FindChild<TextBox>(row);
                if (txtBox != null)
                    SendComplaint("Review", (int)btn.Tag, txtBox.Text);
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

        private void SendComplaint(string type, int targetId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) { MessageBox.Show("Укажите причину жалобы."); return; }
            Core.Context.Complaints.Add(new Complaints
            {
                UserID = UserSession.UserId,
                TargetType = type,
                TargetID = targetId,
                Reason = reason,
                ComplaintDate = System.DateTime.Now
            });
            Core.Context.SaveChanges();
            MessageBox.Show("Жалоба отправлена.");
        }

        private void FreezeBook_Click(object s, RoutedEventArgs e)
        {
            var book = Core.Context.Books.First(x => x.BookID == _bookId);
            book.IsFrozen = true;
            Core.Context.SaveChanges();
            MessageBox.Show("Книга заморожена.");
        }

        private void FreezeReview_Click(object s, RoutedEventArgs e)
        {
            var review = Core.Context.Reviews.First(x => x.ReviewID == (int)((Button)s).Tag);
            Core.Context.Reviews.Remove(review);
            Core.Context.SaveChanges();
            LoadReviews();
            MessageBox.Show("Отзыв удалён.");
        }
        private void AddReview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string reviewText = TxtReviewText.Text.Trim();

                if (string.IsNullOrWhiteSpace(reviewText))
                {
                    MessageBox.Show("⚠️ Пожалуйста, напишите текст отзыва!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CbRating.SelectedItem is ComboBoxItem ratingItem)
                {
                    int rating = int.Parse(ratingItem.Content.ToString());

                    if (rating < 1 || rating > 10)
                    {
                        MessageBox.Show("⚠️ Оценка должна быть от 1 до 10!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Проверяем, не оставлял ли пользователь отзыв на эту книгу
                    var existingReview = Core.Context.Reviews
                        .FirstOrDefault(r => r.BookID == _bookId && r.UserID == UserSession.UserId);

                    if (existingReview != null)
                    {
                        var result = MessageBox.Show(
                            "⚠️ Вы уже оставляли отзыв на эту книгу. Заменить его?",
                            "Подтверждение",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.No) return;

                        existingReview.ReviewText = reviewText;
                        existingReview.Rating = rating;
                        existingReview.ReviewDate = System.DateTime.Now;
                        MessageBox.Show("✅ Ваш отзыв обновлён!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Core.Context.Reviews.Add(new Reviews
                        {
                            BookID = _bookId,
                            UserID = UserSession.UserId,
                            ReviewText = reviewText,
                            Rating = rating,
                            ReviewDate = System.DateTime.Now
                        });
                        MessageBox.Show("✅ Отзыв успешно опубликован!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    Core.Context.SaveChanges();
                    TxtReviewText.Text = "";
                    CbRating.SelectedIndex = 9; // 10 по умолчанию
                    LoadReviews(); // Обновляем список
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при добавлении отзыва:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
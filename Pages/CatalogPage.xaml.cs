using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace UP_New.Pages
{
    public class BookDisplay
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string CoverPath { get; set; }
        public string Author { get; set; }
        public string Genres { get; set; }
        public double AvgRating { get; set; }
    }

    public partial class CatalogPage : Page
    {
        private CollectionViewSource _cvs;

        public CatalogPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadBooks();
        }

        private void LoadGenres()
        {
            CbGenre.ItemsSource = Core.Context.Genres.ToList();
            CbGenre.DisplayMemberPath = "GenreName";
            CbGenre.SelectedValuePath = "GenreID";
            CbGenre.SelectedIndex = -1;
        }

        private void LoadBooks()
        {
            // СНАЧАЛА загружаем данные из БД в память
            var booksList = Core.Context.Books
                .Where(b => b.IsFrozen == false)
                .Include(b => b.Users)
                .Include(b => b.Genres)
                .Include(b => b.Reviews)
                .ToList(); // ← Материализуем здесь!

            // ПОТОМ делаем проекцию с string.Join (уже в памяти)
            var data = booksList.Select(b => new BookDisplay
            {
                BookID = b.BookID,
                Title = b.Title,
                CoverPath = b.CoverPath,
                Author = b.Users != null ? b.Users.DisplayName : "Неизвестно",
                Genres = string.Join(", ", b.Genres.Select(g => g.GenreName)), // Теперь работает!
                AvgRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0.0
            }).ToList();

            _cvs = new CollectionViewSource { Source = data };
            DgBooks.ItemsSource = _cvs.View;
        }

        private void ApplyFilters()
        {
            _cvs.View.Filter = item =>
            {
                var book = item as BookDisplay;
                if (book == null) return false;

                bool matchSearch = string.IsNullOrWhiteSpace(TxtSearch.Text) ||
                                   book.Title.IndexOf(TxtSearch.Text, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   book.Author.IndexOf(TxtSearch.Text, System.StringComparison.OrdinalIgnoreCase) >= 0;

                bool matchGenre = CbGenre.SelectedValue == null ||
                                  book.Genres.IndexOf(((Genres)CbGenre.SelectedItem).GenreName, System.StringComparison.OrdinalIgnoreCase) >= 0;

                return matchSearch && matchGenre;
            };
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void BtnSearch_Click(object sender, RoutedEventArgs e) => ApplyFilters();

        private void SortByName_Click(object sender, RoutedEventArgs e)
        {
            _cvs.View.SortDescriptions.Clear();
            _cvs.View.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
        }

        private void SortByRating_Click(object sender, RoutedEventArgs e)
        {
            _cvs.View.SortDescriptions.Clear();
            _cvs.View.SortDescriptions.Add(new SortDescription("AvgRating", ListSortDirection.Descending));
        }

        private void DgBooks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (DgBooks.SelectedItem is BookDisplay book)
                {
                    // Ищем окно, в котором фактически находится эта страница
                    var mainWindow = System.Windows.Window.GetWindow(this) as MainWindow;

                    if (mainWindow != null && mainWindow.MainFrame != null)
                    {
                        mainWindow.MainFrame.Navigate(new BookDetailsPage(book.BookID));
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти главное окно.", "Ошибка");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int bookId = (int)((Button)sender).Tag;

                System.Diagnostics.Debug.WriteLine($"=== ДОБАВЛЕНИЕ КНИГИ ===");
                System.Diagnostics.Debug.WriteLine($"BookID: {bookId}");
                System.Diagnostics.Debug.WriteLine($"UserID: {UserSession.UserId}");
                System.Diagnostics.Debug.WriteLine($"Login: {UserSession.Login}");

                if (UserSession.UserId == 0)
                {
                    MessageBox.Show("❌ Ошибка: пользователь не авторизован!\n\n" +
                                  "UserId = 0\n" +
                                  "Проверьте вход в систему.",
                        "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем, существует ли книга
                var bookExists = Core.Context.Books.Any(b => b.BookID == bookId);
                System.Diagnostics.Debug.WriteLine($"Книга существует: {bookExists}");

                if (!bookExists)
                {
                    MessageBox.Show($"❌ Книга с ID={bookId} не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем, есть ли уже книга в списке
                var existing = Core.Context.ReadingLists
                    .FirstOrDefault(rl => rl.UserID == UserSession.UserId && rl.BookID == bookId);

                System.Diagnostics.Debug.WriteLine($"Существующая запись: {(existing != null ? "найдена" : "не найдена")}");

                if (existing != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Обновление: старый статус = {existing.Status}");
                    existing.Status = "В планах";
                    existing.AddedDate = System.DateTime.Now;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Создание новой записи в ReadingLists");
                    var newListEntry = new ReadingLists
                    {
                        UserID = UserSession.UserId,
                        BookID = bookId,
                        Status = "В планах",
                        AddedDate = System.DateTime.Now
                    };
                    Core.Context.ReadingLists.Add(newListEntry);
                }

                System.Diagnostics.Debug.WriteLine($"Выполнение SaveChanges...");
                int result = Core.Context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"SaveChanges вернул: {result}");

                // ПРОВЕРКА: действительно ли сохранилось
                var check = Core.Context.ReadingLists
                    .Count(rl => rl.UserID == UserSession.UserId && rl.BookID == bookId);
                System.Diagnostics.Debug.WriteLine($"Проверка: записей в базе = {check}");

                if (check > 0)
                {
                    MessageBox.Show("✅ Книга добавлена в список 'В плана'!\n\n" +
                                  $"BookID: {bookId}\n" +
                                  $"UserID: {UserSession.UserId}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("⚠️ SaveChanges выполнился, но данные не сохранились!\n\n" +
                                  "Возможно, проблема с транзакцией или триггером.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ИСКЛЮЧЕНИЕ: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                MessageBox.Show($"❌ Ошибка при добавлении книги:\n\n" +
                               $"Сообщение: {ex.Message}\n\n" +
                               $"Детали: {ex.InnerException?.Message}\n\n" +
                               $"Проверьте Output окно (View → Output) для подробностей.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
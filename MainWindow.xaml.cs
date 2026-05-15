using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
namespace UP_New
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (UserSession.IsAuthor) BtnAuthor.Visibility = Visibility.Visible;
            if (UserSession.IsAdmin) BtnAdmin.Visibility = Visibility.Visible;

            if (UserSession.IsFrozen)
                MessageBox.Show("⚠️ Ваш аккаунт заморожен! Подайте заявку в Профиле.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

            MainFrame.Navigate(new Pages.CatalogPage());
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                switch (btn.Tag.ToString())
                {
                    case "Profile": MainFrame.Navigate(new Pages.ProfilePage()); break;
                    case "Catalog": MainFrame.Navigate(new Pages.CatalogPage()); break;
                    case "Lists": MainFrame.Navigate(new Pages.ReadingListsPage()); break;
                    case "Author": MainFrame.Navigate(new Pages.AuthorPage()); break;
                    case "Admin": MainFrame.Navigate(new Pages.AdminPage()); break;
                }
            }
        }
    }
}
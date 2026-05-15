using System.Data.Entity;
using System.Linq;
using System.Windows;
using UP_New;

namespace UP_New
{
    public partial class LoginWindow : Window
    {
        public LoginWindow() => InitializeComponent();

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var user = Core.Context.Users.FirstOrDefault(u => u.Login == TxtLogin.Text && u.Password == TxtPass.Password);

            if (user != null)
            {
                UserSession.UserId = user.UserID;
                UserSession.RoleId = user.RoleID;
                UserSession.IsFrozen = user.IsFrozen == true;
                UserSession.DisplayName = user.DisplayName;
                UserSession.Login = user.Login;

                // 1. Создаем главное окно
                MainWindow mainWindow = new MainWindow();

                // 2. Назначаем его ГЛАВНЫМ окном приложения
                Application.Current.MainWindow = mainWindow;

                // 3. Показываем и закрываем вход
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Core.Context.Users.Add(new Users
                {
                    Login = TxtLogin.Text,
                    Password = TxtPass.Password,
                    Email = $"{TxtLogin.Text}@mail.com",
                    DisplayName = TxtLogin.Text,
                    RoleID = 1,
                    IsFrozen = false,
                    RegistrationDate = System.DateTime.Now
                });
                Core.Context.SaveChanges();
                MessageBox.Show("Регистрация успешна! Войдите.");
            }
            catch (System.Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }
    }
}
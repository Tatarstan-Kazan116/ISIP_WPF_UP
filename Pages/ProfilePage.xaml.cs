using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UP_New.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();

            // Отображение информации о пользователе
            string roleName = UserSession.RoleId == 1 ? "Читатель" :
                             UserSession.RoleId == 2 ? "Автор" :
                             UserSession.RoleId == 3 ? "Администратор" : "Неизвестно";

            TbInfo.Text = $"Имя: {UserSession.DisplayName}\n" +
                          $"Логин: {UserSession.Login}\n" +
                          $"Роль: {roleName}";

            // Загрузка отзывов
            try
            {
                var reviews = Core.Context.Reviews
                    .Where(r => r.UserID == UserSession.UserId)
                    .Select(r => new
                    {
                        ReviewText = r.ReviewText ?? "",
                        Rating = r.Rating,
                        ReviewDate = r.ReviewDate
                    })
                    .ToList();

                DgMyReviews.ItemsSource = reviews;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отзывов: {ex.Message}");
            }

            // Проверка заморозки
            if (UserSession.IsFrozen)
            {
                TbFreezeWarn.Visibility = Visibility.Visible;
                PnlAppeal.Visibility = Visibility.Visible;
                TbFreezeWarn.Text = "⚠️ Ваш аккаунт заморожен администрацией.";
            }
        }

        // ✅ ДОБАВЛЕНО: Обработчик заявки на роль
        private void RequestRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, не подавал ли уже заявку
                var existingRequest = Core.Context.RoleRequests
                    .FirstOrDefault(rr => rr.UserID == UserSession.UserId && rr.Status == "На рассмотрении");

                if (existingRequest != null)
                {
                    MessageBox.Show("У вас уже есть заявка на рассмотрении.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Добавляем заявку
                Core.Context.RoleRequests.Add(new RoleRequests
                {
                    UserID = UserSession.UserId,
                    RequestDate = System.DateTime.Now,
                    Status = "На рассмотрении"
                });

                Core.Context.SaveChanges();
                MessageBox.Show("✅ Заявка на роль 'Автор' отправлена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при отправке заявки:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ✅ ДОБАВЛЕНО: Обработчик оспаривания заморозки
        private void AppealFreeze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string reason = TxtAppealReason.Text.Trim();

                if (string.IsNullOrWhiteSpace(reason))
                {
                    MessageBox.Show("Пожалуйста, укажите причину оспаривания.", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Добавляем заявку на разморозку
                Core.Context.UnfreezeRequests.Add(new UnfreezeRequests
                {
                    UserID = UserSession.UserId,
                    TargetType = "User",
                    TargetID = UserSession.UserId,
                    Reason = reason,
                    RequestDate = System.DateTime.Now,
                    Status = "На рассмотрении"
                });

                Core.Context.SaveChanges();
                MessageBox.Show("✅ Заявка на разморозку отправлена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищаем поле
                TxtAppealReason.Text = "";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка при отправке заявки:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
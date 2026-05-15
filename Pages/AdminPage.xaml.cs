using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace UP_New.Pages
{
    public class ComplaintDisplay
    {
        public int ComplaintID { get; set; }
        public string Sender { get; set; }
        public string TargetType { get; set; }
        public string Reason { get; set; }
    }

    public class RoleRequestDisplay
    {
        public int RequestID { get; set; }
        public string DisplayName { get; set; }
        public System.DateTime? RequestDate { get; set; }
    }

    public class UserDisplay
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string RoleName { get; set; }
    }

    public class UnfreezeRequestDisplay
    {
        public int RequestID { get; set; }
        public string UserName { get; set; }
        public string TargetType { get; set; }
        public string TargetName { get; set; }
        public string Reason { get; set; }
        public System.DateTime? RequestDate { get; set; }
    }
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadAllData();
        }

        private void LoadAllData()
        {
            LoadComplaints();
            LoadRoleRequests();
            LoadUnfreezeRequests();
            LoadUsers();
        }


        private void LoadUnfreezeRequests()
        {
            try
            {
                var requestsList = Core.Context.UnfreezeRequests
                    .Include(ur => ur.Users)
                    .Where(ur => ur.Status == "На рассмотрении")
                    .ToList();

                var data = requestsList.Select(ur => new UnfreezeRequestDisplay
                {
                    RequestID = ur.RequestID,
                    UserName = ur.Users != null ? ur.Users.DisplayName : "Удалён",
                    TargetType = ur.TargetType,
                    TargetName = GetTargetName(ur.TargetType, ur.TargetID),
                    Reason = ur.Reason,
                    RequestDate = ur.RequestDate
                }).ToList();

                DgUnfreezeRequests.ItemsSource = data;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки заявок на разморозку:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ApproveUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int requestId)
                {
                    var request = Core.Context.UnfreezeRequests
                        .Include(r => r.Users)
                        .FirstOrDefault(x => x.RequestID == requestId);

                    if (request == null)
                    {
                        MessageBox.Show("Заявка не найдена!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show(
                        $"Разморозить {request.TargetType}?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Размораживаем объект
                        if (request.TargetType == "User" && request.Users != null)
                        {
                            request.Users.IsFrozen = false;
                            MessageBox.Show($"✅ Аккаунт '{request.Users.DisplayName}' разморожен!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (request.TargetType == "Book")
                        {
                            var book = Core.Context.Books.FirstOrDefault(b => b.BookID == request.TargetID);
                            if (book != null)
                            {
                                book.IsFrozen = false;
                                MessageBox.Show($"✅ Книга '{book.Title}' разморожена!",
                                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }

                        // Обновляем статус заявки
                        request.Status = "Утверждено";
                        Core.Context.SaveChanges();

                        LoadUnfreezeRequests();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RejectUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int requestId)
                {
                    var request = Core.Context.UnfreezeRequests.FirstOrDefault(x => x.RequestID == requestId);

                    if (request == null)
                    {
                        MessageBox.Show("Заявка не найдена!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show("Отклонить заявку на разморозку?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        request.Status = "Отклонено";
                        Core.Context.SaveChanges();

                        MessageBox.Show("❌ Заявка отклонена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadUnfreezeRequests();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Вспомогательный метод для получения имени объекта
        private string GetTargetName(string targetType, int? targetId)
        {
            if (targetId == null) return "N/A";

            try
            {
                if (targetType == "User")
                {
                    var user = Core.Context.Users.FirstOrDefault(u => u.UserID == targetId);
                    return user != null ? user.DisplayName : "Пользователь не найден";
                }
                else if (targetType == "Book")
                {
                    var book = Core.Context.Books.FirstOrDefault(b => b.BookID == targetId);
                    return book != null ? book.Title : "Книга не найдена";
                }
            }
            catch { }

            return "Ошибка загрузки";
        }
        private void LoadComplaints()
        {
            try
            {
                var complaintsList = Core.Context.Complaints
                    .Include(c => c.Users)
                    .ToList();

                var data = complaintsList.Select(c => new ComplaintDisplay
                {
                    ComplaintID = c.ComplaintID,
                    Sender = c.Users != null ? c.Users.DisplayName : "Неизвестно",
                    TargetType = c.TargetType,
                    Reason = c.Reason
                }).ToList();

                DgComplaints.ItemsSource = data;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки жалоб:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRoleRequests()
        {
            try
            {
                var requestsList = Core.Context.RoleRequests
                    .Include(rr => rr.Users)
                    .Where(rr => rr.Status == "На рассмотрении")
                    .ToList();

                var data = requestsList.Select(rr => new RoleRequestDisplay
                {
                    RequestID = rr.RequestID,
                    DisplayName = rr.Users != null ? rr.Users.DisplayName : "Удалён",
                    RequestDate = rr.RequestDate
                }).ToList();

                DgRoleRequests.ItemsSource = data;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки заявок:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadUsers()
        {
            try
            {
                var usersList = Core.Context.Users
                    .Include(u => u.Roles)
                    .ToList();

                var data = usersList.Select(u => new UserDisplay
                {
                    UserID = u.UserID,
                    Login = u.Login,
                    RoleName = u.Roles != null ? u.Roles.RoleName : "Не назначена"
                }).ToList();

                DgUsers.ItemsSource = data;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка загрузки пользователей:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AcceptComplaint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int complaintId)
                {
                    var result = MessageBox.Show(
                        "Принять жалобу? Книга/отзыв будет удалён.",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        HandleComplaint(complaintId, true);
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка: не удалось определить ID жалобы", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int complaintId)
                {
                    var result = MessageBox.Show(
                        "Отклонить жалобу? Она будет удалена без последствий.",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        HandleComplaint(complaintId, false);
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка: не удалось определить ID жалобы", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleComplaint(int id, bool isAccepted)
        {
            try
            {
                var complaint = Core.Context.Complaints.FirstOrDefault(x => x.ComplaintID == id);

                if (complaint == null)
                {
                    MessageBox.Show("Жалоба не найдена!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Если жалоба принята - удаляем объект жалобы
                if (isAccepted && complaint.TargetType == "Book")
                {
                    var book = Core.Context.Books.FirstOrDefault(b => b.BookID == complaint.TargetID);
                    if (book != null)
                    {
                        book.IsFrozen = true;
                        MessageBox.Show($"📚 Книга '{book.Title}' заморожена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else if (isAccepted && complaint.TargetType == "Review")
                {
                    var review = Core.Context.Reviews.FirstOrDefault(r => r.ReviewID == complaint.TargetID);
                    if (review != null)
                    {
                        Core.Context.Reviews.Remove(review);
                        MessageBox.Show("⭐ Отзыв удалён", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                // Удаляем саму жалобу
                Core.Context.Complaints.Remove(complaint);
                Core.Context.SaveChanges();

                MessageBox.Show(
                    isAccepted ? "✅ Жалоба принята и обработана" : "❌ Жалоба отклонена",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadComplaints();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка обработки жалобы:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApproveRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int requestId)
                {
                    var req = Core.Context.RoleRequests
                        .Include(r => r.Users)
                        .FirstOrDefault(x => x.RequestID == requestId);

                    if (req == null)
                    {
                        MessageBox.Show("Заявка не найдена!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (req.Users != null)
                    {
                        var result = MessageBox.Show(
                            $"Утвердить заявку пользователя '{req.Users.DisplayName}' на роль Автор?",
                            "Подтверждение",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            req.Users.RoleID = 2; // Роль Автор
                            req.Status = "Утверждено";
                            Core.Context.SaveChanges();

                            MessageBox.Show($"✅ Пользователь '{req.Users.DisplayName}' получил роль 'Автор'!",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                            LoadRoleRequests();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка: не удалось определить ID заявки", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RejectRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int requestId)
                {
                    var req = Core.Context.RoleRequests.FirstOrDefault(x => x.RequestID == requestId);

                    if (req == null)
                    {
                        MessageBox.Show("Заявка не найдена!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var result = MessageBox.Show("Отклонить заявку?", "Подтверждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        req.Status = "Отклонено";
                        Core.Context.SaveChanges();

                        MessageBox.Show("❌ Заявка отклонена", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadRoleRequests();
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка: не удалось определить ID заявки", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ComboBox comboBox &&
                    comboBox.SelectedItem is ComboBoxItem selectedItem &&
                    comboBox.Tag is int userId)
                {
                    if (int.TryParse(selectedItem.Content.ToString(), out int newRoleId))
                    {
                        var user = Core.Context.Users.First(u => u.UserID == userId);
                        user.RoleID = newRoleId;
                        Core.Context.SaveChanges();

                        string roleName = newRoleId == 1 ? "Читатель" :
                                         newRoleId == 2 ? "Автор" : "Администратор";

                        MessageBox.Show($"✅ Роль пользователя изменена на '{roleName}'",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadUsers();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка смены роли:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FreezeUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is int userId)
                {
                    var result = MessageBox.Show(
                        "Заморозить аккаунт пользователя? Он не сможет войти в систему.",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        var user = Core.Context.Users.First(u => u.UserID == userId);
                        user.IsFrozen = true;
                        Core.Context.SaveChanges();

                        MessageBox.Show($"✅ Пользователь ID={userId} заморожен",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadUsers();
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка: не удалось определить ID пользователя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"❌ Ошибка заморозки:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
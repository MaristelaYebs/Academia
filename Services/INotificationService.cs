using Academia.Models;
using Academia.Models.ViewModels;

namespace Academia.Services
{
    public interface INotificationService
    {
        Task<List<NotificationItemViewModel>> GetNotificationsAsync(ApplicationUser user);
        Task<int> GetUnreadCountAsync(ApplicationUser user);
    }
}

using EduTrack.Models;
using EduTrack.Models.ViewModels;

namespace EduTrack.Services
{
    public interface INotificationService
    {
        Task<List<NotificationItemViewModel>> GetNotificationsAsync(ApplicationUser user);
        Task<int> GetUnreadCountAsync(ApplicationUser user);
    }
}

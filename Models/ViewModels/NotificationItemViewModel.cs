using System;

namespace EduTrack.Models.ViewModels
{
    public class NotificationItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Link { get; set; } = string.Empty;
    }
}

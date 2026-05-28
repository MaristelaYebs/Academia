using System;

namespace EduTrack.Models.ViewModels
{
    public class TodoItemViewModel
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SortDate { get; set; }
        public string ActionText { get; set; } = string.Empty;
        public string ActionUrl { get; set; } = string.Empty;
    }
}

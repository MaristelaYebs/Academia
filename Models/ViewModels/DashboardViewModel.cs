using System.Collections.Generic;

namespace Academia.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalAssessments { get; set; }
        public List<Course> RecentCourses { get; set; } = new();
        public List<Enrollment> RecentEnrollments { get; set; } = new();
        public double AverageScore { get; set; }
    }
}
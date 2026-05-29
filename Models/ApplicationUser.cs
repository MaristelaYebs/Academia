using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Academia.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Student"; // Student, Teacher, Admin
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Course> TaughtCourses { get; set; } = new List<Course>();
        public ICollection<AssessmentSubmission> Submissions { get; set; } = new List<AssessmentSubmission>();
    }
}
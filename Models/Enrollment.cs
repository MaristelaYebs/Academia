using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Academia.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public ApplicationUser? Student { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.Now;
        public double Progress { get; set; } = 0; // 0-100%
        public string Status { get; set; } = "Active"; // Active, Completed, Dropped
    }
}
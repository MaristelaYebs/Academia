using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Academia.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string TeacherId { get; set; } = string.Empty;

        [ForeignKey("TeacherId")]
        public ApplicationUser? Teacher { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    }
}
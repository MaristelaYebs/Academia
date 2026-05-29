using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Academia.Models
{
    public class Assessment
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = "Quiz"; // Quiz, Assignment, Exam
        public int MaxScore { get; set; } = 100;
        public DateTime? DueDate { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public ICollection<AssessmentSubmission> Submissions { get; set; } = new List<AssessmentSubmission>();
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduTrack.Models
{
    public class AssessmentSubmission
    {
        public int Id { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey("StudentId")]
        public ApplicationUser? Student { get; set; }

        public int AssessmentId { get; set; }
        public Assessment? Assessment { get; set; }

        public string AnswerText { get; set; } = string.Empty;
        public double? Score { get; set; }
        public string? Feedback { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Submitted"; // Submitted, Graded
    }
}
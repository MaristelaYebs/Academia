using EduTrack.Data;
using EduTrack.Models;
using EduTrack.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationItemViewModel>> GetNotificationsAsync(ApplicationUser user)
        {
            var notifications = new List<NotificationItemViewModel>();

            if (user.Role == "Student")
            {
                var enrolledCourseIds = await _context.Enrollments
                    .Where(e => e.StudentId == user.Id)
                    .Select(e => e.CourseId)
                    .ToListAsync();

                var lessons = await _context.Lessons
                    .Where(l => enrolledCourseIds.Contains(l.CourseId))
                    .Include(l => l.Course)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(20)
                    .ToListAsync();

                foreach (var lesson in lessons)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Title = "New lesson uploaded",
                        Message = $"{lesson.Course?.Title}: {lesson.Title}",
                        Type = "Lesson",
                        Timestamp = lesson.CreatedAt,
                        Link = $"/Courses/Details/{lesson.CourseId}"
                    });
                }

                var assessments = await _context.Assessments
                    .Where(a => enrolledCourseIds.Contains(a.CourseId))
                    .Include(a => a.Course)
                    .OrderByDescending(a => a.Id)
                    .Take(20)
                    .ToListAsync();

                foreach (var assessment in assessments)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Title = "New activity posted",
                        Message = $"{assessment.Course?.Title}: {assessment.Title}",
                        Type = assessment.Type,
                        Timestamp = assessment.DueDate ?? DateTime.Now,
                        Link = $"/Assessments/Details/{assessment.Id}"
                    });
                }
            }
            else if (user.Role == "Teacher")
            {
                var teacherCourseIds = await _context.Courses
                    .Where(c => c.TeacherId == user.Id)
                    .Select(c => c.Id)
                    .ToListAsync();

                var enrollments = await _context.Enrollments
                    .Where(e => teacherCourseIds.Contains(e.CourseId))
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrolledAt)
                    .Take(20)
                    .ToListAsync();

                foreach (var enrollment in enrollments)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Title = "New student enrollment",
                        Message = $"{enrollment.Student?.FullName} enrolled in {enrollment.Course?.Title}",
                        Type = "Enrollment",
                        Timestamp = enrollment.EnrolledAt,
                        Link = $"/Courses/Details/{enrollment.CourseId}"
                    });
                }

                var submissions = await _context.AssessmentSubmissions
                    .Where(s => teacherCourseIds.Contains(s.Assessment!.CourseId))
                    .Include(s => s.Student)
                    .Include(s => s.Assessment).ThenInclude(a => a!.Course)
                    .OrderByDescending(s => s.SubmittedAt)
                    .Take(20)
                    .ToListAsync();

                foreach (var submission in submissions)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Title = "New assessment submission",
                        Message = $"{submission.Student?.FullName} submitted {submission.Assessment?.Title}",
                        Type = "Submission",
                        Timestamp = submission.SubmittedAt,
                        Link = $"/Assessments/Details/{submission.AssessmentId}"
                    });
                }
            }
            else // Admin
            {
                var enrollments = await _context.Enrollments
                    .Include(e => e.Student)
                    .Include(e => e.Course)
                    .OrderByDescending(e => e.EnrolledAt)
                    .Take(20)
                    .ToListAsync();

                foreach (var enrollment in enrollments)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Title = "New student enrollment",
                        Message = $"{enrollment.Student?.FullName} enrolled in {enrollment.Course?.Title}",
                        Type = "Enrollment",
                        Timestamp = enrollment.EnrolledAt,
                        Link = $"/Courses/Details/{enrollment.CourseId}"
                    });
                }

                var submissions = await _context.AssessmentSubmissions
                    .Include(s => s.Student)
                    .Include(s => s.Assessment).ThenInclude(a => a!.Course)
                    .OrderByDescending(s => s.SubmittedAt)
                    .Take(20)
                    .ToListAsync();

                foreach (var submission in submissions)
                {
                    notifications.Add(new NotificationItemViewModel
                    {
                        Title = "New assessment submission",
                        Message = $"{submission.Student?.FullName} submitted {submission.Assessment?.Title}",
                        Type = "Submission",
                        Timestamp = submission.SubmittedAt,
                        Link = $"/Assessments/Details/{submission.AssessmentId}"
                    });
                }
            }

            return notifications
                .OrderByDescending(n => n.Timestamp)
                .Take(30)
                .ToList();
        }

        public async Task<int> GetUnreadCountAsync(ApplicationUser user)
        {
            var notifications = await GetNotificationsAsync(user);
            return notifications.Take(10).Count();
        }
    }
}

using EduTrack.Data;
using EduTrack.Models;
using EduTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize(Roles = "Student")]
    public class TodoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodoController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == user.Id)
                .Select(e => e.CourseId)
                .ToListAsync();

            var lessons = await _context.Lessons
                .Where(l => enrolledCourseIds.Contains(l.CourseId))
                .Include(l => l.Course)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var submissions = await _context.AssessmentSubmissions
                .Where(s => s.StudentId == user.Id)
                .Select(s => s.AssessmentId)
                .ToListAsync();

            var assessments = await _context.Assessments
                .Where(a => enrolledCourseIds.Contains(a.CourseId))
                .Include(a => a.Course)
                .OrderBy(a => a.DueDate ?? DateTime.MaxValue)
                .ToListAsync();

            var items = new List<TodoItemViewModel>();

            items.AddRange(lessons.Select(lesson => new TodoItemViewModel
            {
                ActivityType = "Lesson",
                Title = lesson.Title,
                CourseTitle = lesson.Course?.Title ?? "Course",
                Status = "New lesson",
                SortDate = lesson.CreatedAt,
                ActionText = "Open Course",
                ActionUrl = $"/Courses/Details/{lesson.CourseId}"
            }));

            items.AddRange(assessments.Select(assessment => new TodoItemViewModel
            {
                ActivityType = assessment.Type,
                Title = assessment.Title,
                CourseTitle = assessment.Course?.Title ?? "Course",
                Status = submissions.Contains(assessment.Id) ? "Submitted" : "Pending",
                SortDate = assessment.DueDate ?? DateTime.Now,
                ActionText = submissions.Contains(assessment.Id) ? "View Submission" : "Open Activity",
                ActionUrl = $"/Assessments/Details/{assessment.Id}"
            }));

            var orderedItems = items
                .OrderBy(i => i.Status == "Pending" ? 0 : 1)
                .ThenBy(i => i.SortDate)
                .ToList();

            return View(orderedItems);
        }
    }
}

using Academia.Data;
using Academia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Academia.Controllers
{
    [Authorize]
    public class AssessmentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssessmentsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Teacher,Admin")]
        public IActionResult Create(int courseId) { ViewBag.CourseId = courseId; return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create(Assessment model)
        {
            if (!ModelState.IsValid) { ViewBag.CourseId = model.CourseId; return View(model); }
            _context.Assessments.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Assessment created!";
            return RedirectToAction("Details", "Courses", new { id = model.CourseId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var assessment = await _context.Assessments
                .Include(a => a.Course)
                .Include(a => a.Submissions).ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (assessment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserSubmission = assessment.Submissions.FirstOrDefault(s => s.StudentId == user!.Id);
            ViewBag.CurrentUserId = user?.Id;
            return View(assessment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> Submit(int assessmentId, string answerText)
        {
            var user = await _userManager.GetUserAsync(User);
            var already = await _context.AssessmentSubmissions
                .AnyAsync(s => s.StudentId == user!.Id && s.AssessmentId == assessmentId);

            if (!already)
            {
                _context.AssessmentSubmissions.Add(new AssessmentSubmission
                {
                    StudentId = user!.Id,
                    AssessmentId = assessmentId,
                    AnswerText = answerText
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Submitted successfully!";
            }
            return RedirectToAction("Details", new { id = assessmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Grade(int submissionId, double score, string feedback)
        {
            var submission = await _context.AssessmentSubmissions.FindAsync(submissionId);
            if (submission != null)
            {
                submission.Score = score;
                submission.Feedback = feedback;
                submission.Status = "Graded";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Graded!";

                // Update enrollment progress
                var assessment = await _context.Assessments.FindAsync(submission.AssessmentId);
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.StudentId == submission.StudentId && e.CourseId == assessment!.CourseId);
                if (enrollment != null)
                {
                    var allSubmissions = await _context.AssessmentSubmissions
                        .Where(s => s.StudentId == submission.StudentId && s.Assessment!.CourseId == assessment!.CourseId && s.Score != null)
                        .ToListAsync();
                    var totalAssessments = await _context.Assessments.CountAsync(a => a.CourseId == assessment!.CourseId);
                    enrollment.Progress = totalAssessments > 0 ? (double)allSubmissions.Count / totalAssessments * 100 : 0;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Details", new { id = submission.AssessmentId });
            }
            return RedirectToAction("Index", "Courses");
        }

        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var a = await _context.Assessments.FindAsync(id);
            if (a == null) return NotFound();
            return View(a);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Edit(int id, Assessment model)
        {
            if (!ModelState.IsValid) return View(model);
            var existing = await _context.Assessments.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.Type = model.Type;
            existing.MaxScore = model.MaxScore;
            existing.DueDate = model.DueDate;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Assessment updated!";
            return RedirectToAction("Details", "Courses", new { id = existing.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var a = await _context.Assessments.FindAsync(id);
            if (a != null)
            {
                int cid = a.CourseId;
                _context.Assessments.Remove(a);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Courses", new { id = cid });
            }
            return RedirectToAction("Index", "Courses");
        }
    }
}
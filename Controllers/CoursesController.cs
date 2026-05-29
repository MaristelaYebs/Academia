using Academia.Data;
using Academia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Academia.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoursesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string search = "", string category = "")
        {
            var query = _context.Courses.Include(c => c.Teacher).Include(c => c.Enrollments).AsQueryable();

            if (User.IsInRole("Teacher"))
            {
                var user = await _userManager.GetUserAsync(User);
                query = query.Where(c => c.TeacherId == user!.Id);
            }
            else if (User.IsInRole("Student"))
            {
                query = query.Where(c => c.IsPublished);
            }

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Title.Contains(search) || c.CourseCode.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.Category == category);

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Categories = await _context.Courses.Select(c => c.Category).Distinct().ToListAsync();
            return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons.OrderBy(l => l.Order))
                .Include(c => c.Assessments)
                .Include(c => c.Enrollments).ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            ViewBag.IsEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == user!.Id && e.CourseId == id);
            ViewBag.CurrentUserId = user?.Id;
            if (User.IsInRole("Admin"))
            {
                var enrolledStudentIds = course.Enrollments.Select(e => e.StudentId).ToHashSet();
                var students = await _userManager.GetUsersInRoleAsync("Student");
                ViewBag.AvailableStudents = students
                    .Where(s => !enrolledStudentIds.Contains(s.Id))
                    .OrderBy(s => s.FullName)
                    .ToList();
            }
            return View(course);
        }

        [Authorize(Roles = "Teacher,Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create(Course model)
        {
            var user = await _userManager.GetUserAsync(User);
            model.TeacherId = user!.Id;
            ModelState.Remove(nameof(Course.TeacherId));

            if (!ModelState.IsValid) return View(model);

            model.CreatedAt = DateTime.Now;

            _context.Courses.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Course created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Edit(int id, Course model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var existing = await _context.Courses.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = model.Title;
            existing.Description = model.Description;
            existing.CourseCode = model.CourseCode;
            existing.Category = model.Category;
            existing.IsPublished = model.IsPublished;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Course updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses.Include(c => c.Teacher).FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Course deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollByCode(string courseCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (string.IsNullOrWhiteSpace(courseCode))
            {
                TempData["Error"] = "Please enter a course code.";
                return RedirectToAction(nameof(Index));
            }

            var trimmedCode = courseCode.Trim();
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseCode == trimmedCode && c.IsPublished);
            if (course == null)
            {
                TempData["Error"] = "Invalid course code or the course is not published.";
                return RedirectToAction(nameof(Index));
            }

            var already = await _context.Enrollments.AnyAsync(e => e.StudentId == user!.Id && e.CourseId == course.Id);
            if (!already)
            {
                _context.Enrollments.Add(new Enrollment { StudentId = user!.Id, CourseId = course.Id });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Enrolled successfully!";
            }
            else
            {
                TempData["Success"] = "You are already enrolled in this course.";
            }

            return RedirectToAction(nameof(Details), new { id = course.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnrollStudent(int courseId, string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
            {
                TempData["Error"] = "Please select a student.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var student = await _userManager.FindByIdAsync(studentId);
            if (student == null || !await _userManager.IsInRoleAsync(student, "Student"))
            {
                TempData["Error"] = "Selected user is not a valid student.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            var already = await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
            if (already)
            {
                TempData["Success"] = $"{student.FullName} is already enrolled.";
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            _context.Enrollments.Add(new Enrollment { StudentId = studentId, CourseId = courseId });
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{student.FullName} enrolled successfully.";
            return RedirectToAction(nameof(Details), new { id = courseId });
        }
    }
}
using EduTrack.Data;
using EduTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string search = "")
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            if (!string.IsNullOrEmpty(search))
                students = students.Where(s => s.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || s.Email!.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            ViewBag.Search = search;
            return View(students);
        }

        public async Task<IActionResult> Details(string id)
        {
            var student = await _userManager.FindByIdAsync(id);
            if (student == null) return NotFound();

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == id)
                .Include(e => e.Course)
                .ToListAsync();

            var submissions = await _context.AssessmentSubmissions
                .Where(s => s.StudentId == id)
                .Include(s => s.Assessment)
                .ToListAsync();

            ViewBag.Enrollments = enrollments;
            ViewBag.Submissions = submissions;
            return View(student);
        }
    }
}
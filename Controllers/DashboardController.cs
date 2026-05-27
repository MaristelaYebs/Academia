using EduTrack.Data;
using EduTrack.Models;
using EduTrack.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EduTrack.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var vm = new DashboardViewModel();

            if (User.IsInRole("Admin"))
            {
                vm.TotalCourses = await _context.Courses.CountAsync();
                vm.TotalStudents = (await _userManager.GetUsersInRoleAsync("Student")).Count;
                vm.TotalEnrollments = await _context.Enrollments.CountAsync();
                vm.TotalAssessments = await _context.Assessments.CountAsync();
                vm.RecentCourses = await _context.Courses
                    .Include(c => c.Teacher)
                    .OrderByDescending(c => c.CreatedAt).Take(5).ToListAsync();
                vm.RecentEnrollments = await _context.Enrollments
                    .Include(e => e.Student).Include(e => e.Course)
                    .OrderByDescending(e => e.EnrolledAt).Take(5).ToListAsync();
                var scores = await _context.AssessmentSubmissions.Where(s => s.Score != null).Select(s => s.Score!.Value).ToListAsync();
                vm.AverageScore = scores.Count > 0 ? scores.Average() : 0;
            }
            else if (User.IsInRole("Teacher"))
            {
                vm.TotalCourses = await _context.Courses.Where(c => c.TeacherId == user!.Id).CountAsync();
                vm.TotalEnrollments = await _context.Enrollments
                    .Where(e => e.Course!.TeacherId == user!.Id).CountAsync();
                vm.TotalAssessments = await _context.Assessments
                    .Where(a => a.Course!.TeacherId == user!.Id).CountAsync();
                vm.RecentCourses = await _context.Courses
                    .Where(c => c.TeacherId == user!.Id)
                    .OrderByDescending(c => c.CreatedAt).Take(5).ToListAsync();
            }
            else // Student
            {
                vm.TotalCourses = await _context.Enrollments.Where(e => e.StudentId == user!.Id).CountAsync();
                vm.TotalAssessments = await _context.AssessmentSubmissions.Where(s => s.StudentId == user!.Id).CountAsync();
                var scores = await _context.AssessmentSubmissions
                    .Where(s => s.StudentId == user!.Id && s.Score != null)
                    .Select(s => s.Score!.Value).ToListAsync();
                vm.AverageScore = scores.Count > 0 ? scores.Average() : 0;
                vm.RecentEnrollments = await _context.Enrollments
                    .Where(e => e.StudentId == user!.Id)
                    .Include(e => e.Course).ThenInclude(c => c!.Teacher)
                    .OrderByDescending(e => e.EnrolledAt).Take(5).ToListAsync();
            }

            ViewBag.UserName = user?.FullName;
            ViewBag.UserRole = user?.Role;
            return View(vm);
        }
    }
}
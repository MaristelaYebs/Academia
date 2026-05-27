using EduTrack.Data;
using EduTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EduTrack.Controllers
{
	[Authorize]
	public class LessonsController : Controller
	{
		private readonly AppDbContext _context;

		public LessonsController(AppDbContext context) => _context = context;

		[Authorize(Roles = "Teacher,Admin")]
		public IActionResult Create(int courseId)
		{
			ViewBag.CourseId = courseId;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Teacher,Admin")]
		public async Task<IActionResult> Create(Lesson model)
		{
			if (!ModelState.IsValid) { ViewBag.CourseId = model.CourseId; return View(model); }
			_context.Lessons.Add(model);
			await _context.SaveChangesAsync();
			TempData["Success"] = "Lesson added!";
			return RedirectToAction("Details", "Courses", new { id = model.CourseId });
		}

		[Authorize(Roles = "Teacher,Admin")]
		public async Task<IActionResult> Edit(int id)
		{
			var lesson = await _context.Lessons.FindAsync(id);
			if (lesson == null) return NotFound();
			return View(lesson);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Teacher,Admin")]
		public async Task<IActionResult> Edit(int id, Lesson model)
		{
			if (!ModelState.IsValid) return View(model);
			var existing = await _context.Lessons.FindAsync(id);
			if (existing == null) return NotFound();
			existing.Title = model.Title;
			existing.Content = model.Content;
			existing.Order = model.Order;
			await _context.SaveChangesAsync();
			TempData["Success"] = "Lesson updated!";
			return RedirectToAction("Details", "Courses", new { id = existing.CourseId });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Teacher,Admin")]
		public async Task<IActionResult> Delete(int id)
		{
			var lesson = await _context.Lessons.FindAsync(id);
			if (lesson != null)
			{
				int cid = lesson.CourseId;
				_context.Lessons.Remove(lesson);
				await _context.SaveChangesAsync();
				TempData["Success"] = "Lesson deleted.";
				return RedirectToAction("Details", "Courses", new { id = cid });
			}
			return RedirectToAction("Index", "Courses");
		}
	}
}
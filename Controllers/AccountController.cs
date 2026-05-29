using Academia.Models;
using Academia.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Academia.Controllers
{
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AccountController(UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		[HttpGet]
		public IActionResult Login() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
			if (result.Succeeded)
				return RedirectToAction("Index", "Dashboard");

			ModelState.AddModelError("", "Invalid email or password.");
			return View(model);
		}

		[HttpGet]
		public IActionResult Register() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			var user = new ApplicationUser
			{
				FullName = model.FullName,
				UserName = model.Email,
				Email = model.Email,
				Role = model.Role
			};

			var result = await _userManager.CreateAsync(user, model.Password);
			if (result.Succeeded)
			{
				await _userManager.AddToRoleAsync(user, model.Role);
				await _signInManager.SignInAsync(user, false);
				return RedirectToAction("Index", "Dashboard");
			}

			foreach (var error in result.Errors)
				ModelState.AddModelError("", error.Description);

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction("Login");
		}

		public IActionResult AccessDenied() => View();
	}
}
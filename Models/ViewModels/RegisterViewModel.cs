using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models.ViewModels
{
	public class RegisterViewModel
	{
		[Required, MaxLength(100)]
		public string FullName { get; set; } = string.Empty;

		[Required, EmailAddress]
		public string Email { get; set; } = string.Empty;

		[Required, DataType(DataType.Password), MinLength(6)]
		public string Password { get; set; } = string.Empty;

		[Compare("Password"), DataType(DataType.Password)]
		public string ConfirmPassword { get; set; } = string.Empty;

		[Required]
		public string Role { get; set; } = "Student";
	}
}
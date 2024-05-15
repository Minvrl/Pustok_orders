using System.ComponentModel.DataAnnotations;

namespace MVC_Pustok.ViewModels
{
	public class MemberLoginViewModel
	{
		[Required]
		[MinLength(10)]
		[MaxLength(50)]
		[EmailAddress]
		public string Email { get; set; }
		[Required]
		[MinLength(8)]
		[MaxLength(50)]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Pustok.Areas.Admin.ViewModels;
using MVC_Pustok.Models;

namespace MVC_Pustok.Areas.Admin.Controllers
{
    [Area("admin")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
       
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(AdminViewModel logvm,string returnUrl)
        {
            AppUser admin = await _userManager.FindByNameAsync(logvm.UserName);

            if (admin == null)
            {
                ModelState.AddModelError("", "Username or password is incorrect !");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(admin,logvm.Password,false,false);

            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Username or password is incorrect !");
                return View();
            }
			return returnUrl != null ? Redirect(returnUrl) : RedirectToAction("index", "dashboard");
		}

        public IActionResult GetName()
        {
            var username = User.Identity.Name;
            return Json(User.Identity);
        }

        public async Task<IActionResult> CreateRoles()
        {
            await _roleManager.CreateAsync(new IdentityRole("admin"));
            await _roleManager.CreateAsync(new IdentityRole("super_admin"));
            await _roleManager.CreateAsync(new IdentityRole("member"));
            return Ok();

        }

        public async Task<IActionResult> CreateAdmin()
        {
            AppUser admin = new AppUser
            {
                UserName = "mimy",
                Email = "mimy"
            };

            var r = await _userManager.CreateAsync(admin, "mimmi111");
            await _userManager.AddToRoleAsync(admin, "super_admin");

            return Json(r);
        }

        [Authorize(Roles = "admin,super_admin")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("login", "account");
        }

    }
}



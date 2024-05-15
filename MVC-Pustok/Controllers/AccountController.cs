using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Pustok.Areas.Admin.ViewModels;
using MVC_Pustok.Data;
using MVC_Pustok.Models;
using MVC_Pustok.ViewModels;
using System.Drawing;
using System.Security.Claims;

namespace MVC_Pustok.Controllers
{
	public class AccountController : Controller
	{
		public UserManager<AppUser> _userManager { get; set; }
        public AppDbContext _context { get; set; }
		private readonly SignInManager<AppUser> _signInManager;
		public AccountController(UserManager<AppUser> userManager, AppDbContext context, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
        }
        public IActionResult Register()
		{
			return View();
		}

        [HttpPost]
        public async Task<IActionResult> Register(MemberRegViewModel regvm)
        {
            if (!ModelState.IsValid) return View();

            AppUser user = new AppUser 
            { 
                FullName = regvm.FullName,
                UserName = regvm.UserName,
                Email = regvm.Email
            };
            var r = await _userManager.CreateAsync(user, regvm.Password);

            if (!r.Succeeded)
            {
                foreach (var err in r.Errors)
                {
                    if (err.Code == "DuplicateUserName")
                        ModelState.AddModelError("UserName", "UserName is already taken");
                    else ModelState.AddModelError("", err.Description);
                }
                return View();
            }

            await _userManager.AddToRoleAsync(user, "member");
            return RedirectToAction("index", "home");

        }

        [Authorize(Roles = "member")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [Authorize(Roles = "member")]
        public async Task<IActionResult> Profile(string tab = "dashboard")
        {
            AppUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("login", "account");
            }
            ProfileViewModel pvm = new ProfileViewModel 
            {
                ProfEditVM = new ProfileEditViewModel 
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    UserName = user.UserName
                }
            };
            ViewBag.Tab = tab;
            return View(pvm);

        }

        [Authorize(Roles = "member")]
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileEditViewModel pEditVM, string tab = "profile")
        {
            ViewBag.Tab = tab;
            ProfileViewModel profileVM = new ProfileViewModel();
            profileVM.ProfEditVM = pEditVM;

            if (!ModelState.IsValid) return View(profileVM);

            AppUser? user = await _userManager.GetUserAsync(User);

            if (user == null) return RedirectToAction("login", "account");

            user.UserName = pEditVM.UserName;
            user.Email = pEditVM.Email;
            user.FullName = pEditVM.FullName;

            if (_userManager.Users.Any(x => x.Id != User.FindFirstValue(ClaimTypes.NameIdentifier) && x.NormalizedEmail == pEditVM.Email.ToUpper()))
            {
                ModelState.AddModelError("Email", "Email is already taken");
                return View(profileVM);
            }

            if (pEditVM.NewPassword != null)
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, pEditVM.CurrentPassword, pEditVM.NewPassword);

                if (!passwordResult.Succeeded)
                {
                    foreach (var err in passwordResult.Errors)
                        ModelState.AddModelError("", err.Description);

                    return View(profileVM);
                }
            }


            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    if (err.Code == "DuplicateUserName")
                        ModelState.AddModelError("UserName", "UserName is already taken");
                    else ModelState.AddModelError("", err.Description);
                }
                return View(profileVM);
            }

            await _signInManager.SignInAsync(user, false);

            return View(profileVM);
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(MemberLoginViewModel logvm)
        {
            AppUser user = await _userManager.FindByEmailAsync(logvm.Email);
            if(user == null)
            {
                ModelState.AddModelError("", "Email or password incorrect !");
                return View();
            }

            var r = await _signInManager.PasswordSignInAsync(user, logvm.Password, false, false);
            if (!r.Succeeded)
            {
				ModelState.AddModelError("", "Email or password incorrect !");
				return View();
			}
			return RedirectToAction("index", "home");
		}

	}
}

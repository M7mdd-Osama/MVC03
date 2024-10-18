using Demo.DAL.Models;
using Demo.PL.Helpers;
using Demo.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Demo.PL.Controllers
{
	//[AllowAnonymous]
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;

		public AccountController(UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		#region Register
		public IActionResult Register()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var User = new ApplicationUser()
				{
					UserName = model.Email.Split('@')[0],
					Email = model.Email,
					Fname = model.FName,
					Lname = model.LName,
					IsAgree = model.IsAgree,
				};
				var Result = await _userManager.CreateAsync(User, model.Password);
				if (Result.Succeeded)
					return RedirectToAction(nameof(Login));
				else
					foreach (var error in Result.Errors)
						ModelState.AddModelError(string.Empty, error.Description);
			}
			return View(model);
		}

		#endregion

		#region Login
		public IActionResult Login()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var User = await _userManager.FindByEmailAsync(model.Email);
				if (User is not null)
				{
					var Flag = await _userManager.CheckPasswordAsync(User, model.Password);
					if (Flag)
					{
						var Result = await _signInManager.PasswordSignInAsync(User, model.Password, model.RememberMe, false);
						if (Result.Succeeded)
						{
							return RedirectToAction("Index", "Home");
						}
					}
					else
					{
						ModelState.AddModelError(string.Empty, "Incorrect Password");
					}
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Email is not Exists");
				}
			}
			return View(model);
		}
		#endregion

		#region Sign Out
		public new async Task<IActionResult> SignOut()
		{
			await _signInManager.SignOutAsync();
			return RedirectToAction(nameof(Login));
		}
		#endregion

		public IActionResult ForgetPassword()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> SendEmail(ForgetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var User = await _userManager.FindByEmailAsync(model.Email);
				if (User is not null)
				{
					var token = await _userManager.GeneratePasswordResetTokenAsync(User);
					var ResetPasswordLink = Url.Action("ResetPassword", "Account", new { email = User.Email, Token = token }, Request.Scheme);

					var email = new Email()
					{
						Subject = "Reset Password",
						To = model.Email,
						Body = ResetPasswordLink
					};
					EmailSettings.SendEmail(email);
					return RedirectToAction(nameof(CheckYourInbox));
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Email is not Exists");
				}
			}
			return View("ForgetPassword", model);
		}
		public IActionResult CheckYourInbox()
		{
			return View();
		}
	}
}

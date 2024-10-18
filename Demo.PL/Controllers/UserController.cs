using AutoMapper;
using Demo.DAL.Models;
using Demo.PL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.PL.Controllers
{
	[Authorize]
	public class UserController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IMapper _mapper;

		public UserController(UserManager<ApplicationUser> userManager,
			IMapper mapper)
		{
			_userManager = userManager;
			_mapper = mapper;
		}
		public async Task<IActionResult> Index(string SearchValue)
		{
			if (string.IsNullOrEmpty(SearchValue))
			{
				var Users = await _userManager.Users.Select(
					U => new UserViewModel()
					{
						Id = U.Id,
						FName = U.Fname,
						LName = U.Lname,
						Email = U.Email,
						PhoneNumber = U.PhoneNumber,
						Roles = _userManager.GetRolesAsync(U).Result
					}).ToListAsync();
				return View(Users);
			}
			else
			{
				var User = await _userManager.FindByNameAsync(SearchValue);
				var MappedUser = new UserViewModel()
				{
					Id = User.Id,
					FName = User.Fname,
					LName = User.Lname,
					Email = User.Email,
					PhoneNumber = User.PhoneNumber,
					Roles = _userManager.GetRolesAsync(User).Result
				};
				return View(new List<UserViewModel> { MappedUser });
			}
		}
		public async Task<IActionResult> Details(string Id, string ViewName = "Details")
		{
			if (Id is null)
				return BadRequest();
			var User = await _userManager.FindByIdAsync(Id);
			if (User is null)
				return NotFound();
			var MappedUser = _mapper.Map<ApplicationUser, UserViewModel>(User);
			return View(ViewName, MappedUser);
		}

		public async Task<IActionResult> Edit(string Id)
		{
			return await Details(Id, "Edit");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(UserViewModel model, [FromRoute] string id)
		{
			if (id != model.Id)
				return BadRequest();
			if (ModelState.IsValid)
			{
				try
				{
					var User = await _userManager.FindByIdAsync(id);
					User.PhoneNumber = model.PhoneNumber;
					User.Fname = model.FName;
					User.Lname = model.LName;
					await _userManager.UpdateAsync(User);
					return RedirectToAction(nameof(Index));
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}
			return View(model);
		}

		public async Task<IActionResult> Delete(string id)
		{
			return await Details(id, "Delete");
		}
		[HttpPost]
		public async Task<IActionResult> ConfirmDelete( string id)
		{
			try
			{
				var User = await _userManager.FindByIdAsync(id);
				await _userManager.DeleteAsync(User);
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return RedirectToAction("Error", "Home");
			}
		}
	}
}

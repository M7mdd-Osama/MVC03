using AutoMapper;
using Demo.DAL.Models;
using Demo.PL.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.PL.Controllers
{
	public class RoleController : Controller
	{
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IMapper _mapper;

		public RoleController(RoleManager<IdentityRole> roleManager,
			IMapper mapper)
		{
			_roleManager = roleManager;
			_mapper = mapper;
		}
		public async Task<IActionResult> Index(string SearchValue)
		{
			if (string.IsNullOrEmpty(SearchValue))
			{
				var Roles = await _roleManager.Roles.ToListAsync();
				var MappedRole = _mapper.Map<IEnumerable<IdentityRole>, IEnumerable<RoleViewModel>>(Roles);
				return View(MappedRole);
			}
			else
			{
				var Role = await _roleManager.FindByNameAsync(SearchValue);
				var MappedRole = _mapper.Map<IdentityRole, RoleViewModel>(Role);
				return View(new List<RoleViewModel>() { MappedRole});
			}
		}
	}
}

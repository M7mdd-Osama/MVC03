using AutoMapper;
using Demo.BLL.Interfaces;
using Demo.BLL.Repositories;
using Demo.DAL.Contexts;
using Demo.DAL.Models;
using Demo.PL.MappingProfiles;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.PL
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var Builder = WebApplication.CreateBuilder(args);
			#region Configure Services That Allow Dependancy Injection

			Builder.Services.AddControllersWithViews();
			Builder.Services.AddDbContext<MvcAppDbContext>(options =>
			{
				options.UseSqlServer(Builder.Configuration.GetConnectionString("DefaultConnection"));
			}, ServiceLifetime.Scoped); //Allow Dependancy Injection
			Builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
			Builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

			Builder.Services.AddAutoMapper(M => M.AddProfiles(new List<Profile>() { new EmployeeProfile(), new UserProfile(), new RoleProfile() }));
			//services.AddAutoMapper(M => M.AddProfile(new EmployeeProfile()));
			//services.AddAutoMapper(M => M.AddProfile(new UserProfile()));

			Builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

			Builder.Services.AddIdentity<ApplicationUser, IdentityRole>(Options =>
			{
				Options.Password.RequireNonAlphanumeric = true; //@#$
				Options.Password.RequireDigit = true; //1234
				Options.Password.RequireLowercase = true; //ssfs
				Options.Password.RequireUppercase = true; //FDSDS
														  //Pa$$w0rd
														  //P@ssw0rd
			})
					.AddEntityFrameworkStores<MvcAppDbContext>()
					.AddDefaultTokenProviders();
			//services.AddScoped<UserManager<ApplicationUser>>();
			//services.AddScoped<SignInManager<ApplicationUser>>();
			//services.AddScoped<RoleManager<ApplicationUser>>();
			Builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(Options =>
			{
				Options.LoginPath = "Account/Login";
				Options.AccessDeniedPath = "Home/Error";
			});


			#endregion

			var app = Builder.Build();

			#region Configure Http Request Pipelines
			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Account}/{action=Login}/{id?}");
			});
			#endregion

			app.Run();
		}

	}
}

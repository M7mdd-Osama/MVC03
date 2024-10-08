﻿using AutoMapper;
using Demo.BLL.Interfaces;
using Demo.BLL.Repositories;
using Demo.DAL.Models;
using Demo.PL.Helpers;
using Demo.PL.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;

namespace Demo.PL.Controllers
{
	public class EmployeeController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public EmployeeController(IUnitOfWork unitOfWork,
			IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}
		public IActionResult Index(string SearchValue)
		{
			IEnumerable<Employee> employees;
			if (string.IsNullOrEmpty(SearchValue))
				employees = _unitOfWork.EmployeeRepository.GetAll();
			else
				employees = _unitOfWork.EmployeeRepository.GetEmployeeByName(SearchValue);

			var MappedEmployees = _mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeViewModel>>(employees);
			return View(MappedEmployees);
		}
		public IActionResult Create()
		{
			//ViewBag.Departments = _departmentRepository.GetAll();
			return View();
		}
		[HttpPost]
		public IActionResult Create(EmployeeViewModel employeeVM)
		{
			if (ModelState.IsValid)
			{
				employeeVM.ImageName = DocumentSettings.UploadFile(employeeVM.Image, "Images");

				var MappedEmployee = _mapper.Map<EmployeeViewModel, Employee>(employeeVM);
				_unitOfWork.EmployeeRepository.Add(MappedEmployee);
				var Result = _unitOfWork.Complete();
				if (Result > 0)
				{
					TempData["Message"] = "Employee Is Created";
				}
				return RedirectToAction(nameof(Index));
			}
			return View(employeeVM);
		}
		public IActionResult Details(int? id, string ViewName = "Details")
		{
			if (id is null)
				return BadRequest();
			var employee = _unitOfWork.EmployeeRepository.GetById(id.Value);
			if (employee is null)
				return NotFound();
			var MappedEmployee = _mapper.Map<Employee, EmployeeViewModel>(employee);
			return View(ViewName, MappedEmployee);
		}

		[HttpGet]
		public IActionResult Edit(int? id)
		{
			return Details(id, "Edit");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Edit(EmployeeViewModel employeeVM, [FromRoute] int id)
		{
			if (id != employeeVM.Id)
				return BadRequest();
			if (ModelState.IsValid)
			{
				try
				{
					var MappedEmployee = _mapper.Map<Employee>(employeeVM);
					_unitOfWork.EmployeeRepository.Update(MappedEmployee);
					_unitOfWork.Complete();
					return RedirectToAction(nameof(Index));
				}
				catch (System.Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}
			return View(employeeVM);
		}
		public IActionResult Delete(int? id)
		{
			return Details(id, "Delete");
		}
		[HttpPost]
		public IActionResult Delete(EmployeeViewModel employeeVM, [FromRoute] int id)
		{
			if (id != employeeVM.Id)
				return BadRequest();
			try
			{
				var MappedEmployee = _mapper.Map<EmployeeViewModel, Employee>(employeeVM);
				_unitOfWork.EmployeeRepository.Delete(MappedEmployee);
				_unitOfWork.Complete();
				return RedirectToAction(nameof(Index));
			}
			catch (System.Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(employeeVM);
			}
		}

	}
}

using AssetManagementDemo.Web.Models;
using AssetManagementDemo.Web.Services;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementDemo.Web.Controllers
{
	[Authorize]
	public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAssetAssignmentService _assignmentService;

        public EmployeesController(IEmployeeService employeeService, IAssetAssignmentService assignmentService)
        {
            _employeeService = employeeService;
            _assignmentService = assignmentService;
        }

        // GET: Employees
        public async Task<IActionResult> Index(
            string? searchTerm, 
            string? department, 
            string? status, 
            string? location,
            string? sortBy,
            bool sortDescending = false,
            int pageNumber = 1, 
            int pageSize = 10)
        {
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentDepartment"] = department;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentLocation"] = location;
            ViewData["CurrentSortBy"] = sortBy;
            ViewData["CurrentSortDescending"] = sortDescending;
            ViewData["CurrentPageSize"] = pageSize;

            var pagedEmployees = await _employeeService.GetEmployeesPagedAsync(searchTerm, department, status, location, sortBy, sortDescending, pageNumber, pageSize);
            return View(pagedEmployees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();

            var assignments = await _assignmentService.GetAssignmentsByEmployeeIdAsync(id);
            ViewBag.Assignments = assignments;

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View(new EmployeeCreateViewModel());
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var employee = new Employee
                {
                    EmployeeCode = model.EmployeeCode,
                    EmployeeName = model.EmployeeName,
                    Department = model.Department,
                    Designation = model.Designation,
                    Email = model.Email,
                    Phone = model.Phone,
                    Location = model.Location,
                    JoiningDate = model.JoiningDate,
                    Status = model.Status
                };

                await _employeeService.CreateEmployeeAsync(employee);
                TempData["SuccessMessage"] = $"Employee '{employee.EmployeeName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();

            var viewModel = new EmployeeEditViewModel
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = employee.EmployeeName,
                Department = employee.Department,
                Designation = employee.Designation,
                Email = employee.Email,
                Phone = employee.Phone,
                Location = employee.Location,
                JoiningDate = employee.JoiningDate,
                Status = employee.Status
            };

            return View(viewModel);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel model)
        {
            if (id != model.EmployeeId) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            try
            {
                var employee = new Employee
                {
                    EmployeeId = model.EmployeeId,
                    EmployeeCode = model.EmployeeCode,
                    EmployeeName = model.EmployeeName,
                    Department = model.Department,
                    Designation = model.Designation,
                    Email = model.Email,
                    Phone = model.Phone,
                    Location = model.Location,
                    JoiningDate = model.JoiningDate,
                    Status = model.Status
                };

                var success = await _employeeService.UpdateEmployeeAsync(employee);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = $"Employee '{employee.EmployeeName}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            var success = await _employeeService.DeleteEmployeeAsync(id);

            if (success && employee != null)
            {
                TempData["SuccessMessage"] = $"Employee '{employee.EmployeeName}' deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

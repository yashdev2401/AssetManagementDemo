using System;
using System.Threading.Tasks;
using AssetManagementDemo.Web.Services;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementDemo.Web.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly IAssetAssignmentService _assignmentService;
        private readonly IEmployeeService _employeeService;
        private readonly IAssetService _assetService;

        public AssignmentsController(
            IAssetAssignmentService assignmentService,
            IEmployeeService employeeService,
            IAssetService assetService)
        {
            _assignmentService = assignmentService;
            _employeeService = employeeService;
            _assetService = assetService;
        }

        // GET: Assignments
        public async Task<IActionResult> Index(
            string? searchTerm, 
            bool? isActive, 
            DateTime? assignedDate,
            string? sortBy,
            bool sortDescending = false,
            int pageNumber = 1, 
            int pageSize = 10)
        {
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentIsActive"] = isActive;
            ViewData["CurrentAssignedDate"] = assignedDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentSortBy"] = sortBy;
            ViewData["CurrentSortDescending"] = sortDescending;
            ViewData["CurrentPageSize"] = pageSize;

            var pagedAssignments = await _assignmentService.GetAssignmentsPagedAsync(searchTerm, isActive, assignedDate, sortBy, sortDescending, pageNumber, pageSize);
            return View(pagedAssignments);
        }

        // GET: Assignments/Assign
        public async Task<IActionResult> Assign(int? employeeId, int? assetId)
        {
            ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.AvailableAssets = await _assetService.GetAvailableAssetsAsync();

            var model = new AssignAssetViewModel();
            if (employeeId.HasValue) model.EmployeeId = employeeId.Value;
            if (assetId.HasValue) model.AssetId = assetId.Value;

            return View(model);
        }

        // POST: Assignments/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignAssetViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                ViewBag.AvailableAssets = await _assetService.GetAvailableAssetsAsync();
                return View(model);
            }

            try
            {
                var result = await _assignmentService.AssignAssetAsync(model.EmployeeId, model.AssetId, model.AssignedDate, model.Remarks);
                TempData["SuccessMessage"] = "Asset assigned successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Employees = await _employeeService.GetAllEmployeesAsync();
                ViewBag.AvailableAssets = await _assetService.GetAvailableAssetsAsync();
                return View(model);
            }
        }

        // GET: Assignments/Return/5
        public async Task<IActionResult> Return(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null) return NotFound();

            if (assignment.IsActive == false)
            {
                TempData["ErrorMessage"] = "This asset has already been returned.";
                return RedirectToAction(nameof(Index));
            }

            var model = new ReturnAssetViewModel
            {
                AssignmentId = assignment.AssignmentId,
                EmployeeName = assignment.Employee?.EmployeeName,
                AssetName = assignment.Asset?.AssetName,
                AssetCode = assignment.Asset?.AssetCode,
                AssignedDate = assignment.AssignedDate,
                ReturnDate = DateTime.Today
            };

            return View(model);
        }

        // POST: Assignments/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id, ReturnAssetViewModel model)
        {
            if (id != model.AssignmentId) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            try
            {
                var success = await _assignmentService.ReturnAssetAsync(model.AssignmentId, model.ReturnDate, model.Remarks);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Asset returned successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }
}

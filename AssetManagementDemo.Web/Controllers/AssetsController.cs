using System;
using System.Threading.Tasks;
using AssetManagementDemo.Web.Models;
using AssetManagementDemo.Web.Services;
using AssetManagementDemo.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementDemo.Web.Controllers
{
    public class AssetsController : Controller
    {
        private readonly IAssetService _assetService;
        private readonly IAssetAssignmentService _assignmentService;

        public AssetsController(IAssetService assetService, IAssetAssignmentService assignmentService)
        {
            _assetService = assetService;
            _assignmentService = assignmentService;
        }

        // GET: Assets
        public async Task<IActionResult> Index(
            string? searchTerm, 
            string? category, 
            string? brand,
            string? status, 
            string? sortBy,
            bool sortDescending = false,
            int pageNumber = 1, 
            int pageSize = 10)
        {
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentCategory"] = category;
            ViewData["CurrentBrand"] = brand;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentSortBy"] = sortBy;
            ViewData["CurrentSortDescending"] = sortDescending;
            ViewData["CurrentPageSize"] = pageSize;

            var pagedAssets = await _assetService.GetAssetsPagedAsync(searchTerm, category, brand, status, sortBy, sortDescending, pageNumber, pageSize);
            return View(pagedAssets);
        }

        // GET: Assets/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();

            var assignments = await _assignmentService.GetAssignmentsByAssetIdAsync(id);
            ViewBag.Assignments = assignments;

            return View(asset);
        }

        // GET: Assets/Create
        public IActionResult Create()
        {
            return View(new AssetCreateViewModel());
        }

        // POST: Assets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetCreateViewModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);

            try
            {
                var asset = new Asset
                {
                    AssetCode = viewModel.AssetCode,
                    AssetName = viewModel.AssetName,
                    Category = viewModel.Category,
                    Brand = viewModel.Brand,
                    Model = viewModel.Model,
                    SerialNumber = viewModel.SerialNumber,
                    PurchaseDate = viewModel.PurchaseDate,
                    WarrantyExpiry = viewModel.WarrantyExpiry,
                    PurchasePrice = viewModel.PurchasePrice,
                    Status = viewModel.Status
                };

                await _assetService.CreateAssetAsync(asset);
                TempData["SuccessMessage"] = $"Asset '{asset.AssetCode} - {asset.AssetName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(viewModel);
            }
        }

        // GET: Assets/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();

            var viewModel = new AssetEditViewModel
            {
                AssetId = asset.AssetId,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                Category = asset.Category,
                Brand = asset.Brand,
                Model = asset.Model,
                SerialNumber = asset.SerialNumber,
                PurchaseDate = asset.PurchaseDate,
                WarrantyExpiry = asset.WarrantyExpiry,
                PurchasePrice = asset.PurchasePrice,
                Status = asset.Status
            };

            return View(viewModel);
        }

        // POST: Assets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssetEditViewModel viewModel)
        {
            if (id != viewModel.AssetId) return BadRequest();
            if (!ModelState.IsValid) return View(viewModel);

            try
            {
                var asset = new Asset
                {
                    AssetId = viewModel.AssetId,
                    AssetCode = viewModel.AssetCode,
                    AssetName = viewModel.AssetName,
                    Category = viewModel.Category,
                    Brand = viewModel.Brand,
                    Model = viewModel.Model,
                    SerialNumber = viewModel.SerialNumber,
                    PurchaseDate = viewModel.PurchaseDate,
                    WarrantyExpiry = viewModel.WarrantyExpiry,
                    PurchasePrice = viewModel.PurchasePrice,
                    Status = viewModel.Status
                };

                var success = await _assetService.UpdateAssetAsync(asset);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = $"Asset '{asset.AssetCode}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(viewModel);
            }
        }

        // GET: Assets/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null) return NotFound();

            return View(asset);
        }

        // POST: Assets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            var success = await _assetService.DeleteAssetAsync(id);

            if (success && asset != null)
            {
                TempData["SuccessMessage"] = $"Asset '{asset.AssetCode}' deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

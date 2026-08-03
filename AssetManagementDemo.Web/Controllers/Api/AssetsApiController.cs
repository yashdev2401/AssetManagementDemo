using AssetManagementDemo.Web.DTOs;
using AssetManagementDemo.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AssetManagementDemo.Web.Controllers.Api
{
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AssetsApiController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsApiController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        /// <summary>
        /// Retrieves a paged list of assets with search, category, brand, status, sorting, and pagination options.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<AssetDto>>>> GetAssets(
            [FromQuery] string? searchTerm,
            [FromQuery] string? category,
            [FromQuery] string? brand,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _assetService.GetAssetsPagedAsync(searchTerm, category, brand, status, sortBy, sortDescending, pageNumber, pageSize);
            
            var pagedDto = new PagedResultDto<AssetDto>
            {
                Items = result.Items.Select(a => _assetService.MapToDto(a)).ToList(),
                TotalItems = result.TotalItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                HasPreviousPage = result.HasPreviousPage,
                HasNextPage = result.HasNextPage,
                SortBy = result.SortBy,
                SortDescending = result.SortDescending
            };

            return Ok(ApiResponse<PagedResultDto<AssetDto>>.SuccessResponse(pagedDto));
        }

        /// <summary>
        /// Retrieves an asset by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AssetDto>>> GetAssetById(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                return NotFound(ApiResponse<AssetDto>.ErrorResponse($"Asset with ID {id} not found."));
            }

            return Ok(ApiResponse<AssetDto>.SuccessResponse(_assetService.MapToDto(asset)));
        }

        /// <summary>
        /// Creates a new asset.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<AssetDto>>> CreateAsset([FromBody] CreateAssetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<AssetDto>.ErrorResponse("Validation failed."));
            }

            var entity = _assetService.MapToEntity(dto);
            var created = await _assetService.CreateAssetAsync(entity);
            var resultDto = _assetService.MapToDto(created);

            return CreatedAtAction(nameof(GetAssetById), new { id = created.AssetId }, 
                ApiResponse<AssetDto>.SuccessResponse(resultDto, "Asset created successfully."));
        }

        /// <summary>
        /// Updates an existing asset.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<AssetDto>>> UpdateAsset(int id, [FromBody] UpdateAssetDto dto)
        {
            var existing = await _assetService.GetAssetByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<AssetDto>.ErrorResponse($"Asset with ID {id} not found."));
            }
            existing.AssetCode = !string.IsNullOrWhiteSpace(dto.AssetCode)? dto.AssetCode : existing.AssetCode;
            existing.AssetName = !string.IsNullOrWhiteSpace(dto.AssetName)
                ? dto.AssetName
                : existing.AssetName;
            existing.Category = !string.IsNullOrWhiteSpace(dto.Category)
                ? dto.Category
                : existing.Category;
            existing.Brand = !string.IsNullOrWhiteSpace(dto.Brand)
                ? dto.Brand
                : existing.Brand;
            existing.Model = !string.IsNullOrWhiteSpace(dto.Model)
                ? dto.Model
                : existing.Model;
            existing.SerialNumber = !string.IsNullOrWhiteSpace(dto.SerialNumber)
                ? dto.SerialNumber
                : existing.SerialNumber;
            existing.PurchaseDate = dto.PurchaseDate ?? existing.PurchaseDate;
            existing.WarrantyExpiry = dto.WarrantyExpiry ?? existing.WarrantyExpiry;
            existing.PurchasePrice = dto.PurchasePrice ?? existing.PurchasePrice;
            existing.Status = !string.IsNullOrWhiteSpace(dto.Status)
                ? dto.Status
                : existing.Status;
            await _assetService.UpdateAssetAsync(existing);
            return Ok(ApiResponse<AssetDto>.SuccessResponse(_assetService.MapToDto(existing), "Asset updated successfully."));
        }

        /// <summary>
        /// Deletes an asset by ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAsset(int id)
        {
            var success = await _assetService.DeleteAssetAsync(id);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse($"Asset with ID {id} not found."));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Asset deleted successfully."));
        }
    }
}

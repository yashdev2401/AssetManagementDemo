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
    public class AssignmentsApiController : ControllerBase
    {
        private readonly IAssetAssignmentService _assignmentService;

        public AssignmentsApiController(IAssetAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        /// <summary>
        /// Retrieves a paged list of asset assignments with search, active/returned status, date filtering, sorting, and pagination.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<AssignmentDto>>>> GetAssignments(
            [FromQuery] string? searchTerm,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? assignedDate,
            [FromQuery] string? sortBy,
            [FromQuery] bool sortDescending = false,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _assignmentService.GetAssignmentsPagedAsync(searchTerm, isActive, assignedDate, sortBy, sortDescending, pageNumber, pageSize);
            
            var pagedDto = new PagedResultDto<AssignmentDto>
            {
                Items = result.Items.Select(a => _assignmentService.MapToDto(a)).ToList(),
                TotalItems = result.TotalItems,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages,
                HasPreviousPage = result.HasPreviousPage,
                HasNextPage = result.HasNextPage,
                SortBy = result.SortBy,
                SortDescending = result.SortDescending
            };

            return Ok(ApiResponse<PagedResultDto<AssignmentDto>>.SuccessResponse(pagedDto));
        }

        /// <summary>
        /// Retrieves an assignment record by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AssignmentDto>>> GetAssignmentById(int id)
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id);
            if (assignment == null)
            {
                return NotFound(ApiResponse<AssignmentDto>.ErrorResponse($"Assignment with ID {id} not found."));
            }

            return Ok(ApiResponse<AssignmentDto>.SuccessResponse(_assignmentService.MapToDto(assignment)));
        }

        /// <summary>
        /// Assigns an asset to an employee.
        /// </summary>
        [HttpPost("assign")]
        public async Task<ActionResult<ApiResponse<AssignmentDto>>> AssignAsset([FromBody] AssignAssetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<AssignmentDto>.ErrorResponse("Validation failed."));
            }

            var created = await _assignmentService.AssignAssetAsync(dto.EmployeeId, dto.AssetId, dto.AssignedDate, dto.Remarks);
            var resultDto = _assignmentService.MapToDto(created);

            return CreatedAtAction(nameof(GetAssignmentById), new { id = created.AssignmentId }, 
                ApiResponse<AssignmentDto>.SuccessResponse(resultDto, "Asset assigned successfully."));
        }

        /// <summary>
        /// Returns an assigned asset.
        /// </summary>
        [HttpPost("return")]
        public async Task<ActionResult<ApiResponse<bool>>> ReturnAsset([FromBody] ReturnAssetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Validation failed."));
            }

            var success = await _assignmentService.ReturnAssetAsync(dto.AssignmentId, dto.ReturnDate, dto.Remarks);
            if (!success)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse($"Assignment with ID {dto.AssignmentId} not found."));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Asset returned successfully."));
        }
    }
}

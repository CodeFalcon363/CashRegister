using CashRegister.API.DTOs;
using CashRegister.Application.Interfaces;
using CashRegister.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CashRegister.API.Controllers;

// Admin-only controller for user management, branch management, and entry oversight.
// Provides full CRUD operations with no branch restrictions.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IBranchService _branchService;
    private readonly ICashEntryService _cashEntryService;

    public AdminController(
        IUserService userService,
        IBranchService branchService,
        ICashEntryService cashEntryService)
    {
        _userService = userService;
        _branchService = branchService;
        _cashEntryService = cashEntryService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // User Management Endpoints

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            var userDtos = users.Select(u => new UserDto(
                u.Id,
                u.Username,
                u.Email,
                u.Role.ToString(),
                u.BranchId,
                u.Branch?.BranchName,
                u.IsActive,
                u.CreatedAt
            )).ToList();

            return Ok(userDtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userDto = new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.Role.ToString(),
                user.BranchId,
                user.Branch?.BranchName,
                user.IsActive,
                user.CreatedAt
            );

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (!Enum.TryParse<UserRole>(request.Role, out var role))
            {
                return BadRequest(new { message = "Invalid role" });
            }

            var user = await _userService.CreateUserAsync(
                request.Username,
                request.Email,
                request.Password,
                role,
                request.BranchId
            );

            var userDto = new UserDto(
                user.Id,
                user.Username,
                user.Email,
                user.Role.ToString(),
                user.BranchId,
                user.Branch?.BranchName,
                user.IsActive,
                user.CreatedAt
            );

            return CreatedAtAction(nameof(GetUser), new { id = userDto.Id }, userDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("users/{id}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            if (!Enum.TryParse<UserRole>(request.Role, out var role))
            {
                return BadRequest(new { message = "Invalid role" });
            }

            await _userService.UpdateUserAsync(
                id,
                request.Username,
                request.Email,
                role,
                request.BranchId,
                request.IsActive
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("users/{id}/change-password")]
    public async Task<ActionResult> ChangeUserPassword(int id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            var success = await _userService.ChangePasswordAsync(id, request.NewPassword);

            if (!success)
            {
                return NotFound(new { message = "User not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Branch Management Endpoints

    [HttpGet("branches")]
    public async Task<ActionResult<List<BranchDto>>> GetAllBranches()
    {
        try
        {
            var branches = await _branchService.GetAllBranchesAsync();
            var branchDtos = branches.Select(b => new BranchDto(
                b.Id,
                b.BranchCode,
                b.BranchName,
                b.CreatedAt
            )).ToList();

            return Ok(branchDtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("branches/{id}")]
    public async Task<ActionResult<BranchDto>> GetBranch(int id)
    {
        try
        {
            var branch = await _branchService.GetBranchByIdAsync(id);

            if (branch == null)
            {
                return NotFound(new { message = "Branch not found" });
            }

            var branchDto = new BranchDto(
                branch.Id,
                branch.BranchCode,
                branch.BranchName,
                branch.CreatedAt
            );

            return Ok(branchDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("branches")]
    public async Task<ActionResult<BranchDto>> CreateBranch([FromBody] CreateBranchRequest request)
    {
        try
        {
            var branch = await _branchService.CreateBranchAsync(request.BranchCode, request.BranchName);

            var branchDto = new BranchDto(
                branch.Id,
                branch.BranchCode,
                branch.BranchName,
                branch.CreatedAt
            );

            return CreatedAtAction(nameof(GetBranch), new { id = branchDto.Id }, branchDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("branches/{id}")]
    public async Task<ActionResult> UpdateBranch(int id, [FromBody] UpdateBranchRequest request)
    {
        try
        {
            await _branchService.UpdateBranchAsync(id, request.BranchCode, request.BranchName);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("branches/{id}")]
    public async Task<ActionResult> DeleteBranch(int id)
    {
        try
        {
            await _branchService.DeleteBranchAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Entry Management Endpoints

    [HttpGet("entries")]
    public async Task<ActionResult<List<CashEntryDto>>> GetAllEntries([FromQuery] int? branchId = null, [FromQuery] string? status = null)
    {
        try
        {
            var entries = await _cashEntryService.GetAllEntriesAsync();

            if (branchId.HasValue)
            {
                entries = entries.Where(e => e.BranchId == branchId.Value).ToList();
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<EntryStatus>(status, out var entryStatus))
            {
                entries = entries.Where(e => e.Status == entryStatus).ToList();
            }

            var entryDtos = entries.Select(e => new CashEntryDto(
                e.Id,
                e.BranchId,
                e.Branch.BranchName,
                e.EntryDate,
                e.Status.ToString(),
                e.CreatedByUserId,
                e.CreatedBy.Username,
                e.CreatedAt,
                e.AuthorizedByUserId,
                e.AuthorizedBy?.Username,
                e.AuthorizedAt,
                e.RejectionReason,
                e.Rows.Select(r => new CashEntryRowDto(
                    r.Id,
                    r.SequenceOrder,
                    r.RowType,
                    r.IsOutflow,
                    r.Amount1000,
                    r.Amount500,
                    r.Amount200,
                    r.Amount100,
                    r.Amount50,
                    r.Amount20,
                    r.Amount10,
                    r.Amount5,
                    r.Amount2,
                    r.Amount1,
                    r.CoinAmount,
                    r.GetTotal()
                )).ToList()
            )).ToList();

            return Ok(entryDtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("entries/{id}")]
    public async Task<ActionResult<CashEntryDto>> GetEntry(int id)
    {
        try
        {
            var entry = await _cashEntryService.GetEntryByIdAsync(id);

            if (entry == null)
            {
                return NotFound(new { message = "Entry not found" });
            }

            var entryDto = new CashEntryDto(
                entry.Id,
                entry.BranchId,
                entry.Branch.BranchName,
                entry.EntryDate,
                entry.Status.ToString(),
                entry.CreatedByUserId,
                entry.CreatedBy.Username,
                entry.CreatedAt,
                entry.AuthorizedByUserId,
                entry.AuthorizedBy?.Username,
                entry.AuthorizedAt,
                entry.RejectionReason,
                entry.Rows.Select(r => new CashEntryRowDto(
                    r.Id,
                    r.SequenceOrder,
                    r.RowType,
                    r.IsOutflow,
                    r.Amount1000,
                    r.Amount500,
                    r.Amount200,
                    r.Amount100,
                    r.Amount50,
                    r.Amount20,
                    r.Amount10,
                    r.Amount5,
                    r.Amount2,
                    r.Amount1,
                    r.CoinAmount,
                    r.GetTotal()
                )).ToList()
            );

            return Ok(entryDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("entries/{id}/change-status")]
    public async Task<ActionResult> ChangeEntryStatus(int id, [FromBody] ChangeEntryStatusRequest request)
    {
        try
        {
            if (!Enum.TryParse<EntryStatus>(request.NewStatus, out var newStatus))
            {
                return BadRequest(new { message = "Invalid status" });
            }

            var userId = GetUserId();
            await _cashEntryService.ChangeEntryStatusAsync(id, newStatus, userId, request.RejectionReason);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("entries/{id}")]
    public async Task<ActionResult> DeleteEntry(int id)
    {
        try
        {
            await _cashEntryService.DeleteEntryAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

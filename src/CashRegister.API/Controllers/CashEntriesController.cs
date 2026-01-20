using CashRegister.API.DTOs;
using CashRegister.Application.Interfaces;
using CashRegister.Domain.Entities;
using CashRegister.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CashRegister.API.Controllers;

// Handles cash entry operations for inputers and authorizers.
// All operations are branch-isolated based on user's assigned branch.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CashEntriesController : ControllerBase
{
    private readonly ICashEntryService _cashEntryService;

    public CashEntriesController(ICashEntryService cashEntryService)
    {
        _cashEntryService = cashEntryService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int GetBranchId() => int.Parse(User.FindFirstValue("BranchId")!);

    [HttpGet]
    [Authorize(Roles = "Inputer,Authorizer,Viewer")]
    public async Task<ActionResult<List<CashEntryDto>>> GetEntries([FromQuery] string? status = null)
    {
        try
        {
            var branchId = GetBranchId();
            EntryStatus? entryStatus = null;

            if (status != null && Enum.TryParse<EntryStatus>(status, out var parsedStatus))
            {
                entryStatus = parsedStatus;
            }

            var entries = await _cashEntryService.GetEntriesByBranchAsync(branchId, entryStatus);

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

    [HttpGet("{id}")]
    [Authorize(Roles = "Inputer,Authorizer,Viewer")]
    public async Task<ActionResult<CashEntryDto>> GetEntry(int id)
    {
        try
        {
            var branchId = GetBranchId();
            var entry = await _cashEntryService.GetEntryByIdAsync(id);

            if (entry == null || entry.BranchId != branchId)
            {
                return NotFound(new { message = "Entry not found or access denied" });
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

    [HttpGet("by-date")]
    [Authorize(Roles = "Inputer,Authorizer,Viewer")]
    public async Task<ActionResult<CashEntryDto?>> GetEntryByDate([FromQuery] DateTime date)
    {
        try
        {
            var branchId = GetBranchId();
            var entry = await _cashEntryService.GetEntryByBranchAndDateAsync(branchId, date);

            if (entry == null)
            {
                return Ok(null);
            }

            var entryDto = new CashEntryDto(
                entry.Id,
                entry.BranchId,
                entry.Branch?.BranchName ?? "",
                entry.EntryDate,
                entry.Status.ToString(),
                entry.CreatedByUserId,
                entry.CreatedBy?.Username ?? "",
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

    [HttpGet("previous-day")]
    [Authorize(Roles = "Inputer")]
    public async Task<ActionResult<CashEntryDto?>> GetPreviousDayEntry([FromQuery] DateTime date)
    {
        try
        {
            var branchId = GetBranchId();
            var entry = await _cashEntryService.GetPreviousDayEntryAsync(branchId, date);

            if (entry == null)
            {
                return Ok(null);
            }

            var entryDto = new CashEntryDto(
                entry.Id,
                entry.BranchId,
                entry.Branch?.BranchName ?? "",
                entry.EntryDate,
                entry.Status.ToString(),
                entry.CreatedByUserId,
                entry.CreatedBy?.Username ?? "",
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

    [HttpGet("row-template")]
    [Authorize(Roles = "Inputer")]
    public ActionResult<List<RowTemplateDto>> GetRowTemplate()
    {
        var rowDefinitions = new List<RowTemplateDto>
        {
            new("Opening Balance", false, 1),
            new("Till Out", true, 2),
            new("Vault Out Bulk", true, 3),
            new("Vault Out Teller 1", true, 4),
            new("Vault Out Teller 2", true, 5),
            new("Vault Out Teller 3", true, 6),
            new("Vault Out Teller 4", true, 7),
            new("Vault Out Teller 5", true, 8),
            new("Vault Out Teller 6", true, 9),
            new("Load ATM 1", true, 10),
            new("Load ATM 2", true, 11),
            new("Load ATM 3", true, 12),
            new("Load ATM 4", true, 13),
            new("Load ATM 5", true, 14),
            new("Load ATM 6", true, 15),
            new("Load ATM 7", true, 16),
            new("Load ATM 8", true, 17),
            new("Unload ATM 1", false, 18),
            new("Unload ATM 2", false, 19),
            new("Unload ATM 3", false, 20),
            new("Unload ATM 4", false, 21),
            new("Unload ATM 5", false, 22),
            new("Unload ATM 6", false, 23),
            new("Unload ATM 7", false, 24),
            new("Unload ATM 8", false, 25),
            new("BSU SUPPLY", false, 26),
            new("BSU EVACUATION", true, 27),
            new("Vault In Teller 1", false, 28),
            new("Vault In Teller 2", false, 29),
            new("Vault In Teller 3", false, 30),
            new("Vault In Teller 4", false, 31),
            new("Vault In Teller 5", false, 32),
            new("Vault In Teller 6", false, 33),
            new("Vault In Bulk", false, 34),
            new("Vault Figure", false, 35),
            new("Till Total", false, 36),
            new("Vault Closing Balance", false, 37)
        };

        return Ok(rowDefinitions);
    }

    [HttpPost]
    [Authorize(Roles = "Inputer")]
    public async Task<ActionResult<CashEntryDto>> CreateEntry([FromBody] CreateCashEntryRequest request)
    {
        try
        {
            var userId = GetUserId();
            var branchId = GetBranchId();

            var entry = await _cashEntryService.CreateDraftAsync(branchId, request.EntryDate, userId);

            // Create row entities
            var rows = new List<CashEntryRow>();
            foreach (var rowRequest in request.Rows)
            {
                var row = new CashEntryRow(entry.Id, rowRequest.RowType, rowRequest.IsOutflow, rowRequest.SequenceOrder);
                row.SetAmounts(
                    rowRequest.Amount1000,
                    rowRequest.Amount500,
                    rowRequest.Amount200,
                    rowRequest.Amount100,
                    rowRequest.Amount50,
                    rowRequest.Amount20,
                    rowRequest.Amount10,
                    rowRequest.Amount5,
                    rowRequest.Amount2,
                    rowRequest.Amount1,
                    rowRequest.AmountCoin
                );
                rows.Add(row);
            }

            await _cashEntryService.SaveRowsAsync(entry.Id, rows);

            var createdEntry = await _cashEntryService.GetEntryByIdAsync(entry.Id);
            var entryDto = new CashEntryDto(
                createdEntry!.Id,
                createdEntry.BranchId,
                createdEntry.Branch.BranchName,
                createdEntry.EntryDate,
                createdEntry.Status.ToString(),
                createdEntry.CreatedByUserId,
                createdEntry.CreatedBy.Username,
                createdEntry.CreatedAt,
                createdEntry.AuthorizedByUserId,
                createdEntry.AuthorizedBy?.Username,
                createdEntry.AuthorizedAt,
                createdEntry.RejectionReason,
                createdEntry.Rows.Select(r => new CashEntryRowDto(
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

            return CreatedAtAction(nameof(GetEntry), new { id = entryDto.Id }, entryDto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/rows")]
    [Authorize(Roles = "Inputer")]
    public async Task<ActionResult> UpdateEntryRows(int id, [FromBody] List<UpdateCashEntryRowRequest> rowsRequest)
    {
        try
        {
            var branchId = GetBranchId();
            var entry = await _cashEntryService.GetEntryByIdAsync(id);

            if (entry == null || entry.BranchId != branchId)
            {
                return NotFound(new { message = "Entry not found or access denied" });
            }

            if (entry.Status != EntryStatus.Draft)
            {
                return BadRequest(new { message = "Only draft entries can be modified" });
            }

            // Create row entities from requests
            var rows = new List<CashEntryRow>();
            foreach (var rowRequest in rowsRequest)
            {
                var row = new CashEntryRow(id, rowRequest.RowType, rowRequest.IsOutflow, rowRequest.SequenceOrder);
                row.SetAmounts(
                    rowRequest.Amount1000,
                    rowRequest.Amount500,
                    rowRequest.Amount200,
                    rowRequest.Amount100,
                    rowRequest.Amount50,
                    rowRequest.Amount20,
                    rowRequest.Amount10,
                    rowRequest.Amount5,
                    rowRequest.Amount2,
                    rowRequest.Amount1,
                    rowRequest.AmountCoin
                );
                rows.Add(row);
            }

            await _cashEntryService.SaveRowsAsync(id, rows);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/submit")]
    [Authorize(Roles = "Inputer")]
    public async Task<ActionResult> SubmitEntry(int id)
    {
        try
        {
            var branchId = GetBranchId();
            var entry = await _cashEntryService.GetEntryByIdAsync(id);

            if (entry == null || entry.BranchId != branchId)
            {
                return NotFound(new { message = "Entry not found or access denied" });
            }

            await _cashEntryService.SubmitEntryAsync(id);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Authorizer")]
    public async Task<ActionResult> ApproveEntry(int id)
    {
        try
        {
            var userId = GetUserId();
            var branchId = GetBranchId();
            var entry = await _cashEntryService.GetEntryByIdAsync(id);

            if (entry == null || entry.BranchId != branchId)
            {
                return NotFound(new { message = "Entry not found or access denied" });
            }

            await _cashEntryService.ApproveEntryAsync(id, userId);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Authorizer")]
    public async Task<ActionResult> RejectEntry(int id, [FromBody] RejectCashEntryRequest request)
    {
        try
        {
            var userId = GetUserId();
            var branchId = GetBranchId();
            var entry = await _cashEntryService.GetEntryByIdAsync(id);

            if (entry == null || entry.BranchId != branchId)
            {
                return NotFound(new { message = "Entry not found or access denied" });
            }

            await _cashEntryService.RejectEntryAsync(id, userId, request.RejectionReason);

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

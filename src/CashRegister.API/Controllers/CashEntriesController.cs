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

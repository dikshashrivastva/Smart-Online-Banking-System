using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.Services.Interfaces;
using SmartBank.Models.DTOs.Transactions;

namespace SmartBank.API.Controllers;

[ApiController]
[Authorize]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IValidator<DepositRequestDto> _depositValidator;
    private readonly IValidator<WithdrawRequestDto> _withdrawValidator;
    private readonly IValidator<TransferRequestDto> _transferValidator;

    public TransactionsController(
        ITransactionService transactionService,
        IValidator<DepositRequestDto> depositValidator,
        IValidator<WithdrawRequestDto> withdrawValidator,
        IValidator<TransferRequestDto> transferValidator)
    {
        _transactionService = transactionService;
        _depositValidator = depositValidator;
        _withdrawValidator = withdrawValidator;
        _transferValidator = transferValidator;
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(DepositRequestDto request)
    {
        var validation = await _depositValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ApiResponse(false, validation.Errors.First().ErrorMessage, validation.Errors));

        var result = await _transactionService.DepositAsync(GetUserId(), request, GetIpAddress(), GetDeviceInfo());
        return Ok(ApiResponse(true, "Deposit completed.", result));
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw(WithdrawRequestDto request)
    {
        var validation = await _withdrawValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ApiResponse(false, validation.Errors.First().ErrorMessage, validation.Errors));

        var result = await _transactionService.WithdrawAsync(GetUserId(), request, GetIpAddress(), GetDeviceInfo());
        return Ok(ApiResponse(true, "Withdrawal completed.", result));
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(TransferRequestDto request, [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey)
    {
        var validation = await _transferValidator.ValidateAsync(request);
        if (!validation.IsValid)
            return BadRequest(ApiResponse(false, validation.Errors.First().ErrorMessage, validation.Errors));

        var result = await _transactionService.TransferAsync(GetUserId(), request, GetIpAddress(), GetDeviceInfo(), idempotencyKey);
        return Ok(ApiResponse(true, "Transfer completed.", result));
    }

    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] TransactionHistoryQueryDto query)
    {
        var result = await _transactionService.GetHistoryAsync(GetUserId(), query);
        return Ok(ApiResponse(true, "Transaction history loaded.", result));
    }

    [HttpGet("{id:int}/receipt")]
    public async Task<IActionResult> Receipt(int id)
    {
        var result = await _transactionService.GetReceiptAsync(GetUserId(), id);
        return Ok(ApiResponse(true, "Receipt loaded.", result));
    }

    [HttpGet("statement")]
    public async Task<IActionResult> Statement([FromQuery] int accountId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var csv = await _transactionService.ExportStatementCsvAsync(GetUserId(), accountId, from, to);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"statement-{accountId}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    private string? GetIpAddress()
        => HttpContext.Connection.RemoteIpAddress?.ToString();

    private string? GetDeviceInfo()
        => Request.Headers.UserAgent.ToString();

    private static object ApiResponse(bool success, string message, object? data)
        => new { success, message, data };
}

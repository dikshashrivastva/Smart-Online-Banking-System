using SmartBank.Models.DTOs.Transactions;

namespace SmartBank.API.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponseDto> DepositAsync(int userId, DepositRequestDto request, string? ipAddress, string? deviceInfo);
    Task<TransactionResponseDto> WithdrawAsync(int userId, WithdrawRequestDto request, string? ipAddress, string? deviceInfo);
    Task<TransactionResponseDto> TransferAsync(int userId, TransferRequestDto request, string? ipAddress, string? deviceInfo, string? idempotencyKey);
    Task<PagedResultDto<TransactionResponseDto>> GetHistoryAsync(int userId, TransactionHistoryQueryDto query);
    Task<TransactionReceiptDto> GetReceiptAsync(int userId, int transactionId);
    Task<string> ExportStatementCsvAsync(int userId, int accountId, DateTime? from, DateTime? to);
}

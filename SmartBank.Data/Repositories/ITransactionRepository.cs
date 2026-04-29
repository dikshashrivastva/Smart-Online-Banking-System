using SmartBank.Models.DTOs.Transactions;
using SmartBank.Models.Entities;

namespace SmartBank.Data.Repositories;

public interface ITransactionRepository
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    Task<Account?> GetAccountForUpdateAsync(int accountId);
    Task<Account?> GetAccountByNumberAsync(string accountNumber);
    Task<Transfer?> GetTransferByIdempotencyKeyAsync(string idempotencyKey);
    Task<Transaction?> GetDebitTransactionForTransferAsync(int transferId);
    Task<decimal> GetTodaysCompletedWithdrawalTotalAsync(int accountId, DateTime businessDate);
    Task AddTransactionAsync(Transaction transaction);
    Task AddTransferAsync(Transfer transfer);
    Task SaveChangesAsync();
    Task<PagedResultDto<Transaction>> GetHistoryAsync(int accountId, int page, int size, DateTime? from, DateTime? to, string? type);
    Task<Transaction?> GetReceiptTransactionAsync(int transactionId);
    Task<List<Transaction>> GetStatementAsync(int accountId, DateTime? from, DateTime? to);
}

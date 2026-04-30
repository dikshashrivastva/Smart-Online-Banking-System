using Microsoft.EntityFrameworkCore;
using SmartBank.Data.Context;
using SmartBank.Models.DTOs.Transactions;
using SmartBank.Models.Entities;

namespace SmartBank.Data.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly SmartOnlineBankingDbContext _context;

    public TransactionRepository(SmartOnlineBankingDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        if (!_context.Database.IsRelational())
            return await operation();

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Account?> GetAccountForUpdateAsync(int accountId)
    {
        if (_context.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
        {
            return await _context.Accounts
                .FromSqlInterpolated($"SELECT * FROM Accounts WITH (UPDLOCK, ROWLOCK) WHERE AccountId = {accountId}")
                .Include(a => a.User)
                .FirstOrDefaultAsync();
        }

        return await _context.Accounts
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AccountId == accountId);
    }

    public async Task<Account?> GetAccountByNumberAsync(string accountNumber)
        => await _context.Accounts
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

    public async Task<Transfer?> GetTransferByIdempotencyKeyAsync(string idempotencyKey)
        => await _context.Transfers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey);

    public async Task<Transaction?> GetDebitTransactionForTransferAsync(int transferId)
        => await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TransferId == transferId && t.TransactionType == "Transfer");

    public async Task<decimal> GetTodaysCompletedWithdrawalTotalAsync(int accountId, DateTime businessDate)
    {
        var start = businessDate.Date;
        var end = start.AddDays(1);

        return await _context.Transactions
            .Where(t => t.AccountId == accountId
                && t.TransactionType == "Withdraw"
                && t.Status == "Completed"
                && t.CreatedAt >= start
                && t.CreatedAt < end)
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;
    }

    public Task AddTransactionAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task AddTransferAsync(Transfer transfer)
    {
        _context.Transfers.Add(transfer);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task<PagedResultDto<Transaction>> GetHistoryAsync(int accountId, int page, int size, DateTime? from, DateTime? to, string? type)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId);

        if (from.HasValue)
            query = query.Where(t => t.CreatedAt >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(t => t.CreatedAt < to.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(t => t.TransactionType == type);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResultDto<Transaction>
        {
            Items = items,
            Page = page,
            Size = size,
            TotalCount = total
        };
    }

    public async Task<Transaction?> GetReceiptTransactionAsync(int transactionId)
        => await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .ThenInclude(a => a.User)
            .Include(t => t.Transfer)
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

    public async Task<List<Transaction>> GetStatementAsync(int accountId, DateTime? from, DateTime? to)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId);

        if (from.HasValue)
            query = query.Where(t => t.CreatedAt >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(t => t.CreatedAt < to.Value.Date.AddDays(1));

        return await query.OrderBy(t => t.CreatedAt).ToListAsync();
    }
}

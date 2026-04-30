using System.Globalization;
using System.Text;
using SmartBank.API.Services.Interfaces;
using SmartBank.Data.Repositories;
using SmartBank.Models.DTOs.Transactions;
using SmartBank.Models.Entities;

namespace SmartBank.API.Services;

public class TransactionService : ITransactionService
{
    private const decimal DailyWithdrawalLimit = 100000m;
    private readonly ITransactionRepository _repo;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(ITransactionRepository repo, ILogger<TransactionService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<TransactionResponseDto> DepositAsync(int userId, DepositRequestDto request, string? ipAddress, string? deviceInfo)
    {
        try
        {
            var result = await _repo.ExecuteInTransactionAsync(async () =>
            {
                ValidateAmount(request.Amount);
                var account = await GetOwnedAccountForUpdateAsync(userId, request.AccountId);
                var before = account.Balance;
                account.Balance += request.Amount;
                account.UpdatedAt = DateTime.UtcNow;

                var transaction = CreateTransaction(account.AccountId, "Deposit", request.Amount, before, account.Balance, request.Description, ipAddress, deviceInfo);
                await _repo.AddTransactionAsync(transaction);
                await _repo.SaveChangesAsync();
                return Map(transaction);
            });

            _logger.LogInformation("Deposit completed for account {AccountId}, reference {ReferenceNumber}", request.AccountId, result.ReferenceNumber);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deposit failed for account {AccountId}", request.AccountId);
            throw;
        }
    }

    public async Task<TransactionResponseDto> WithdrawAsync(int userId, WithdrawRequestDto request, string? ipAddress, string? deviceInfo)
    {
        try
        {
            var result = await _repo.ExecuteInTransactionAsync(async () =>
            {
                ValidateAmount(request.Amount);
                var account = await GetOwnedAccountForUpdateAsync(userId, request.AccountId);
                var withdrawnToday = await _repo.GetTodaysCompletedWithdrawalTotalAsync(account.AccountId, DateTime.UtcNow);

                if (withdrawnToday + request.Amount > DailyWithdrawalLimit)
                    throw new InvalidOperationException("Daily withdrawal limit of INR 100000 exceeded.");

                if (account.Balance < request.Amount)
                    throw new InvalidOperationException("Insufficient balance.");

                var before = account.Balance;
                account.Balance -= request.Amount;
                account.UpdatedAt = DateTime.UtcNow;

                var transaction = CreateTransaction(account.AccountId, "Withdraw", request.Amount, before, account.Balance, request.Description, ipAddress, deviceInfo);
                await _repo.AddTransactionAsync(transaction);
                await _repo.SaveChangesAsync();
                return Map(transaction);
            });

            _logger.LogInformation("Withdrawal completed for account {AccountId}, reference {ReferenceNumber}", request.AccountId, result.ReferenceNumber);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Withdrawal failed for account {AccountId}", request.AccountId);
            throw;
        }
    }

    public async Task<TransactionResponseDto> TransferAsync(int userId, TransferRequestDto request, string? ipAddress, string? deviceInfo, string? idempotencyKey)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existingTransfer = await _repo.GetTransferByIdempotencyKeyAsync(idempotencyKey);
                if (existingTransfer is not null)
                {
                    var existingDebit = await _repo.GetDebitTransactionForTransferAsync(existingTransfer.TransferId);
                    if (existingDebit is not null)
                    {
                        _logger.LogInformation("Duplicate transfer idempotency key returned cached result {IdempotencyKey}", idempotencyKey);
                        return Map(existingDebit);
                    }
                }
            }

            var result = await _repo.ExecuteInTransactionAsync(async () =>
            {
                ValidateAmount(request.Amount);
                if (request.FromAccountId == request.ToAccountId)
                    throw new InvalidOperationException("Self-transfer is not allowed.");

                var lockedAccounts = await LockTransferAccountsAsync(request.FromAccountId, request.ToAccountId);
                var fromAccount = lockedAccounts.First(a => a.AccountId == request.FromAccountId);
                var toAccount = lockedAccounts.First(a => a.AccountId == request.ToAccountId);

                EnsureOwned(userId, fromAccount);

                if (fromAccount.Balance < request.Amount)
                    throw new InvalidOperationException("Insufficient balance.");

                var reference = GenerateReferenceNumber();
                var transfer = new Transfer
                {
                    FromAccountId = fromAccount.AccountId,
                    ToAccountId = toAccount.AccountId,
                    Amount = request.Amount,
                    Fee = 0m,
                    Remarks = request.Description?.Trim(),
                    ReferenceNumber = reference,
                    Status = "Completed",
                    InitiatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim()
                };

                var fromBefore = fromAccount.Balance;
                fromAccount.Balance -= request.Amount;
                fromAccount.UpdatedAt = DateTime.UtcNow;

                var toBefore = toAccount.Balance;
                toAccount.Balance += request.Amount;
                toAccount.UpdatedAt = DateTime.UtcNow;

                var debit = CreateTransaction(fromAccount.AccountId, "Transfer", request.Amount, fromBefore, fromAccount.Balance, request.Description, ipAddress, deviceInfo);
                var credit = CreateTransaction(toAccount.AccountId, "Transfer", request.Amount, toBefore, toAccount.Balance, request.Description, ipAddress, deviceInfo);

                await _repo.AddTransferAsync(transfer);
                await _repo.SaveChangesAsync();

                debit.TransferId = transfer.TransferId;
                credit.TransferId = transfer.TransferId;
                await _repo.AddTransactionAsync(debit);
                await _repo.AddTransactionAsync(credit);
                await _repo.SaveChangesAsync();

                return Map(debit);
            });

            _logger.LogInformation("Transfer completed from {FromAccountId} to {ToAccountId}, reference {ReferenceNumber}", request.FromAccountId, request.ToAccountId, result.ReferenceNumber);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transfer failed from {FromAccountId} to {ToAccountId}", request.FromAccountId, request.ToAccountId);
            throw;
        }
    }

    public async Task<PagedResultDto<TransactionResponseDto>> GetHistoryAsync(int userId, TransactionHistoryQueryDto query)
    {
        var account = await _repo.GetAccountForUpdateAsync(query.AccountId)
            ?? throw new InvalidOperationException("Account not found.");
        EnsureOwned(userId, account);

        var page = Math.Max(1, query.Page);
        var size = Math.Clamp(query.Size, 1, 100);
        var history = await _repo.GetHistoryAsync(query.AccountId, page, size, query.From, query.To, NormalizeType(query.Type));

        return new PagedResultDto<TransactionResponseDto>
        {
            Items = history.Items.Select(Map).ToList(),
            Page = history.Page,
            Size = history.Size,
            TotalCount = history.TotalCount
        };
    }

    public async Task<TransactionReceiptDto> GetReceiptAsync(int userId, int transactionId)
    {
        var transaction = await _repo.GetReceiptTransactionAsync(transactionId)
            ?? throw new InvalidOperationException("Transaction not found.");

        EnsureOwned(userId, transaction.Account);

        return new TransactionReceiptDto
        {
            ReferenceNumber = transaction.ReferenceNumber ?? string.Empty,
            AccountNumber = transaction.Account.AccountNumber,
            AccountHolderName = $"{transaction.Account.User.FirstName} {transaction.Account.User.LastName}".Trim(),
            Type = transaction.TransactionType ?? string.Empty,
            Amount = transaction.Amount,
            Fee = transaction.Transfer?.Fee ?? 0m,
            Status = transaction.Status ?? string.Empty,
            CreatedAt = transaction.CreatedAt ?? DateTime.UtcNow,
            Description = transaction.Description
        };
    }

    public async Task<string> ExportStatementCsvAsync(int userId, int accountId, DateTime? from, DateTime? to)
    {
        var account = await _repo.GetAccountForUpdateAsync(accountId)
            ?? throw new InvalidOperationException("Account not found.");
        EnsureOwned(userId, account);

        var transactions = await _repo.GetStatementAsync(accountId, from, to);
        var csv = new StringBuilder();
        csv.AppendLine("Date,Reference,Type,Description,Amount,BalanceBefore,BalanceAfter,Status");

        foreach (var transaction in transactions)
        {
            csv.AppendLine(string.Join(",",
                Escape(transaction.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty),
                Escape(transaction.ReferenceNumber ?? string.Empty),
                Escape(transaction.TransactionType ?? string.Empty),
                Escape(transaction.Description ?? string.Empty),
                transaction.Amount.ToString("0.00", CultureInfo.InvariantCulture),
                transaction.BalanceBefore.ToString("0.00", CultureInfo.InvariantCulture),
                transaction.BalanceAfter.ToString("0.00", CultureInfo.InvariantCulture),
                Escape(transaction.Status ?? string.Empty)));
        }

        return csv.ToString();
    }

    private async Task<Account> GetOwnedAccountForUpdateAsync(int userId, int accountId)
    {
        var account = await _repo.GetAccountForUpdateAsync(accountId)
            ?? throw new InvalidOperationException("Account not found.");
        EnsureOwned(userId, account);
        return account;
    }

    private async Task<List<Account>> LockTransferAccountsAsync(int fromAccountId, int toAccountId)
    {
        var accounts = new List<Account>();
        foreach (var accountId in new[] { fromAccountId, toAccountId }.OrderBy(id => id))
        {
            var account = await _repo.GetAccountForUpdateAsync(accountId)
                ?? throw new InvalidOperationException("Account not found.");
            accounts.Add(account);
        }

        return accounts;
    }

    private static void EnsureOwned(int userId, Account account)
    {
        if (account.UserId != userId)
            throw new UnauthorizedAccessException("You are not allowed to operate on this account.");
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
    }

    private static Transaction CreateTransaction(int accountId, string type, decimal amount, decimal before, decimal after, string? description, string? ipAddress, string? deviceInfo)
        => new()
        {
            AccountId = accountId,
            TransactionType = type,
            Amount = amount,
            BalanceBefore = before,
            BalanceAfter = after,
            Description = description?.Trim(),
            ReferenceNumber = GenerateReferenceNumber(),
            Channel = "API",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            Ipaddress = ipAddress,
            DeviceInfo = deviceInfo
        };

    private static TransactionResponseDto Map(Transaction transaction)
        => new()
        {
            Id = transaction.TransactionId,
            ReferenceNumber = transaction.ReferenceNumber ?? string.Empty,
            Type = transaction.TransactionType ?? string.Empty,
            Amount = transaction.Amount,
            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,
            Status = transaction.Status ?? string.Empty,
            CreatedAt = transaction.CreatedAt ?? DateTime.UtcNow
        };

    private static string GenerateReferenceNumber()
        => $"TXN{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();

    private static string? NormalizeType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return null;

        var value = type.Trim();
        return value.Equals("Deposit", StringComparison.OrdinalIgnoreCase) ? "Deposit"
            : value.Equals("Withdraw", StringComparison.OrdinalIgnoreCase) ? "Withdraw"
            : value.Equals("Transfer", StringComparison.OrdinalIgnoreCase) ? "Transfer"
            : null;
    }

    private static string Escape(string value)
        => value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}

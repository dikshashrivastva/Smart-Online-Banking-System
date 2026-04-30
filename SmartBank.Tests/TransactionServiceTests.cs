using Microsoft.Extensions.Logging.Abstractions;
using SmartBank.API.Services;
using SmartBank.Data.Repositories;
using SmartBank.Models.DTOs.Transactions;
using SmartBank.Models.Entities;
using Xunit;

namespace SmartBank.Tests;

public class TransactionServiceTests
{
    [Fact]
    public async Task TransferService_ShouldDebitAndCredit_WhenValidTransfer()
    {
        var repo = new FakeTransactionRepository();
        repo.Accounts.Add(CreateAccount(1, 7, 5000m));
        repo.Accounts.Add(CreateAccount(2, 8, 1000m));
        var service = CreateService(repo);

        var result = await service.TransferAsync(7, new TransferRequestDto
        {
            FromAccountId = 1,
            ToAccountId = 2,
            Amount = 750m,
            Description = "Rent"
        }, "127.0.0.1", "test", "idem-1");

        Assert.Equal("Completed", result.Status);
        Assert.Equal(4250m, repo.Accounts.Single(a => a.AccountId == 1).Balance);
        Assert.Equal(1750m, repo.Accounts.Single(a => a.AccountId == 2).Balance);
        Assert.Equal(2, repo.Transactions.Count);
    }

    [Fact]
    public async Task TransferService_ShouldFail_WhenInsufficientBalance()
    {
        var repo = new FakeTransactionRepository();
        repo.Accounts.Add(CreateAccount(1, 7, 100m));
        repo.Accounts.Add(CreateAccount(2, 8, 1000m));
        var service = CreateService(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransferAsync(7, new TransferRequestDto
        {
            FromAccountId = 1,
            ToAccountId = 2,
            Amount = 750m
        }, null, null, null));

        Assert.Equal(100m, repo.Accounts.Single(a => a.AccountId == 1).Balance);
        Assert.Empty(repo.Transactions);
    }

    [Fact]
    public async Task TransferService_ShouldFail_WhenSelfTransfer()
    {
        var repo = new FakeTransactionRepository();
        repo.Accounts.Add(CreateAccount(1, 7, 5000m));
        var service = CreateService(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransferAsync(7, new TransferRequestDto
        {
            FromAccountId = 1,
            ToAccountId = 1,
            Amount = 100m
        }, null, null, null));
    }

    [Fact]
    public async Task DepositService_ShouldFail_WhenNegativeAmount()
    {
        var repo = new FakeTransactionRepository();
        repo.Accounts.Add(CreateAccount(1, 7, 5000m));
        var service = CreateService(repo);

        await Assert.ThrowsAsync<ArgumentException>(() => service.DepositAsync(7, new DepositRequestDto
        {
            AccountId = 1,
            Amount = -10m
        }, null, null));
    }

    [Fact]
    public async Task WithdrawService_ShouldFail_WhenDailyLimitExceeded()
    {
        var repo = new FakeTransactionRepository { WithdrawnToday = 99500m };
        repo.Accounts.Add(CreateAccount(1, 7, 5000m));
        var service = CreateService(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.WithdrawAsync(7, new WithdrawRequestDto
        {
            AccountId = 1,
            Amount = 600m
        }, null, null));
    }

    private static TransactionService CreateService(FakeTransactionRepository repo)
        => new(repo, NullLogger<TransactionService>.Instance);

    private static Account CreateAccount(int accountId, int userId, decimal balance)
        => new()
        {
            AccountId = accountId,
            UserId = userId,
            AccountNumber = $"SB000{accountId}",
            AccountType = "Savings",
            Balance = balance,
            Status = "Active",
            User = new User
            {
                UserId = userId,
                FirstName = "Test",
                LastName = $"User{userId}",
                Email = $"user{userId}@example.com",
                PasswordHash = "hash",
                RoleId = 1
            }
        };
}

internal class FakeTransactionRepository : ITransactionRepository
{
    public List<Account> Accounts { get; } = [];
    public List<Transaction> Transactions { get; } = [];
    public List<Transfer> Transfers { get; } = [];
    public decimal WithdrawnToday { get; set; }
    private int _nextTransactionId = 1;
    private int _nextTransferId = 1;

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
        => await operation();

    public Task<Account?> GetAccountForUpdateAsync(int accountId)
        => Task.FromResult(Accounts.FirstOrDefault(a => a.AccountId == accountId));

    public Task<Account?> GetAccountByNumberAsync(string accountNumber)
        => Task.FromResult(Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber));

    public Task<Transfer?> GetTransferByIdempotencyKeyAsync(string idempotencyKey)
        => Task.FromResult(Transfers.FirstOrDefault(t => t.IdempotencyKey == idempotencyKey));

    public Task<Transaction?> GetDebitTransactionForTransferAsync(int transferId)
        => Task.FromResult(Transactions.FirstOrDefault(t => t.TransferId == transferId && t.TransactionType == "Transfer"));

    public Task<decimal> GetTodaysCompletedWithdrawalTotalAsync(int accountId, DateTime businessDate)
        => Task.FromResult(WithdrawnToday);

    public Task AddTransactionAsync(Transaction transaction)
    {
        transaction.TransactionId = _nextTransactionId++;
        Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task AddTransferAsync(Transfer transfer)
    {
        transfer.TransferId = _nextTransferId++;
        Transfers.Add(transfer);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
        => Task.CompletedTask;

    public Task<PagedResultDto<Transaction>> GetHistoryAsync(int accountId, int page, int size, DateTime? from, DateTime? to, string? type)
        => Task.FromResult(new PagedResultDto<Transaction>
        {
            Items = Transactions.Where(t => t.AccountId == accountId).ToList(),
            Page = page,
            Size = size,
            TotalCount = Transactions.Count(t => t.AccountId == accountId)
        });

    public Task<Transaction?> GetReceiptTransactionAsync(int transactionId)
        => Task.FromResult(Transactions.FirstOrDefault(t => t.TransactionId == transactionId));

    public Task<List<Transaction>> GetStatementAsync(int accountId, DateTime? from, DateTime? to)
        => Task.FromResult(Transactions.Where(t => t.AccountId == accountId).ToList());
}

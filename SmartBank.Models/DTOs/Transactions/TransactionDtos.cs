using SmartBank.Models.DTOs.Customer;

namespace SmartBank.Models.DTOs.Transactions;

public class DepositRequestDto
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class WithdrawRequestDto
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class TransferRequestDto
{
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public class TransactionResponseDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TransactionReceiptDto
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
}

public class TransactionHistoryQueryDto
{
    public int AccountId { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Type { get; set; }
}

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => Size <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)Size);
}

public class ToAccountLookupDto
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolderName { get; set; } = string.Empty;
}

public class TransactionFormViewModel
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public List<AccountDto> Accounts { get; set; } = [];
}

public class TransferFormViewModel
{
    public int FromAccountId { get; set; }
    public string ToAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public List<AccountDto> Accounts { get; set; } = [];
}

public class TransactionHistoryViewModel
{
    public int AccountId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Type { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public List<AccountDto> Accounts { get; set; } = [];
    public PagedResultDto<TransactionResponseDto> Results { get; set; } = new();
}

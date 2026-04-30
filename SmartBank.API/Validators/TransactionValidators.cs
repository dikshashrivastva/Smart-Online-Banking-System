using FluentValidation;
using SmartBank.Models.DTOs.Transactions;

namespace SmartBank.API.Validators;

public class DepositRequestDtoValidator : AbstractValidator<DepositRequestDto>
{
    public DepositRequestDtoValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class WithdrawRequestDtoValidator : AbstractValidator<WithdrawRequestDto>
{
    public WithdrawRequestDtoValidator()
    {
        RuleFor(x => x.AccountId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public class TransferRequestDtoValidator : AbstractValidator<TransferRequestDto>
{
    public TransferRequestDtoValidator()
    {
        RuleFor(x => x.FromAccountId).GreaterThan(0);
        RuleFor(x => x.ToAccountId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x).Must(x => x.FromAccountId != x.ToAccountId)
            .WithMessage("Self-transfer is not allowed.");
    }
}

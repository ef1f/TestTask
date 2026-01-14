using FluentValidation;
using TestTask.Domain.Contracts;
using TestTask.Domain.Models;

namespace TestTask.Api.Validators;

public abstract class BaseTransactionValidator<T> : AbstractValidator<T> where T : ITransaction
{
    public BaseTransactionValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Сумма должна быть положительной.");
        RuleFor(x => x.DateTime).LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Дата не может быть в будущем.");
    }
}
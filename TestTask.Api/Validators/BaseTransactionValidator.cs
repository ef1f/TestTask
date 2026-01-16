using FluentValidation;
using TestTask.Core;


namespace TestTask.Api.Validators;

public abstract class BaseTransactionValidator<T> : AbstractValidator<T> where T : ITransaction
{
    public BaseTransactionValidator()
    {
        RuleFor(x => x.Id).NotEqual(Guid.Empty).WithMessage("Id не может быть пустым.");
        RuleFor(x => x.ClientId).NotEqual(Guid.Empty).WithMessage("Id не может быть пустым.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Сумма должна быть положительной.");
        RuleFor(x => x.DateTime).LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Дата не может быть в будущем.");
    }
}
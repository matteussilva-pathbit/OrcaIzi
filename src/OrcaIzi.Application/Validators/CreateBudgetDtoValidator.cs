using FluentValidation;

namespace OrcaIzi.Application.Validators
{
    public class CreateBudgetDtoValidator : AbstractValidator<CreateBudgetDto>
    {
        public CreateBudgetDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("CustomerId is required.");

            RuleFor(x => x.ExpirationDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one item is required.");

            RuleForEach(x => x.Items).SetValidator(new CreateBudgetItemDtoValidator());
        }
    }

    public class CreateBudgetItemDtoValidator : AbstractValidator<CreateBudgetItemDto>
    {
        public CreateBudgetItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Item Name is required.")
                .MaximumLength(100).WithMessage("Item Name must not exceed 100 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("UnitPrice must be greater than 0.");
        }
    }
}

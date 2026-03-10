using FluentValidation;

namespace OrcaIzi.Application.Validators
{
    public class CreateBudgetTemplateDtoValidator : AbstractValidator<CreateBudgetTemplateDto>
    {
        public CreateBudgetTemplateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one item is required.");

            RuleForEach(x => x.Items).SetValidator(new CreateBudgetTemplateItemDtoValidator());
        }
    }

    public class CreateBudgetTemplateItemDtoValidator : AbstractValidator<CreateBudgetTemplateItemDto>
    {
        public CreateBudgetTemplateItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Item Name is required.")
                .MaximumLength(200).WithMessage("Item Name must not exceed 200 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("UnitPrice must be greater than 0.");
        }
    }
}


﻿namespace OrcaIzi.Application.Validators
{
    public class CreateCatalogItemDtoValidator : AbstractValidator<CreateCatalogItemDto>
    {
        public CreateCatalogItemDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
                .When(x => x.Description != null);

            RuleFor(x => x.Unit)
                .MaximumLength(50).WithMessage("Unit must not exceed 50 characters.")
                .When(x => x.Unit != null);

            RuleFor(x => x.Category)
                .MaximumLength(100).WithMessage("Category must not exceed 100 characters.")
                .When(x => x.Category != null);

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("UnitPrice must be greater than zero.");
        }
    }
}




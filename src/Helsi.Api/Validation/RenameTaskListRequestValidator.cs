using FluentValidation;
using Helsi.Api.Dtos;

namespace Helsi.Api.Validation;

public sealed class RenameTaskListRequestValidator : AbstractValidator<RenameTaskListRequest>
{
    public RenameTaskListRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title must be <= 255 characters");
    }
}
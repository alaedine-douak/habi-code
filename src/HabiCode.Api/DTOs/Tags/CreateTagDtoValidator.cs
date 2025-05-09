using FluentValidation;

namespace HabiCode.Api.DTOs.Tags;

public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(t => t.Name).NotEmpty().MinimumLength(3);
        RuleFor(t => t.Description).MaximumLength(50);
    }
}

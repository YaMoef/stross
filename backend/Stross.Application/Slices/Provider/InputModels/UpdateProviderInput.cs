using FluentValidation;
using Stross.Infrastructure.EntityTypeConfigurations;

namespace Stross.Application.Slices.Provider.InputModels;

public sealed record UpdateProviderInput(string Name, string Url, bool Enabled);

internal sealed class UpdateProviderInputValidator : AbstractValidator<UpdateProviderInput>
{
    public UpdateProviderInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(ProviderConfiguration.NameMaxLength);

        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Url must be a valid absolute URI")
            .MaximumLength(ProviderConfiguration.UrlMaxLength);
    }
}

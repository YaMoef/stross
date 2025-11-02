using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetMusicDirectoryInput(string Id);

internal sealed class SubsonicGetMusicDirectoryInputValidator : AbstractValidator<SubsonicGetMusicDirectoryInput>
{
    public SubsonicGetMusicDirectoryInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required for getMusicDirectory");
    }
}
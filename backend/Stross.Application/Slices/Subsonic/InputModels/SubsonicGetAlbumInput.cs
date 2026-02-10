using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetAlbumInput(string Id);

internal sealed class SubsonicGetAlbumInputValidator : AbstractValidator<SubsonicGetAlbumInput>
{
    public SubsonicGetAlbumInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Id is required for getAlbum");
    }
}

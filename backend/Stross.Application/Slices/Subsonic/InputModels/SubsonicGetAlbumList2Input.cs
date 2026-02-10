using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetAlbumList2Input(
    string Type,
    int Size = 10,
    int Offset = 0,
    int? FromYear = null,
    int? ToYear = null,
    string? Genre = null,
    string? MusicFolderId = null
);

internal sealed class SubsonicGetAlbumList2InputValidator : AbstractValidator<SubsonicGetAlbumList2Input>
{
    private static readonly string[] ValidTypes =
    {
        "random",
        "newest",
        "frequent",
        "recent",
        "starred",
        "alphabeticalByName",
        "alphabeticalByArtist",
        "byYear",
        "byGenre"
    };

    public SubsonicGetAlbumList2InputValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required for getAlbumList2")
            .Must(type => ValidTypes.Contains(type.ToLowerInvariant()))
            .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}");

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(500)
            .WithMessage("Size must be between 1 and 500");

        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Offset must be 0 or greater");

        RuleFor(x => x.FromYear)
            .GreaterThan(0)
            .When(x => x.FromYear.HasValue)
            .WithMessage("FromYear must be greater than 0 when specified");

        RuleFor(x => x.ToYear)
            .GreaterThan(0)
            .When(x => x.ToYear.HasValue)
            .WithMessage("ToYear must be greater than 0 when specified");

        RuleFor(x => x.Genre)
            .NotEmpty()
            .When(x => x.Type.ToLowerInvariant() == "bygenre")
            .WithMessage("Genre is required when type is byGenre");

        RuleFor(x => x.FromYear)
            .NotNull()
            .When(x => x.Type.ToLowerInvariant() == "byyear")
            .WithMessage("FromYear is required when type is byYear");

        RuleFor(x => x.ToYear)
            .NotNull()
            .When(x => x.Type.ToLowerInvariant() == "byyear")
            .WithMessage("ToYear is required when type is byYear");
    }
}
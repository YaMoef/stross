using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed class SubsonicSearch3Input
{
    public string Query { get; init; } = string.Empty;
    public int ArtistCount { get; init; } = 20;
    public int ArtistOffset { get; init; } = 0;
    public int AlbumCount { get; init; } = 20;
    public int AlbumOffset { get; init; } = 0;
    public int SongCount { get; init; } = 20;
    public int SongOffset { get; init; } = 0;
    public string? MusicFolderId { get; init; }
}

public sealed class SubsonicSearch3InputValidator : AbstractValidator<SubsonicSearch3Input>
{
    public SubsonicSearch3InputValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Query is required");

        RuleFor(x => x.ArtistCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(500)
            .WithMessage("Artist count must be between 1 and 500");

        RuleFor(x => x.AlbumCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(500)
            .WithMessage("Album count must be between 1 and 500");

        RuleFor(x => x.SongCount)
            .GreaterThan(0)
            .LessThanOrEqualTo(500)
            .WithMessage("Song count must be between 1 and 500");

        RuleFor(x => x.ArtistOffset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Artist offset must be 0 or greater");

        RuleFor(x => x.AlbumOffset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Album offset must be 0 or greater");

        RuleFor(x => x.SongOffset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Song offset must be 0 or greater");
    }
}
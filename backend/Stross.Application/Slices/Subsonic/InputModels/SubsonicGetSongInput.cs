using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

/// <summary>
/// Input model for the Subsonic getSong query.
/// Used to get details for a specific song.
/// </summary>
public sealed record SubsonicGetSongInput
{
    /// <summary>
    /// A string which uniquely identifies the song within the music collection.
    /// </summary>
    public string Id { get; init; } = string.Empty;
}

public sealed class SubsonicGetSongInputValidator : AbstractValidator<SubsonicGetSongInput>
{
    public SubsonicGetSongInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Song ID is required")
            .Must(BeValidLong)
            .WithMessage("Song ID must be a valid number");
    }

    private static bool BeValidLong(string id)
    {
        return long.TryParse(id, out long _);
    }
}
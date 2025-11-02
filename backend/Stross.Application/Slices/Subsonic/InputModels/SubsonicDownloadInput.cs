using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

/// <summary>
/// Input model for the Subsonic download query.
/// Used to download audio files for a specific song.
/// </summary>
public sealed record SubsonicDownloadInput
{
    /// <summary>
    /// A string which uniquely identifies the song within the music collection.
    /// </summary>
    public string Id { get; init; } = string.Empty;
}

public sealed class SubsonicDownloadInputValidator : AbstractValidator<SubsonicDownloadInput>
{
    public SubsonicDownloadInputValidator()
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
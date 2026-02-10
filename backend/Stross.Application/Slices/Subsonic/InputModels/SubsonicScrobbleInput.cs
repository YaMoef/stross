using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

/// <summary>
/// Input model for the Subsonic scrobble command.
/// Used to register the local playback of a track.
/// </summary>
public sealed record SubsonicScrobbleInput
{
    /// <summary>
    /// A string which uniquely identifies the song within the music collection.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// (Since 1.8.0) The time (in milliseconds since 1 Jan 1970) at which the song was listened to.
    /// </summary>
    public long? Time { get; init; }

    /// <summary>
    /// (Since 1.8.0) Whether the song was submitted.
    /// </summary>
    public bool? Submission { get; init; }
}

public sealed class SubsonicScrobbleInputValidator : AbstractValidator<SubsonicScrobbleInput>
{
    public SubsonicScrobbleInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Song ID is required")
            .Must(BeValidLong)
            .WithMessage("Song ID must be a valid number");

        RuleFor(x => x.Time)
            .GreaterThan(0)
            .When(x => x.Time.HasValue)
            .WithMessage("Time must be greater than 0 when provided (milliseconds since 1 Jan 1970)");
    }

    private static bool BeValidLong(string id)
    {
        return long.TryParse(id, out long _);
    }
}

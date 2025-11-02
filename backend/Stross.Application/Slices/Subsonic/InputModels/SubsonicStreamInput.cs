using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

/// <summary>
/// Input model for the Subsonic stream query.
/// Used to stream audio content for a specific song.
/// </summary>
public sealed record SubsonicStreamInput
{
    /// <summary>
    /// A string which uniquely identifies the song within the music collection.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// (Since 1.2.0) Only applicable to video streaming. If specified, start streaming at the given offset (in seconds) into the video.
    /// </summary>
    public long? TimeOffset { get; init; }

    /// <summary>
    /// (Since 1.6.0) The maximum bit rate to use, in case there are multiple applicable bit rates to choose from.
    /// </summary>
    public int? MaxBitRate { get; init; }

    /// <summary>
    /// (Since 1.14.0) The preferred target format for the music stream. Can be used to bypass transcoding.
    /// </summary>
    public string? Format { get; init; }
}

public sealed class SubsonicStreamInputValidator : AbstractValidator<SubsonicStreamInput>
{
    public SubsonicStreamInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Song ID is required")
            .Must(BeValidLong)
            .WithMessage("Song ID must be a valid number");

        RuleFor(x => x.TimeOffset)
            .GreaterThanOrEqualTo(0)
            .When(x => x.TimeOffset.HasValue)
            .WithMessage("TimeOffset must be greater than or equal to 0 when provided");

        RuleFor(x => x.MaxBitRate)
            .GreaterThan(0)
            .When(x => x.MaxBitRate.HasValue)
            .WithMessage("MaxBitRate must be greater than 0 when provided");

        RuleFor(x => x.Format)
            .Must(format => string.IsNullOrEmpty(format) || !string.IsNullOrWhiteSpace(format))
            .WithMessage("Format must not be whitespace only when provided");
    }

    private static bool BeValidLong(string id)
    {
        return long.TryParse(id, out long _);
    }
}
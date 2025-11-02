using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed class SubsonicSearchInput
{
    public string? Artist { get; init; }
    public string? Album { get; init; }
    public string? Title { get; init; }
    public string? Any { get; init; }
    public int Count { get; init; } = 20;
    public int Offset { get; init; } = 0;
    public long? NewerThan { get; init; }
}

public sealed class SubsonicSearchInputValidator : AbstractValidator<SubsonicSearchInput>
{
    public SubsonicSearchInputValidator()
    {
        RuleFor(x => x.Count)
            .GreaterThan(0)
            .LessThanOrEqualTo(500)
            .WithMessage("Count must be between 1 and 500");

        RuleFor(x => x.Offset)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Offset must be 0 or greater");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Artist) || !string.IsNullOrEmpty(x.Album) || 
                      !string.IsNullOrEmpty(x.Title) || !string.IsNullOrEmpty(x.Any))
            .WithMessage("At least one search parameter (Artist, Album, Title, or Any) must be provided");
    }
}
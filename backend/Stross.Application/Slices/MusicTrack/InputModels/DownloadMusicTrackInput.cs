using FluentValidation;
using Stross.Infrastructure.EntityTypeConfigurations;

namespace Stross.Application.Slices.MusicTrack.InputModels;

public sealed record DownloadMusicTrackInput(long ProviderId, string SourceUrl);

internal sealed class DownloadMusicTrackInputValidator : AbstractValidator<DownloadMusicTrackInput>
{
    public DownloadMusicTrackInputValidator()
    {
        RuleFor(x => x.ProviderId)
            .GreaterThan(0)
            .WithMessage("ProviderId must be a valid positive number");

        RuleFor(x => x.SourceUrl)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("SourceUrl must be a valid absolute URI")
            .MaximumLength(MusicTrackConfiguration.ExternalUrlMaxLength);
    }
}
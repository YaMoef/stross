using FluentValidation;
using Stross.Infrastructure.EntityTypeConfigurations;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicCreatePlaylistInput(string Name, string? PlaylistId, string[]? SongId = null);

public sealed class SubsonicCreatePlaylistInputValidator : AbstractValidator<SubsonicCreatePlaylistInput>
{
    public SubsonicCreatePlaylistInputValidator()
    {
        RuleFor(input => input.Name)
            .NotEmpty()
            .WithMessage("Playlist name is required")
            .MaximumLength(PlaylistConfiguration.NameMaxLength);
    }
}

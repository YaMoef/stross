using FluentValidation;
using Stross.Infrastructure.EntityTypeConfigurations;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicUpdatePlaylistInput(string PlaylistId, string? Name = null, string? Comment = null, string? Description = null, bool? Public = null, string[]? SongIdToAdd = null, string[]? SongIndexToRemove = null);

public sealed class SubsonicUpdatePlaylistInputValidator : AbstractValidator<SubsonicUpdatePlaylistInput>
{
    public SubsonicUpdatePlaylistInputValidator()
    {
        RuleFor(input => input.PlaylistId)
            .NotEmpty()
            .WithMessage("Playlist id is required");

        RuleFor(input => input.Name)
            .MaximumLength(PlaylistConfiguration.NameMaxLength)
            .When(input => !string.IsNullOrWhiteSpace(input.Name));

        RuleFor(input => input.Comment)
            .MaximumLength(PlaylistConfiguration.CommentMaxLength)
            .When(input => input.Comment is not null);

        RuleFor(input => input.Description)
            .MaximumLength(PlaylistConfiguration.CommentMaxLength)
            .When(input => input.Description is not null);
    }
}

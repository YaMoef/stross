using FluentValidation;

namespace Stross.Application.Slices.Subsonic.InputModels;

public sealed record SubsonicGetMusicFoldersInput();

internal sealed class SubsonicGetMusicFoldersInputValidator : AbstractValidator<SubsonicGetMusicFoldersInput>
{
    public SubsonicGetMusicFoldersInputValidator()
    {
        // No validation needed for getMusicFolders as it takes no parameters
    }
}

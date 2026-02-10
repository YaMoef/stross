using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Config;
using Stross.Infrastructure;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicDownloadQuery(SubsonicDownloadInput Input) : IRequest<SubsonicDownloadResponse>;

internal sealed class SubsonicDownloadQueryValidator : AbstractValidator<SubsonicDownloadQuery>
{
    public SubsonicDownloadQueryValidator(IValidator<SubsonicDownloadInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicDownloadQueryHandler : IRequestHandler<SubsonicDownloadQuery, SubsonicDownloadResponse>
{
    private readonly StrossContext _context;
    private readonly StrossStorageConfig _storageConfig;

    public SubsonicDownloadQueryHandler(StrossContext context, IOptionsSnapshot<StrossStorageConfig> storageConfigSnapshot)
    {
        _context = context;
        _storageConfig = storageConfigSnapshot.Value;
    }

    public async Task<SubsonicDownloadResponse> Handle(SubsonicDownloadQuery request, CancellationToken cancellationToken)
    {
        // Parse the song ID
        if (!long.TryParse(request.Input.Id, out long songId))
            throw new Stross.Exception.Exceptions.ValidationException("Invalid song ID format");

        // Retrieve the song
        Domain.Entities.MusicTrack? musicTrack = await _context.MusicTracks
            .FirstOrDefaultAsync(x => x.Id == songId, cancellationToken);

        if (musicTrack == null)
            throw new Exception.Exceptions.EntityNotFoundException($"Song with ID '{request.Input.Id}' not found");

        string fullAudioPath = Path.Combine(_storageConfig.BasePath, musicTrack.AudioFileLocation);

        // Check if the audio file exists
        if (!File.Exists(fullAudioPath))
            throw new Exception.Exceptions.EntityNotFoundException($"Audio file not found at '{fullAudioPath}'");

        // Get file information
        string contentType = GetContentTypeFromExtension(Path.GetExtension(fullAudioPath));

        // For downloads, use a friendly filename based on the track name
        string extension = Path.GetExtension(fullAudioPath);
        string sanitizedFileName = SanitizeFileName(musicTrack.FriendlyName) + extension;

        return new SubsonicDownloadResponse(
            fullAudioPath,
            contentType,
            musicTrack.Size,
            sanitizedFileName
        );
    }

    private static string GetContentTypeFromExtension(string extension)
    {
        string lowerExtension = extension.ToLowerInvariant();

        return lowerExtension switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            ".m4a" => "audio/mp4",
            ".aac" => "audio/aac",
            ".ogg" => "audio/ogg",
            ".wma" => "audio/x-ms-wma",
            ".opus" => "audio/opus",
            _ => "application/octet-stream"
        };
    }

    private static string SanitizeFileName(string fileName)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string sanitized = fileName;

        foreach (char invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        // Also replace common problematic characters
        sanitized = sanitized
            .Replace("?", "")
            .Replace("*", "")
            .Replace(":", "-")
            .Replace("\"", "'");

        return sanitized.Trim();
    }
}

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Config;
using Stross.Infrastructure;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicStreamQuery(SubsonicStreamInput Input) : IRequest<SubsonicStreamResponse>;

internal sealed class SubsonicStreamQueryValidator : AbstractValidator<SubsonicStreamQuery>
{
    public SubsonicStreamQueryValidator(IValidator<SubsonicStreamInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicStreamQueryHandler : IRequestHandler<SubsonicStreamQuery, SubsonicStreamResponse>
{
    private readonly StrossContext _context;
    private readonly StrossStorageConfig _storageConfig;

    public SubsonicStreamQueryHandler(StrossContext context, IOptionsSnapshot<StrossStorageConfig> storageConfigSnapshot)
    {
        _context = context;
        _storageConfig = storageConfigSnapshot.Value;
    }

    public async Task<SubsonicStreamResponse> Handle(SubsonicStreamQuery request, CancellationToken cancellationToken)
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
        string fileName = Path.GetFileName(fullAudioPath);

        return new SubsonicStreamResponse(
            fullAudioPath,
            contentType,
            musicTrack.Size,
            fileName
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
}

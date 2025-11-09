using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Shared.Helpers;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicSearch2Query(SubsonicSearch2Input Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicSearch2QueryValidator : AbstractValidator<SubsonicSearch2Query>
{
    public SubsonicSearch2QueryValidator(IValidator<SubsonicSearch2Input> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicSearch2QueryHandler : IRequestHandler<SubsonicSearch2Query, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicSearch2QueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicSearch2Query request, CancellationToken cancellationToken)
    {
        string searchQuery = request.Input.Query.SanitizeSearchString()!;

        // Search for artists (creators)
        List<Creator> artists = await _context.Creators
            .Where(x => x.Name.ToLower().Contains(searchQuery))
            .Skip(request.Input.ArtistOffset)
            .Take(request.Input.ArtistCount)
            .ToListAsync(cancellationToken);

        // Search for songs (music tracks)
        List<Domain.Entities.MusicTrack> songs = await _context.MusicTracks
            .Include(x => x.Creators)
            .Include(x => x.Provider)
            .Where(x => x.FriendlyName.ToLower().Contains(searchQuery) ||
                        x.OriginalName.ToLower().Contains(searchQuery) ||
                        x.Creators.Any(c => c.Name.ToLower().Contains(searchQuery)))
            .Skip(request.Input.SongOffset)
            .Take(request.Input.SongCount)
            .ToListAsync(cancellationToken);

        // TODO: Search for albums when SubsonicAlbum entity is created
        // List<SubsonicAlbum> albums = await _context.SubsonicAlbums
        //     .Where(x => x.Title.ToLower().Contains(searchQuery) || 
        //                x.Artist.ToLower().Contains(searchQuery))
        //     .Skip(request.Input.AlbumOffset)
        //     .Take(request.Input.AlbumCount)
        //     .ToListAsync(cancellationToken);


        Response response = new Response
        {
            SearchResult2 = new SearchResult2
            {
                Artist = artists.Select(x => x.ToSubsonicArtistResponse()).ToList(),
                Album = [], // TODO: Implement album search when SubsonicAlbum entity is available
                Song = songs.Select(x => x.ToSubsonicSongResponse()).ToList()
            }
        };

        return new SubsonicBaseResponse(response);
    }
}
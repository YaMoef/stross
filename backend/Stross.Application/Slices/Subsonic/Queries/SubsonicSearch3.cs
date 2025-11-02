using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicSearch3Query(SubsonicSearch3Input Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicSearch3QueryValidator : AbstractValidator<SubsonicSearch3Query>
{
    public SubsonicSearch3QueryValidator(IValidator<SubsonicSearch3Input> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class SubsonicSearch3QueryHandler : IRequestHandler<SubsonicSearch3Query, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicSearch3QueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicSearch3Query request, CancellationToken cancellationToken)
    {
        string searchQuery = request.Input.Query.ToLower();

        // Search for artists (creators) organized by ID3 tags
        List<Creator> artists = await _context.Creators
            .Where(x => x.Name.ToLower().Contains(searchQuery))
            .Skip(request.Input.ArtistOffset)
            .Take(request.Input.ArtistCount)
            .ToListAsync(cancellationToken);

        // Search for songs (music tracks) organized by ID3 tags
        List<Domain.Entities.MusicTrack> songs = await _context.MusicTracks
            .Include(x => x.Creators)
            .Include(x => x.Provider)
            .Where(x => x.FriendlyName.ToLower().Contains(searchQuery) ||
                        x.OriginalName.ToLower().Contains(searchQuery) ||
                        x.Creators.Any(c => c.Name.ToLower().Contains(searchQuery)))
            .Skip(request.Input.SongOffset)
            .Take(request.Input.SongCount)
            .ToListAsync(cancellationToken);

        // TODO: Search for albums organized by ID3 tags when SubsonicAlbum entity is created
        // This would be similar to search2 but organized according to ID3 tags rather than file structure
        // List<SubsonicAlbum> albums = await _context.SubsonicAlbums
        //     .Where(x => x.Title.ToLower().Contains(searchQuery) || 
        //                x.Artist.ToLower().Contains(searchQuery))
        //     .Skip(request.Input.AlbumOffset)
        //     .Take(request.Input.AlbumCount)
        //     .ToListAsync(cancellationToken);


        Response response = new Response
        {
            SearchResult3 = new SearchResult3
            {
                Artist = artists.Select(x => x.ToSubsonicArtistID3Response()).ToList(),
                Album = [], // TODO: Implement album search when SubsonicAlbum entity is available
                Song = songs.Select(x => x.ToSubsonicSongResponse()).ToList()
            }
        };

        return new SubsonicBaseResponse(response);
    }
}
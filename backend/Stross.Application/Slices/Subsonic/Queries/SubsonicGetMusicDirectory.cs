using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.Mappings;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Domain.Entities;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.SubsonicModels;
using Directory = Stross.SubsonicModels.Directory;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetMusicDirectoryQuery(SubsonicGetMusicDirectoryInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetMusicDirectoryQueryValidator : AbstractValidator<SubsonicGetMusicDirectoryQuery>
{
    public SubsonicGetMusicDirectoryQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");

        RuleFor(x => x.Input.Id)
            .NotEmpty()
            .WithMessage("Id is required for getMusicDirectory");
    }
}

internal sealed class SubsonicGetMusicDirectoryQueryHandler : IRequestHandler<SubsonicGetMusicDirectoryQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicGetMusicDirectoryQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetMusicDirectoryQuery request, CancellationToken cancellationToken)
    {
        string id = request.Input.Id;

        // Try to parse as created ID (music folder)
        if (int.TryParse(id, out int creatorId))
        {
            Creator? creator = await _context.Creators
                .FirstOrDefaultAsync(p => p.Id == creatorId, cancellationToken);

            if (creator is not null)
            {
                List<Domain.Entities.MusicTrack> allMusicTracksForProvider =
                    await _context.MusicTracks
                        .Include(m => m.Creators)
                        .ThenInclude(m => m.ExternalCreatorMusicTrack)
                        .Where(m => m.Creators.Any() && m.Creators.FirstOrDefault()!.Id == creatorId)
                        .ToListAsync(cancellationToken);

                Response response = new Response
                {
                    Directory = new Directory
                    {
                        Id = creatorId.ToString(),
                        Name = creator.Name,
                        Child = allMusicTracksForProvider.Select(c => c.ToSubsonicSongResponse()).ToList()
                    }
                };

                return new SubsonicBaseResponse(response);
            }
        }

        throw new EntityNotFoundException(nameof(Domain.Entities.Provider));
    }
}
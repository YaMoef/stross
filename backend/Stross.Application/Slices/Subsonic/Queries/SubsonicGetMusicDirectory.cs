using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Abstractions.Accessors;
using Stross.Application.Slices.Subsonic.Helpers;
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
    private readonly IUserAccessor _userAccessor;

    public SubsonicGetMusicDirectoryQueryHandler(StrossContext context, IUserAccessor userAccessor)
    {
        _context = context;
        _userAccessor = userAccessor;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetMusicDirectoryQuery request, CancellationToken cancellationToken)
    {
        // parse as a long
        if (long.TryParse(request.Input.Id, out long parsedId))
        {
            Domain.Entities.User? currentUser = await _userAccessor.GetCurrentUserAsync(cancellationToken);

            if (currentUser is null)
                throw new AuthenticationException();

            // Check if it's a provider
            Domain.Entities.Provider? provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.Id == parsedId, cancellationToken);

            if (provider is not null)
            {
                List<Creator> creatorsInProvider = await _context.Creators
                    .Include(c => c.ExternalCreators)
                    .Include(c => c.MusicTracks)
                    .Where(c => c.ExternalCreators.Any(ecmt => ecmt.ProviderId == parsedId))
                    .OrderBy(c => c.Name)
                    .ToListAsync(cancellationToken);

                Response response = new Response
                {
                    Directory = new Directory
                    {
                        Id = provider.Id.ToString(),
                        Name = provider.Name,
                        Child = creatorsInProvider.Select(c => new Child
                        {
                            Id = c.Id.ToString(),
                            Parent = provider.Id.ToString(),
                            Title = c.Name,
                            IsDir = true,
                            Artist = c.Name,
                            CoverArt = c.Id.ToString(),
                            Duration = c.MusicTracks.Sum(m => m.Duration)
                        }).ToList()
                    }
                };

                return new SubsonicBaseResponse(response);
            }

            // Check if it's a creator (show music tracks within a creator)
            Creator? creator = await _context.Creators
                .Include(c => c.ExternalCreators)
                .ThenInclude(ecmt => ecmt.Provider)
                .FirstOrDefaultAsync(c => c.Id == parsedId, cancellationToken);

            if (creator is not null)
            {
                List<Domain.Entities.MusicTrack> musicTracksForCreator = await _context.MusicTracks
                    .Include(m => m.Creators)
                    .Include(m => m.Provider)
                    .Where(m => m.Creators.Any(c => c.Id == parsedId))
                    .OrderBy(m => m.FriendlyName)
                    .ToListAsync(cancellationToken);

                // Get the parent provider ID from the creator's external relationship
                ExternalCreator? externalRelation = creator.ExternalCreators.FirstOrDefault();
                string parentId = externalRelation?.ProviderId.ToString() ?? "1";

                StarredData starredData = await StarredDataHelper.LoadStarredDataForUserAsync(_context, currentUser.Id, cancellationToken);

                Response response = new Response
                {
                    Directory = new Directory
                    {
                        Id = creator.Id.ToString(),
                        Name = creator.Name,
                        Parent = parentId,
                        Child = musicTracksForCreator.Select(t => t.ToSubsonicSongResponse(starredData.StarredTracks.GetValueOrDefault(t.Id))).ToList()
                    }
                };

                return new SubsonicBaseResponse(response);
            }
        }

        throw new EntityNotFoundException(nameof(Domain.Entities.Provider));
    }
}
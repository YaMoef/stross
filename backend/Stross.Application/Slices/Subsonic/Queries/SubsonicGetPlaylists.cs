using FluentValidation;
using MediatR;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetPlaylistsQuery(SubsonicGetPlaylistsInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetPlaylistsQueryValidator : AbstractValidator<SubsonicGetPlaylistsQuery>
{
    public SubsonicGetPlaylistsQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetPlaylistsQueryHandler : IRequestHandler<SubsonicGetPlaylistsQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicGetPlaylistsQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public Task<SubsonicBaseResponse> Handle(SubsonicGetPlaylistsQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement playlist functionality - for now return empty list
        Response response = new Response
        {
            Playlists = new List<Playlist>()
        };

        return Task.FromResult(new SubsonicBaseResponse(response));
    }
}
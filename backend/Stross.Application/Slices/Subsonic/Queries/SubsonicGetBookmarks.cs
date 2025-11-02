using FluentValidation;
using MediatR;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetBookmarksQuery(SubsonicGetBookmarksInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetBookmarksQueryValidator : AbstractValidator<SubsonicGetBookmarksQuery>
{
    public SubsonicGetBookmarksQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetBookmarksQueryHandler : IRequestHandler<SubsonicGetBookmarksQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicGetBookmarksQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public Task<SubsonicBaseResponse> Handle(SubsonicGetBookmarksQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement bookmark functionality - for now return empty list
        Response response = new Response
        {
            Bookmarks = new List<Bookmark>()
        };

        return Task.FromResult(new SubsonicBaseResponse(response));
    }
}
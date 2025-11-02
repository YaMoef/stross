using FluentValidation;
using MediatR;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetStarredQuery(SubsonicGetStarredInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetStarredQueryValidator : AbstractValidator<SubsonicGetStarredQuery>
{
    public SubsonicGetStarredQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetStarredQueryHandler : IRequestHandler<SubsonicGetStarredQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicGetStarredQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public Task<SubsonicBaseResponse> Handle(SubsonicGetStarredQuery request, CancellationToken cancellationToken)
    {
        // TODO: Implement starring functionality - for now return empty list
        Response response = new Response
        {
            Starred = new Starred
            {
                Artist = [],
                Album = [],
                Song = []
            }
        };

        return Task.FromResult(new SubsonicBaseResponse(response));
    }
}
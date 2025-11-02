using FluentValidation;
using MediatR;
using Stross.Application.Slices.Subsonic.InputModels;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.Infrastructure;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Queries;

public sealed record SubsonicGetStarred2Query(SubsonicGetStarred2Input Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetStarred2QueryValidator : AbstractValidator<SubsonicGetStarred2Query>
{
    public SubsonicGetStarred2QueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetStarred2QueryHandler : IRequestHandler<SubsonicGetStarred2Query, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicGetStarred2QueryHandler(StrossContext context)
    {
        _context = context;
    }

    public Task<SubsonicBaseResponse> Handle(SubsonicGetStarred2Query request, CancellationToken cancellationToken)
    {
        // TODO: Implement starring functionality (ID3 organized) - for now return empty list
        Response response = new Response
        {
            Starred2 = new Starred2
            {
                Artist = [],
                Album = [],
                Song = []
            }
        };

        return Task.FromResult(new SubsonicBaseResponse(response));
    }
}
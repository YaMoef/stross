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

public sealed record SubsonicGetMusicFoldersQuery(SubsonicGetMusicFoldersInput Input) : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicGetMusicFoldersQueryValidator : AbstractValidator<SubsonicGetMusicFoldersQuery>
{
    public SubsonicGetMusicFoldersQueryValidator()
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input cannot be null");
    }
}

internal sealed class SubsonicGetMusicFoldersQueryHandler : IRequestHandler<SubsonicGetMusicFoldersQuery, SubsonicBaseResponse>
{
    private readonly StrossContext _context;

    public SubsonicGetMusicFoldersQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<SubsonicBaseResponse> Handle(SubsonicGetMusicFoldersQuery request, CancellationToken cancellationToken)
    {
        // Get all enabled providers to use as music folders
        List<Domain.Entities.Provider> providers = await _context.Providers
            .Where(p => p.Enabled)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

        Response response = new Response
        {
            MusicFolders = providers.Select(p => p.ToSubsonicMusicFolderResponse()).ToList()
        };

        return new SubsonicBaseResponse(response);
    }
}
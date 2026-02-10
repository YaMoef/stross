using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;

namespace Stross.Application.Slices.Provider.Commands;

public sealed record DeleteProviderCommand(long Id) : IRequest;

internal sealed class DeleteProviderCommandValidator : AbstractValidator<DeleteProviderCommand>
{
    public DeleteProviderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");
    }
}

internal sealed class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand>
{
    private readonly StrossContext _context;

    public DeleteProviderCommandHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Provider? provider =
            await _context.Providers
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (provider is null)
            throw new EntityNotFoundException(nameof(Provider));

        bool hasMusicTracks = await _context.MusicTracks
            .AnyAsync(mt => mt.ProviderId == request.Id, cancellationToken);

        if (hasMusicTracks)
            throw new ProviderException("Cannot delete provider with associated music tracks");

        bool hasExternalCreators = await _context.ExternalCreators
            .AnyAsync(ec => ec.ProviderId == request.Id, cancellationToken);

        if (hasExternalCreators)
            throw new ProviderException("Cannot delete provider with associated external creators");

        bool hasExternalAlbums = await _context.ExternalAlbums
            .AnyAsync(ea => ea.ProviderId == request.Id, cancellationToken);

        if (hasExternalAlbums)
            throw new ProviderException("Cannot delete provider with associated external albums");

        _context.Providers.Remove(provider);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

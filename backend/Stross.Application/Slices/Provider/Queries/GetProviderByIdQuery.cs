using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Slices.Provider.ResponseModels;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;

namespace Stross.Application.Slices.Provider.Queries;

public sealed record GetProviderByIdQuery(long Id) : IRequest<ProviderDetailsResponse>;

internal sealed class GetProviderByIdQueryValidator : AbstractValidator<GetProviderByIdQuery>
{
    public GetProviderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");
    }
}

internal sealed class GetProviderByIdQueryHandler : IRequestHandler<GetProviderByIdQuery, ProviderDetailsResponse>
{
    private readonly StrossContext _context;

    public GetProviderByIdQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<ProviderDetailsResponse> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.Provider? provider = await _context.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (provider is null)
            throw new EntityNotFoundException(nameof(Provider));

        return new ProviderDetailsResponse(provider.Id, provider.Name, provider.Url, provider.Enabled, provider.CreatedAt, provider.UpdatedAt);
    }
}

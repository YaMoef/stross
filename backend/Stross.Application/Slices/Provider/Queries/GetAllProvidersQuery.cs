using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Slices.Provider.ResponseModels;
using Stross.Infrastructure;

namespace Stross.Application.Slices.Provider.Queries;

public sealed record GetAllProvidersQuery() : IRequest<List<ProviderResponse>>;

internal sealed class GetAllProvidersQueryHandler : IRequestHandler<GetAllProvidersQuery, List<ProviderResponse>>
{
    private readonly StrossContext _context;

    public GetAllProvidersQueryHandler(StrossContext context)
    {
        _context = context;
    }

    public async Task<List<ProviderResponse>> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
    {
        List<ProviderResponse> providers = await _context.Providers
            .AsNoTracking()
            .Select(p => new ProviderResponse(p.Id, p.Name, p.Enabled))
            .ToListAsync(cancellationToken);

        return providers;
    }
}

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Shared.Helpers;
using Stross.Application.Slices.Provider.InputModels;
using Stross.Application.Slices.Provider.ResponseModels;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.Infrastructure.Services.GrpcService;
using ValidationException = Stross.Exception.Exceptions.ValidationException;

namespace Stross.Application.Slices.Provider.Commands;

public sealed record UpdateProviderCommand(long Id, UpdateProviderInput Input) : IRequest<ProviderDetailsResponse>;

internal sealed class UpdateProviderCommandValidator : AbstractValidator<UpdateProviderCommand>
{
    public UpdateProviderCommandValidator(IValidator<UpdateProviderInput> inputValidator)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");

        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class UpdateProviderCommandHandler : IRequestHandler<UpdateProviderCommand, ProviderDetailsResponse>
{
    private readonly StrossContext _context;
    private readonly IGrpcService _grpcService;

    public UpdateProviderCommandHandler(StrossContext context, IGrpcService grpcService)
    {
        _context = context;
        _grpcService = grpcService;
    }

    public async Task<ProviderDetailsResponse> Handle(UpdateProviderCommand request, CancellationToken cancellationToken)
    {
        Domain.Entities.Provider? provider =
            await _context.Providers.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (provider is null)
            throw new EntityNotFoundException(nameof(Provider));

        string name = request.Input.Name.SanitizeString() ?? throw new ValidationException(nameof(request.Input.Name), "Name is required.");

        Domain.Entities.Provider? existingProviderWithSameName =
            await _context.Providers.FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower() && p.Id != request.Id, cancellationToken);

        if (existingProviderWithSameName is not null)
            throw new EntityAlreadyExistsException(nameof(Provider));

        if (!string.Equals(provider.Url, request.Input.Url, StringComparison.OrdinalIgnoreCase))
        {
            bool isPingSuccessful = await _grpcService.PingAsync(request.Input.Url, cancellationToken);

            if (!isPingSuccessful)
                throw new ProviderException($"Failed to ping provider on url {request.Input.Url}");
        }

        provider.SetName(name);
        provider.SetUrl(request.Input.Url);
        provider.SetEnabled(request.Input.Enabled);

        await _context.SaveChangesAsync(cancellationToken);

        return new ProviderDetailsResponse(provider.Id, provider.Name, provider.Url, provider.Enabled, provider.CreatedAt, provider.UpdatedAt);
    }
}

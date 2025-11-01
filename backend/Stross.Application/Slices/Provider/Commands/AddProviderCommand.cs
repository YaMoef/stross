using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stross.Application.Shared.Helpers;
using Stross.Application.Slices.Provider.InputModels;
using Stross.Exception.Exceptions;
using Stross.Infrastructure;
using Stross.Infrastructure.Services.GrpcService;
using ValidationException = Stross.Exception.Exceptions.ValidationException;

namespace Stross.Application.Slices.Provider.Commands;

public sealed record AddProviderCommand(AddProviderInput Input) : IRequest<long>;

internal sealed class AddProviderCommandValidator : AbstractValidator<AddProviderCommand>
{
    public AddProviderCommandValidator(IValidator<AddProviderInput> inputValidator)
    {
        RuleFor(x => x.Input)
            .NotNull()
            .WithMessage("Input is required")
            .SetValidator(inputValidator);
    }
}

internal sealed class AddProviderCommandHandler : IRequestHandler<AddProviderCommand, long>
{
    private readonly IGrpcService _grpcService;
    private readonly StrossContext _context;

    public AddProviderCommandHandler(IGrpcService grpcService, StrossContext context)
    {
        _grpcService = grpcService;
        _context = context;
    }

    public async Task<long> Handle(AddProviderCommand request, CancellationToken cancellationToken)
    {
        string name = request.Input.Name.SanitizeString() ?? throw new ValidationException(nameof(request.Input.Name), "Name is required.");

        Domain.Entities.Provider? existingProvider =
            await _context.Providers.FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower(), cancellationToken);

        if (existingProvider is not null)
            throw new EntityAlreadyExistsException(nameof(Provider));

        bool isPingSuccessful = await _grpcService.PingAsync(request.Input.Url, cancellationToken);

        if (!isPingSuccessful)
            throw new ProviderException($"Failed to ping provider on url {request.Input.Url}");

        Domain.Entities.Provider newProvider =
            new Domain.Entities.Provider(name, request.Input.Url);

        _context.Providers.Add(newProvider);

        await _context.SaveChangesAsync(cancellationToken);

        return newProvider.Id;
    }
}
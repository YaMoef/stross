using MediatR;
using Stross.Application.Slices.Subsonic.ResponseModels;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.Commands;

public sealed record SubsonicPingCommand() : IRequest<SubsonicBaseResponse>;

internal sealed class SubsonicPingCommandHandler : IRequestHandler<SubsonicPingCommand, SubsonicBaseResponse>
{
    public Task<SubsonicBaseResponse> Handle(SubsonicPingCommand request, CancellationToken cancellationToken)
    {
        Response response = new Response();

        return Task.FromResult(new SubsonicBaseResponse(response));
    }
}
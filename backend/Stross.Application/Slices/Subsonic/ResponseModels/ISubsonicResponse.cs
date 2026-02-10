using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.ResponseModels;

public interface ISubsonicResponse
{
    public Response Response { get; }
}

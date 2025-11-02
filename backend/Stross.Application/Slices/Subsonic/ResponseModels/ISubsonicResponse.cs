using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.ResponseModels;

public interface ISubsonicResponse
{
    Response Response { get; }
}
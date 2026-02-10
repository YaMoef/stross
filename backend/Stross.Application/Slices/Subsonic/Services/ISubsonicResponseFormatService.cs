using Stross.Application.Slices.Subsonic.ResponseModels;

namespace Stross.Application.Slices.Subsonic.Services;

public interface ISubsonicResponseFormatService
{
    public SubsonicResponseFormat ResponseFormat { get; }
    public void SetResponseFormat(SubsonicResponseFormat format);
}

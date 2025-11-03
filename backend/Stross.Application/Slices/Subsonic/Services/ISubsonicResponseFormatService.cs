using Stross.Application.Slices.Subsonic.ResponseModels;

namespace Stross.Application.Slices.Subsonic.Services;

public interface ISubsonicResponseFormatService
{
    SubsonicResponseFormat ResponseFormat { get; }
    void SetResponseFormat(SubsonicResponseFormat format);
}
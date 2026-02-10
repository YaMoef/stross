using Stross.Application.Slices.Subsonic.ResponseModels;

namespace Stross.Application.Slices.Subsonic.Services;

internal sealed class SubsonicResponseFormatService : ISubsonicResponseFormatService
{
    private SubsonicResponseFormat _responseFormat = SubsonicResponseFormat.Json;

    public SubsonicResponseFormat ResponseFormat => _responseFormat;

    public void SetResponseFormat(SubsonicResponseFormat format)
    {
        _responseFormat = format;
    }
}

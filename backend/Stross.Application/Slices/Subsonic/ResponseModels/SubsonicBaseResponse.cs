using System.Text.Json.Serialization;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.ResponseModels;

public sealed record SubsonicBaseResponse([property: JsonPropertyName("subsonic-response")] Response Response) : ISubsonicResponse
{
    [JsonIgnore]
    public SubsonicResponseFormat Format { get; init; } = SubsonicResponseFormat.Json;
}
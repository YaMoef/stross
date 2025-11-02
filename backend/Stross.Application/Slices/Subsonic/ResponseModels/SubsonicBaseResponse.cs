using System.Text.Json.Serialization;
using Stross.SubsonicModels;

namespace Stross.Application.Slices.Subsonic.ResponseModels;

public sealed record SubsonicBaseResponse([property:JsonPropertyName("subsonic-response")]Response Response) : ISubsonicResponse;
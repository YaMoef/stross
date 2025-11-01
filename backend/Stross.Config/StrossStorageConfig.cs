namespace Stross.Config;

public class StrossStorageConfig
{
    public static readonly string SectionName = nameof(StrossStorageConfig);

    public required string BasePath { get; init; }
}
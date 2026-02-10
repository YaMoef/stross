namespace Stross.Config;

public class DatabaseConfig
{
    public static readonly string SectionName = nameof(DatabaseConfig);

    public required string ConnectionString { get; init; }
}

namespace Stross.Config;

public class StrossAdminAccount
{
    public static readonly string SectionName = nameof(StrossAdminAccount);

    public required string UserName { get; init; }
    public required string DisplayName { get; init; }
    public required string Password { get; init; }
}

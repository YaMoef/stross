namespace Stross.Application.Shared.Helpers;

public static class SanitzeHelper
{
    public static string? SanitizeString(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        return input.Trim();
    }
}
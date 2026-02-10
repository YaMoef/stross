namespace Stross.Application.Slices.Provider.ResponseModels;

public sealed record ProviderDetailsResponse(long Id, string Name, string Url, bool Enabled, DateTime CreatedAt, DateTime? UpdatedAt);

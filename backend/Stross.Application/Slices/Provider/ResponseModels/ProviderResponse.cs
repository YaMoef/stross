namespace Stross.Application.Slices.Provider.ResponseModels;

public sealed record ProviderResponse(long Id, string Name, bool Enabled);

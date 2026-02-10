using Stross.Domain.Helpers;
using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class UserApiKey : BaseEntity
{
    public string KeyName { get; private set; }

    private readonly string _apiKey;

    private UserApiKey()
    {
    }

    public UserApiKey(string apiKey, string keyName)
    {
        _apiKey = apiKey;
        KeyName = keyName;
    }

    // normally we do not put logic in the domain
    // but for security reasons we do not want to expose
    // the api key in the application
    public string GetMd5Hash(string salt)
    {
        if (string.IsNullOrEmpty(salt) || salt.Length < Constants.Constants.SaltMinSize)
            throw new ArgumentException("Salt is not sufficient", nameof(salt));

        return Md5Helper.GetMd5HashFromPasswordAndSalt(salt, _apiKey);
    }
}

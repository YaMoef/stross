using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; private set; }
    public string DisplayName { get; private set; }
    public string? Password { get; private set; }
    public bool IsDefaultUser { get; init; } = false;

    private readonly List<UserApiKey> _userApiKeys = [];
    public IReadOnlyCollection<UserApiKey> UserApiKeys => _userApiKeys;

    private readonly List<UserStarredItem> _starredItems = [];
    public IReadOnlyCollection<UserStarredItem> StarredItems => _starredItems;

    private User()
    {

    }

    public User(string userName, string displayName)
    {
        UserName = userName;
        DisplayName = displayName;
    }

    public User AddApiKey(string apiKey, string keyName)
    {
        _userApiKeys.Add(new UserApiKey(apiKey, keyName));

        return this;
    }

    public User AddStarredItem(UserStarredItem starredItem)
    {
        _starredItems.Add(starredItem);

        return this;
    }

    public User RemoveStarredItem(UserStarredItem unStarredItem)
    {
        _starredItems.Remove(unStarredItem);

        return this;
    }
}

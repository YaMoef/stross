using Stross.Domain.Seedwork;

namespace Stross.Domain.Entities;

public class Provider : BaseEntity
{
    public string Name { get; private set; }
    public bool Enabled { get; private set; } = true;
    public string Url { get; private set; }

    private Provider()
    {
    }

    public Provider(string name, string url)
    {
        Name = name;
        Url = url;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    public void SetUrl(string url)
    {
        Url = url;
    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
    }
}

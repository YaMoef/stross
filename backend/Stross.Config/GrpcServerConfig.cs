namespace Stross.Config;

public class GrpcServerConfig
{
    public int Port { get; set; }
    public required string Host { get; set; }
}
using System.Security.Cryptography;
using System.Text;

namespace Stross.Domain.Helpers;

public static class Md5Helper
{
    private static string ComputeMd5FromString(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder builder = new StringBuilder(hashBytes.Length * 2);

            for (int i = 0; i < hashBytes.Length; i++)
                builder.Append(hashBytes[i].ToString("x2")); // hex, lowercase

            return builder.ToString();
        }
    }

    public static string GetMd5HashFromPasswordAndSalt(string salt, string password)
    {
        return ComputeMd5FromString($"{password}{salt}");
    }
}
using System.Security.Cryptography;

namespace UrlShortener.Application.Services;

public sealed class ShortCodeGenerator
{
    private const string Alphabet =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate(int length = 7)
    {
        var result = new char[length];
        for (var i = 0; i < result.Length; i++)
            result[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        return new string(result);
    }
}

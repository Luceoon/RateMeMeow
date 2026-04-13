using System.Numerics;
using System.Text;

namespace RMM.Utils;

public static class Base62Converter
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Encode(Guid guid)
    {
        var bytes = guid.ToByteArray();
        // Ensure the value is positive to avoid sign bit issues with BigInteger
        // We add a trailing zero byte if needed.
        var absoluteBytes = new byte[bytes.Length + 1];
        Array.Copy(bytes, absoluteBytes, bytes.Length);
        
        var number = new BigInteger(absoluteBytes);
        var result = new StringBuilder();

        while (number > 0)
        {
            number = BigInteger.DivRem(number, 62, out var remainder);
            result.Insert(0, Alphabet[(int)remainder]);
        }

        return result.ToString();
    }

    public static Guid Decode(string base62)
    {
        BigInteger number = 0;
        foreach (var c in base62)
        {
            var index = Alphabet.IndexOf(c);
            if (index == -1) throw new ArgumentException("Invalid character in Base62 string.");
            number = number * 62 + index;
        }

        var bytes = number.ToByteArray();
        
        // BigInteger.ToByteArray() might include a sign byte (00 at the end if the number was positive and the last byte's high bit was set)
        // Or it might be shorter than 16 bytes if the leading bytes were zero.
        var guidBytes = new byte[16];
        
        // Copy up to 16 bytes. If it's shorter, the remaining bytes will stay 0.
        // If it's longer (due to sign bit), we only take the first 16 bytes.
        int bytesToCopy = Math.Min(bytes.Length, 16);
        Array.Copy(bytes, guidBytes, bytesToCopy);

        return new Guid(guidBytes);
    }
}

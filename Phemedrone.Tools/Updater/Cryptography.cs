using System;
using System.Linq;
using System.Security.Cryptography;

namespace Phemedrone.Tools.Updater;

public class Cryptography
{
    public static byte[] CombineArrays(params byte[][] arrays)
    {
        var result = new byte[arrays.Sum(a => a.Length)];

        var offset = 0;
        foreach (var a in arrays)
        {
            Array.Copy(a, 0, result, offset, a.Length);
            offset += a.Length;
        }

        return result;
    }
    
    public static byte[] GetRandomKey()
    {
        var key = new byte[16];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return key;
    }
    
    public static string GetRandomString()
    {
        var key = new byte[32];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(key);
        return Convert.ToBase64String(key).Replace("+", "").Replace("/", "").Substring(0, 16);
    }
    
    public static byte[] Rc4(byte[] data, byte[] key)
    {
        var s = new int[256];
        for (var k = 0; k < 256; k++)
        {
            s[k] = k;
        }
        
        var t = new int[256];
        
        if (key.Length == 256)
        {
            Buffer.BlockCopy(key, 0, t, 0, key.Length);
        }
        else
        {
            for (var _ = 0; _ < 256; _++)
            {
                t[_] = key[_ % key.Length];
            }
        }

        int i;
        var j = 0;
        for (i = 0; i < 256; i++)
        {
            j = (j + s[i] + t[i]) % 256;

            (s[i], s[j]) = (s[j], s[i]);
        }

        i = j = 0;
        var result = new byte[data.Length];
        for (var iteration = 0; iteration < data.Length; iteration++)
        {
            i = (i + 1) % 256;

            j = (j + s[i]) % 256;

            (s[i], s[j]) = (s[j], s[i]);

            var k = s[(s[i] + s[j]) % 256];
            
            result[iteration] = Convert.ToByte(data[iteration] ^ k);
        }

        return result;
    }
}
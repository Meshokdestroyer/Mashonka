using System;
using System.Collections.Generic;
using System.IO;

namespace Phemedrone.Tools.Updater;

public class ContentDecoder
{
    private Dictionary<string, string> DecodedValues { get; }
    public string this[string key] => DecodedValues.ContainsKey(key) ? DecodedValues[key] : string.Empty;
    
    public ContentDecoder(string encodedContentB64)
    {
        var decodedB64Content = Convert.FromBase64String(encodedContentB64);
        
        var key = new byte[16];
        var encodedContent = new byte[decodedB64Content.Length - 16];
        
        Array.Copy(decodedB64Content, 0, key, 0, key.Length);
        Array.Copy(decodedB64Content, 16, encodedContent, 0, encodedContent.Length);
        
        var rawContent = Cryptography.Rc4(encodedContent, key);
        DecodedValues = new Dictionary<string, string>();

        using var ms = new MemoryStream(rawContent);
        using var br = new BinaryReader(ms);

        var valuesLength = br.ReadInt32();

        for (var i = 0; i < valuesLength; i++)
        {
            DecodedValues.Add(br.ReadString(), br.ReadString());
        }
    }
}
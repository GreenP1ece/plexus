
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Domain.Models;
public class Message
{
    private string _serializedData = default!;

    public Message(object data) 
    {
        Data = data;
    }

    private Message()
    {
    }

    public int Id { get; private set; }

    public Type Type { get; private set; } = default!;

    public bool Published { get; private set; }

    public void MarkAsPublished() => Published = true;

    private static readonly JsonSerializerOptions _serializeOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public object Data
    {
        get
        {
            var deserializedData = JsonSerializer.Deserialize(_serializedData, Type, _serializeOptions);
            if (deserializedData != null)
                return deserializedData;
            throw new InvalidOperationException("Failed to deserialize message data.");
        }
        set
        {
            Type = value.GetType();
            _serializedData = JsonSerializer.Serialize(value, _serializeOptions);
        }
    }
}
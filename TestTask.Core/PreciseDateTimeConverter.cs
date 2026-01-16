using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestTask.Core;

public class PreciseDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-ddTHH:mm:sszzz";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
    {
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
            return default;

        return DateTime.Parse(dateString);
    }
    
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) 
    {
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(value, TimeZoneInfo.Local);
        writer.WriteStringValue(localTime.ToString(Format));
    }
}
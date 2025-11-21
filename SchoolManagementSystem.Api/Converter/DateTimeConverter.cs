using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolManagementSystem.Api.Converter
{
    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateTimeFormat));
        }

    }

    public class NullableDateTimeConverter : JsonConverter<DateTime?>
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm";

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return string.IsNullOrEmpty(value) ? null : DateTime.Parse(value);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(DateTimeFormat));
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace PHAMVIETDUNG_SE1885_A02_FE.Presentation.ViewModels
{
    public class NullableBooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return false; // Default to false when null is encountered
            }
            if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }
            if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }
            if (reader.TokenType == JsonTokenType.String)
            {
                if (bool.TryParse(reader.GetString(), out bool result))
                {
                    return result;
                }
            }
            return false;
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}

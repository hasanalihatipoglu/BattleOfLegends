using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoLLogic;

public class PositionJsonConverter : JsonConverter<Position>
{
    public override Position Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        System.Diagnostics.Debug.WriteLine("[PositionConverter] Read called");

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token for Position");
        }

        int row = 0;
        int column = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                var position = new Position(row, column);
                System.Diagnostics.Debug.WriteLine($"[PositionConverter] Created Position: ({row}, {column})");
                return position;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "Row":
                        row = reader.GetInt32();
                        System.Diagnostics.Debug.WriteLine($"[PositionConverter] Read Row: {row}");
                        break;
                    case "Column":
                        column = reader.GetInt32();
                        System.Diagnostics.Debug.WriteLine($"[PositionConverter] Read Column: {column}");
                        break;
                }
            }
        }

        throw new JsonException("Unexpected end of JSON while reading Position");
    }

    public override void Write(Utf8JsonWriter writer, Position value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("Row", value.Row);
        writer.WriteNumber("Column", value.Column);
        writer.WriteEndObject();
    }
}

using ChadGreen.Management.Shared.Contracts;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChadGreen.Management.Api.Services;

internal sealed class EngagementPresentationAssignmentDtoJsonConverter : JsonConverter<EngagementPresentationAssignmentDto>
{
    public override EngagementPresentationAssignmentDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringId = Normalize(reader.GetString());
            if (stringId is null)
            {
                throw new JsonException("Presentation entry string values must contain a non-empty id.");
            }

            return new EngagementPresentationAssignmentDto(stringId, -1);
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Presentation entries must be a string id or an object.");
        }

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var id = GetOptionalString(root, "id")
            ?? GetOptionalString(root, "presentationSlug")
            ?? GetOptionalString(root, "slug");

        if (id is null)
        {
            throw new JsonException("Presentation object entries must contain an id.");
        }

        return new EngagementPresentationAssignmentDto(
            id,
            GetOptionalDisplayOrder(root) ?? -1,
            GetOptionalString(root, "sessionName"),
            GetOptionalString(root, "date"),
            GetOptionalString(root, "time"),
            GetOptionalString(root, "timeZone"),
            GetOptionalString(root, "room"),
            GetOptionalString(root, "sessionUrl"));
    }

    public override void Write(Utf8JsonWriter writer, EngagementPresentationAssignmentDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("id", value.Id);
        writer.WriteNumber("displayOrder", value.DisplayOrder);
        WriteOptionalString(writer, "sessionName", value.SessionName);
        WriteOptionalString(writer, "date", value.Date);
        WriteOptionalString(writer, "time", value.Time);
        WriteOptionalString(writer, "timeZone", value.TimeZone);
        WriteOptionalString(writer, "room", value.Room);
        WriteOptionalString(writer, "sessionUrl", value.SessionUrl);
        writer.WriteEndObject();
    }

    private static string? GetOptionalString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            throw new JsonException($"Property '{propertyName}' must be a string when provided.");
        }

        return Normalize(value.GetString());
    }

    private static int? GetOptionalDisplayOrder(JsonElement root)
    {
        if (!root.TryGetProperty("displayOrder", out var value) ||
            value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetInt32(out var number) => number,
            JsonValueKind.String when int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) => parsed,
            _ => throw new JsonException("Property 'displayOrder' must be an integer when provided.")
        };
    }

    private static void WriteOptionalString(Utf8JsonWriter writer, string propertyName, string? value)
    {
        var normalized = Normalize(value);
        if (normalized is not null)
        {
            writer.WriteString(propertyName, normalized);
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

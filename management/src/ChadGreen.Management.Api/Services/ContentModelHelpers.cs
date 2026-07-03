using System.Globalization;
using System.Text.Json;

namespace ChadGreen.Management.Api.Services;

internal static class ContentModelHelpers
{
    public static string ToRelativePath(string siteRoot, string absolutePath)
        => Path.GetRelativePath(siteRoot, absolutePath).Replace('\\', '/');

    public static string NormalizeSlug(string? slug, string fallback)
    {
        var source = string.IsNullOrWhiteSpace(slug) ? fallback : slug;
        var normalized = new string(source
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());

        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        normalized = normalized.Trim('-');
        return string.IsNullOrWhiteSpace(normalized) ? "untitled" : normalized;
    }

    public static Dictionary<string, object?> AsDictionary(object? value)
    {
        return value as Dictionary<string, object?> ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    public static List<object?> AsList(object? value)
    {
        if (value is List<object?> nullableList)
        {
            return nullableList;
        }

        if (value is List<object> list)
        {
            return list.Select(item => (object?)item).ToList();
        }

        return [];
    }

    public static string? GetString(Dictionary<string, object?> dictionary, string key)
    {
        if (!dictionary.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value.ToString();
    }

    public static bool GetBool(Dictionary<string, object?> dictionary, string key, bool fallback = false)
    {
        if (!dictionary.TryGetValue(key, out var value) || value is null)
        {
            return fallback;
        }

        return value switch
        {
            bool boolValue => boolValue,
            string stringValue when bool.TryParse(stringValue, out var parsed) => parsed,
            _ => fallback
        };
    }

    public static List<string> GetStringList(Dictionary<string, object?> dictionary, string key)
    {
        if (!dictionary.TryGetValue(key, out var value) || value is null)
        {
            return [];
        }

        return AsList(value)
            .Select(item => item?.ToString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToList();
    }

    public static List<int> GetIntList(Dictionary<string, object?> dictionary, string key)
    {
        if (!dictionary.TryGetValue(key, out var value) || value is null)
        {
            return [];
        }

        var output = new List<int>();
        foreach (var item in AsList(value))
        {
            if (item is int intValue)
            {
                output.Add(intValue);
                continue;
            }

            if (item is long longValue && longValue is >= int.MinValue and <= int.MaxValue)
            {
                output.Add((int)longValue);
                continue;
            }

            if (item is string text && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                output.Add(parsed);
            }
        }

        return output;
    }

    public static string CoerceScalarString(object? value, string fallback)
    {
        if (TryGetScalarString(value, out var scalar))
        {
            var normalized = scalar.Trim();
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                return normalized;
            }
        }

        var normalizedFallback = fallback.Trim();
        return string.IsNullOrWhiteSpace(normalizedFallback) ? fallback : normalizedFallback;
    }

    private static bool TryGetScalarString(object? value, out string scalar)
    {
        switch (value)
        {
            case null:
                scalar = string.Empty;
                return false;
            case string stringValue:
                scalar = stringValue;
                return true;
            case bool boolValue:
                scalar = boolValue ? bool.TrueString : bool.FalseString;
                return true;
            case byte or sbyte or short or ushort or int or uint or long or ulong or
                float or double or decimal:
                scalar = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
                return true;
            case DateOnly dateOnly:
                scalar = dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                return true;
            case DateTime dateTime:
                scalar = dateTime.ToString("O", CultureInfo.InvariantCulture);
                return true;
            case DateTimeOffset dateTimeOffset:
                scalar = dateTimeOffset.ToString("O", CultureInfo.InvariantCulture);
                return true;
            case Guid guid:
                scalar = guid.ToString();
                return true;
            case JsonElement jsonElement:
                return TryGetScalarStringFromJsonElement(jsonElement, out scalar);
            case System.Collections.IDictionary:
                scalar = string.Empty;
                return false;
            case System.Collections.IEnumerable when value is not string:
                scalar = string.Empty;
                return false;
            default:
                scalar = value.ToString() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(scalar);
        }
    }

    private static bool TryGetScalarStringFromJsonElement(JsonElement value, out string scalar)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                scalar = value.GetString() ?? string.Empty;
                return true;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                scalar = value.ToString();
                return true;
            default:
                scalar = string.Empty;
                return false;
        }
    }
}

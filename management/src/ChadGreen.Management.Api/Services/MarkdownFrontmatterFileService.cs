using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ChadGreen.Management.Api.Services;

public record MarkdownDocument(Dictionary<string, object?> Frontmatter, string Body);

public interface IMarkdownFrontmatterFileService
{
    Task<MarkdownDocument> ReadAsync(string absolutePath, CancellationToken cancellationToken = default);

    Task WriteAsync(string absolutePath, MarkdownDocument document, CancellationToken cancellationToken = default);

    Task EnsureNotModifiedSinceAsync(string absolutePath, DateTimeOffset? expectedLastModifiedUtc, CancellationToken cancellationToken = default);
}

public sealed class MarkdownFrontmatterFileService : IMarkdownFrontmatterFileService
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private readonly ISerializer _serializer = new SerializerBuilder()
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
        .Build();

    public async Task<MarkdownDocument> ReadAsync(string absolutePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Markdown file was not found.", absolutePath);
        }

        var text = await File.ReadAllTextAsync(absolutePath, cancellationToken);
        if (!TrySplitFrontmatter(text, out var yaml, out var body))
        {
            return new MarkdownDocument(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase), text);
        }

        if (string.IsNullOrWhiteSpace(yaml))
        {
            return new MarkdownDocument(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase), body);
        }

        var parsed = _deserializer.Deserialize<object?>(yaml);
        var normalized = NormalizeValue(parsed) as Dictionary<string, object?> ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        return new MarkdownDocument(normalized, body);
    }

    public async Task WriteAsync(string absolutePath, MarkdownDocument document, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        var sanitizedFrontmatter = PruneNullValues(document.Frontmatter) as Dictionary<string, object?>
            ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var yaml = _serializer.Serialize(sanitizedFrontmatter);
        var body = document.Body ?? string.Empty;
        var content = $"---{Environment.NewLine}{yaml}---{Environment.NewLine}{body}";
        await File.WriteAllTextAsync(absolutePath, content, cancellationToken);
    }

    public Task EnsureNotModifiedSinceAsync(string absolutePath, DateTimeOffset? expectedLastModifiedUtc, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!expectedLastModifiedUtc.HasValue)
        {
            return Task.CompletedTask;
        }

        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Markdown file was not found.", absolutePath);
        }

        var currentLastModifiedUtc = File.GetLastWriteTimeUtc(absolutePath);
        var expectedUtc = expectedLastModifiedUtc.Value.UtcDateTime;
        if (Math.Abs((currentLastModifiedUtc - expectedUtc).TotalSeconds) > 1)
        {
            throw new SaveConflictException(
                absolutePath,
                expectedLastModifiedUtc,
                DateTime.SpecifyKind(currentLastModifiedUtc, DateTimeKind.Utc));
        }

        return Task.CompletedTask;
    }

    private static bool TrySplitFrontmatter(string text, out string yaml, out string body)
    {
        yaml = string.Empty;
        body = text;

        if (!text.StartsWith("---", StringComparison.Ordinal))
        {
            return false;
        }

        using var reader = new StringReader(text);
        var firstLine = reader.ReadLine();
        if (!string.Equals(firstLine, "---", StringComparison.Ordinal))
        {
            return false;
        }

        var yamlLines = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (line == "---")
            {
                yaml = string.Join(Environment.NewLine, yamlLines);
                body = reader.ReadToEnd() ?? string.Empty;
                return true;
            }

            yamlLines.Add(line);
        }

        return false;
    }

    private static object? NormalizeValue(object? value)
    {
        return value switch
        {
            null => null,
            IDictionary<object, object> objectMap => objectMap
                .ToDictionary(
                    keyValue => keyValue.Key.ToString() ?? string.Empty,
                    keyValue => NormalizeValue(keyValue.Value),
                    StringComparer.OrdinalIgnoreCase),
            IDictionary<string, object> stringMap => stringMap
                .ToDictionary(
                    keyValue => keyValue.Key,
                    keyValue => NormalizeValue(keyValue.Value),
                    StringComparer.OrdinalIgnoreCase),
            IEnumerable<object> sequence => sequence.Select(NormalizeValue).ToList(),
            _ => value
        };
    }

    private static object? PruneNullValues(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is Dictionary<string, object?> map)
        {
            return map
                .Select(pair => new KeyValuePair<string, object?>(pair.Key, PruneNullValues(pair.Value)))
                .Where(pair => pair.Value is not null)
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }

        if (value is IEnumerable<object?> sequence)
        {
            return sequence
                .Select(PruneNullValues)
                .Where(item => item is not null)
                .ToList();
        }

        return value;
    }
}

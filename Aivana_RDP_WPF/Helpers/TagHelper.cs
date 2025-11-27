using System.Text.Json;

namespace Aivana_RDP_WPF.Helpers;

/// <summary>
/// Helper for tag management (JSON serialization/deserialization).
/// </summary>
public static class TagHelper
{
    public static List<string> ParseTags(string tagsJson)
    {
        if (string.IsNullOrWhiteSpace(tagsJson) || tagsJson == "[]")
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static string SerializeTags(List<string> tags)
    {
        if (tags == null || tags.Count == 0)
            return "[]";

        return JsonSerializer.Serialize(tags);
    }

    public static bool HasTag(string tagsJson, string tag)
    {
        var tags = ParseTags(tagsJson);
        return tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
    }

    public static List<string> AddTag(string tagsJson, string tag)
    {
        var tags = ParseTags(tagsJson);
        if (!tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            tags.Add(tag);
        }
        return tags;
    }

    public static List<string> RemoveTag(string tagsJson, string tag)
    {
        var tags = ParseTags(tagsJson);
        tags.RemoveAll(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        return tags;
    }
}


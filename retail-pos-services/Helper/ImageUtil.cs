static class ImageUtil
{
    public static List<string> ToList(string jsonOrSingle)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(jsonOrSingle)) return list;

        try
        {
            if (jsonOrSingle.TrimStart().StartsWith("["))
                list = System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonOrSingle) ?? new();
            else
                list.Add(jsonOrSingle);
        }
        catch
        {
            list.Add(jsonOrSingle);
        }

        return Normalize(list);
    }

    public static List<string> Normalize(IEnumerable<string> paths)
    {
        return paths
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x
                .Replace("http://localhost:7128", "")
                .Replace("https://localhost:7128", "")
                .Trim()
                .TrimStart('/'))
            .Select(x => "/" + x.Replace("\\", "/"))
            .Distinct()
            .ToList();
    }

    public static string ToJson(List<string> paths)
        => System.Text.Json.JsonSerializer.Serialize(paths ?? new List<string>());
}
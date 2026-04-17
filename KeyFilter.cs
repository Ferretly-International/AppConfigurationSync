namespace AppConfigurationSync;

public static class KeyFilter
{
    public static bool ShouldIgnore(string key, IEnumerable<string> keysToIgnore) =>
        keysToIgnore.Any(prefix =>
            key == prefix || key.StartsWith(prefix + ":", StringComparison.Ordinal));
}

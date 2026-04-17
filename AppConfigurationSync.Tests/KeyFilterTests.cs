using AppConfigurationSync;

namespace AppConfigurationSync.Tests;

public class KeyFilterTests
{
    [Fact]
    public void ShouldIgnoreKey_WhenKeyExactlyMatchesIgnoreEntry()
    {
        var ignore = new[] { "AdminApp" };
        Assert.True(KeyFilter.ShouldIgnore("AdminApp", ignore));
    }

    [Fact]
    public void ShouldIgnoreKey_WhenKeyStartsWithPrefixFollowedByColon()
    {
        var ignore = new[] { "AdminApp" };
        Assert.True(KeyFilter.ShouldIgnore("AdminApp:ConnectionString", ignore));
    }

    [Fact]
    public void ShouldNotIgnoreKey_WhenKeyOnlyPartiallyMatchesPrefix()
    {
        var ignore = new[] { "AdminApp" };
        Assert.False(KeyFilter.ShouldIgnore("AdminAppOther:Setting", ignore));
    }

    [Fact]
    public void ShouldNotIgnoreKey_WhenIgnoreListIsEmpty()
    {
        Assert.False(KeyFilter.ShouldIgnore("AdminApp:ConnectionString", []));
    }

    [Fact]
    public void ShouldIgnoreKey_WhenAnyOfMultiplePrefixesMatch()
    {
        var ignore = new[] { "AdminApp", "LegacyFeature" };
        Assert.True(KeyFilter.ShouldIgnore("LegacyFeature:Enabled", ignore));
    }

    [Fact]
    public void ShouldNotIgnoreKey_WhenNoPrefixesMatch()
    {
        var ignore = new[] { "AdminApp", "LegacyFeature" };
        Assert.False(KeyFilter.ShouldIgnore("NewFeature:Enabled", ignore));
    }
}

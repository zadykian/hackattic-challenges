using System.Configuration;
using System.Text.Json;

namespace Hackattic.Challenges.ApiAccess;

internal static class AccessToken
{
    private static readonly string[] configFiles =
    [
        "appsettings.local.json",
        "appsettings.json"
    ];

    public static string Value()
        => configFiles
            .First(File.Exists)
            .To(File.ReadAllText)
            .To(content => JsonSerializer.Deserialize<JsonElement>(content))
            .GetProperty("accessToken")
            .GetString()
            ?? throw new ConfigurationErrorsException("Access token is not provided!");
}
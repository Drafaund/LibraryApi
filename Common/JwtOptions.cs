namespace LibraryApi.Common;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinKeyLength = 32;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    // Never in appsettings.json: comes from the .env file or the Jwt__Key environment variable.
    public string Key { get; set; } = string.Empty;

    public TimeSpan AccessTokenLifetime { get; set; }

    public TimeSpan RefreshTokenLifetime { get; set; }
}

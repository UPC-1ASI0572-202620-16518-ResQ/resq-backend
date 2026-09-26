namespace ResQ.API.IAM.Infrastructure.Tokens.JWT.Configuration;

/// <summary>
///     Configuration settings used by JWT token generation and validation.
/// </summary>
public class TokenSettings
{
    /// <summary>
    ///     Secret signing key used to sign JWT tokens. Configure in appsettings.json.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
}

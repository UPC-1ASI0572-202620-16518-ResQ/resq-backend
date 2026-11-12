using System.Text.Json.Serialization;
using ResQ.API.IAM.Domain.Model.ValueObjects;

namespace ResQ.API.IAM.Domain.Model.Aggregates;

/// <summary>
///     User aggregate root. Every user belongs to exactly one organization.
/// </summary>
public class User(string username, string passwordHash, Roles role, Guid organizationId)
{
    public User() : this(string.Empty, string.Empty, Roles.Citizen, Guid.Empty)
    {
    }

    public int Id { get; }

    public string Username { get; private set; } = username;

    [JsonIgnore]
    public string PasswordHash { get; private set; } = passwordHash;

    public Roles Role { get; } = role;

    /// <summary>
    ///     Organization the user belongs to. Used to scope every request of the user.
    /// </summary>
    public Guid OrganizationId { get; private set; } = organizationId;

    public User UpdateUsername(string username)
    {
        Username = username;
        return this;
    }

    public User UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        return this;
    }
}

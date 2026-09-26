namespace ResQ.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Value Object that encapsulates the first and last name of a user.
/// </summary>
public record FullName(string FirstName, string LastName)
{
    public FullName() : this(string.Empty, string.Empty)
    {
    }

    public string GetFullName() => $"{FirstName} {LastName}".Trim();
}

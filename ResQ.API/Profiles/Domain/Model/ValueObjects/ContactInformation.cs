namespace ResQ.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Value Object that centralizes the communication channels of a user.
/// </summary>
public record ContactInformation(string Email, string PhoneNumber)
{
    public ContactInformation() : this(string.Empty, string.Empty)
    {
    }

    /// <summary>
    ///     Checks if the contact information has a valid phone number.
    /// </summary>
    public bool HasValidPhone()
    {
        return !string.IsNullOrWhiteSpace(PhoneNumber);
    }
}

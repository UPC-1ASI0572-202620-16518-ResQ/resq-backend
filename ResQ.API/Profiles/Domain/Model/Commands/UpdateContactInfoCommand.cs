namespace ResQ.API.Profiles.Domain.Model.Commands;

public record UpdateContactInfoCommand(int UserId, string? NewEmail, string? NewPhoneNumber);

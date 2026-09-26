namespace ResQ.API.Profiles.Domain.Model.Commands;

public record CreateUserProfileCommand(int UserId, string FirstName, string LastName, string Email);

using System.Text.RegularExpressions;
using ResQ.API.IAM.Application.Internal.OutboundServices;
using ResQ.API.IAM.Domain.Model.Aggregates;
using ResQ.API.IAM.Domain.Model.Commands;
using ResQ.API.IAM.Domain.Model.ValueObjects;
using ResQ.API.IAM.Domain.Repositories;
using ResQ.API.IAM.Domain.Services;
using ResQ.API.Shared.Domain.Repositories;

namespace ResQ.API.IAM.Application.Internal.CommandServices;

/// <summary>
///     Handles sign-in, sign-up (new organization + Admin) and creation of organization members.
/// </summary>
public class UserCommandService(
    IUserRepository userRepository,
    IOrganizationRepository organizationRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork)
    : IUserCommandService
{
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 72; // BCrypt only uses the first 72 bytes

    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Username) || string.IsNullOrEmpty(command.Password))
            throw new UnauthorizedAccessException("Invalid username or password");

        var user = await userRepository.FindByUsernameAsync(command.Username.Trim());

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password");

        var token = tokenService.GenerateToken(user);

        return (user, token);
    }

    public async Task<int> Handle(SignUpCommand command)
    {
        // Validated before saving so a failed profile creation never leaves a user without profile
        ValidateUserData(command.FirstName, command.LastName, command.Email, command.Username, command.Password);

        var username = command.Username.Trim();
        EnsureUsernameIsFree(username);

        var organizationName = string.IsNullOrWhiteSpace(command.OrganizationName)
            ? $"{username} organization"
            : command.OrganizationName;
        var organization = Organization.Register(organizationName);

        // The creator of an organization is its administrator
        var user = new User(username, hashingService.HashPassword(command.Password), Roles.Admin, organization.Id);

        await organizationRepository.AddAsync(organization);
        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();
        return user.Id;
    }

    public async Task<int> Handle(CreateMemberCommand command)
    {
        ValidateUserData(command.FirstName, command.LastName, command.Email, command.Username, command.Password);

        if (command.Role is not (Roles.Citizen or Roles.Volunteer))
            throw new ArgumentException("Invalid role. Allowed values for members: citizen, volunteer.");

        if (await organizationRepository.FindByOrganizationIdAsync(command.OrganizationId) is null)
            throw new KeyNotFoundException("Organization not found.");

        var username = command.Username.Trim();
        EnsureUsernameIsFree(username);

        var user = new User(username, hashingService.HashPassword(command.Password), command.Role,
            command.OrganizationId);

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();
        return user.Id;
    }

    private void EnsureUsernameIsFree(string username)
    {
        if (userRepository.ExistsByUsername(username))
            throw new InvalidOperationException($"Username {username} is already taken");
    }

    private static void ValidateUserData(string firstName, string lastName, string email, string username,
        string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.");

        if (username.Trim().Length > 50)
            throw new ArgumentException("Username cannot exceed 50 characters.");

        ValidatePassword(password);

        if (string.IsNullOrWhiteSpace(firstName) || firstName.Trim().Length > 50)
            throw new ArgumentException("First name is required and cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(lastName) || lastName.Trim().Length > 50)
            throw new ArgumentException("Last name is required and cannot exceed 50 characters.");

        if (string.IsNullOrWhiteSpace(email) || email.Trim().Length > 150 || !EmailPattern.IsMatch(email.Trim()))
            throw new ArgumentException("Email must be a valid email address of up to 150 characters.");
    }

    /// <summary>
    ///     Password policy: 8 to 72 characters, with at least one letter and one digit.
    /// </summary>
    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.");

        if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
            throw new ArgumentException(
                $"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters.");

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new ArgumentException("Password must contain at least one letter and one digit.");
    }

    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
}

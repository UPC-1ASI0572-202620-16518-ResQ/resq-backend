using ResQ.API.IAM.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace ResQ.API.IAM.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
///     EF Core model configuration for IAM bounded context entities.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Applies the IAM entity configurations to the model builder.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        // IAM Context

        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<User>().Property(u => u.Username).IsRequired();
        builder.Entity<User>().Property(u => u.PasswordHash).IsRequired();

        builder.Entity<User>()
            .Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();
    }
}

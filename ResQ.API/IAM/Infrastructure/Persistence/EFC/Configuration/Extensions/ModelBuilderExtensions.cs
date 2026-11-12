using ResQ.API.IAM.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace ResQ.API.IAM.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
///     EF Core configuration for the IAM bounded context.
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        // Organizations
        builder.Entity<Organization>().HasKey(o => o.Id);
        builder.Entity<Organization>().Property(o => o.Id).ValueGeneratedNever();
        builder.Entity<Organization>().Property(o => o.Name).IsRequired().HasMaxLength(120);
        builder.Entity<Organization>().Property(o => o.CreatedAt).IsRequired();

        // Users
        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<User>().Property(u => u.Username).IsRequired();
        builder.Entity<User>().Property(u => u.PasswordHash).IsRequired();
        builder.Entity<User>().Property(u => u.OrganizationId).IsRequired();

        builder.Entity<User>()
            .Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();
    }
}

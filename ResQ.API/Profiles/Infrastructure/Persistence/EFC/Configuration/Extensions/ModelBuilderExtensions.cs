using Microsoft.EntityFrameworkCore;
using ResQ.API.Profiles.Domain.Model.Aggregates;

namespace ResQ.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyUserConfiguration(this ModelBuilder builder)
    {
        builder.Entity<UserProfile>().ToTable("UserProfiles");
        builder.Entity<UserProfile>().HasKey(p => p.Id);
        
        // Ensure ID is not auto-generated since it comes from IAM
        builder.Entity<UserProfile>().Property(p => p.Id).ValueGeneratedNever();
        
        // Embedded Pattern for Value Objects
        builder.Entity<UserProfile>().OwnsOne(p => p.Name, n =>
        {
            n.Property(p => p.FirstName).HasColumnName("FirstName").HasMaxLength(50).IsRequired();
            n.Property(p => p.LastName).HasColumnName("LastName").HasMaxLength(50).IsRequired();
        });

        builder.Entity<UserProfile>().OwnsOne(p => p.ContactInfo, c =>
        {
            c.Property(p => p.Email).HasColumnName("Email").HasMaxLength(150).IsRequired();
            c.Property(p => p.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(20);
        });

        builder.Entity<UserProfile>().OwnsOne(p => p.Preferences, pref =>
        {

            pref.Property(p => p.TimeZone).HasColumnName("TimeZone").HasMaxLength(50).HasDefaultValue("UTC");
            pref.Property(p => p.ReceiveSmsAlerts).HasColumnName("ReceiveSmsAlerts").HasDefaultValue(false);
        });

        return builder;
    }
}

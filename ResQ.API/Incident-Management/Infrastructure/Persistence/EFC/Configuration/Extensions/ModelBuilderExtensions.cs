using Microsoft.EntityFrameworkCore;
using ResQ.API.Incident_Management.Domain.Model.Aggregates;
using ResQ.API.Incident_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Incident_Management.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyIncidentConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Incident>().ToTable("Incidents");

        builder.Entity<Incident>().HasKey(i => i.Id);

        // IncidentId Value Object
        builder.Entity<Incident>().Property(i => i.Id).HasConversion(incidentId => incidentId.Value, value => new IncidentId(value)).ValueGeneratedNever();

        // ZoneId is an external reference to Building Management
        builder.Entity<Incident>().Property(i => i.ZoneId).HasConversion(zoneId => zoneId.Value, value => new ZoneId(value)).IsRequired();
        builder.Entity<Incident>().Property(i => i.OrganizationId).IsRequired();
        builder.Entity<Incident>().HasIndex(i => new { i.OrganizationId, i.RiskDetectionId }).IsUnique();

        // Risk information
        builder.Entity<Incident>().Property(i => i.Type).HasConversion<string>().IsRequired();

        builder.Entity<Incident>().Property(i => i.Level).HasConversion<string>().IsRequired();

        // Incident lifecycle
        builder.Entity<Incident>().Property(i => i.Status).HasConversion<string>().IsRequired();

        // Optional responsible user
        builder.Entity<Incident>().Property(i => i.AssignedTo)
            .HasConversion(
                attendantId => attendantId == null ? (Guid?)null : attendantId.Value,
                value => value.HasValue ? new AttendantId(value.Value) : null);

        builder.Entity<Incident>().Property(i => i.CreatedAt).IsRequired();

        builder.Entity<Incident>().Property(i => i.ResolvedAt);

        builder.Entity<Incident>().Property(i => i.ResolutionNotes);

        builder.Entity<Incident>().HasMany(i => i.HumanActions)
            .WithOne().HasForeignKey(action => action.IncidentId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<IncidentHumanAction>().ToTable("IncidentHumanActions");
        builder.Entity<IncidentHumanAction>().HasKey(action => action.Id);
        builder.Entity<IncidentHumanAction>()
            .Property(action => action.IncidentId)
            .HasConversion(id => id.Value, value => new IncidentId(value))
            .IsRequired();
        builder.Entity<IncidentHumanAction>().Property(action => action.ActionCode).HasMaxLength(80).IsRequired();
        builder.Entity<IncidentHumanAction>().Property(action => action.Notes).HasMaxLength(1000);
        builder.Entity<IncidentHumanAction>().HasIndex(action => new { action.IncidentId, action.PerformedAt });

        builder.Entity<Incident>().HasMany(i => i.SafetyVerifications)
            .WithOne().HasForeignKey(verification => verification.IncidentId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<IncidentSafetyVerification>().ToTable("IncidentSafetyVerifications");
        builder.Entity<IncidentSafetyVerification>().HasKey(verification => verification.Id);
        builder.Entity<IncidentSafetyVerification>()
            .Property(verification => verification.IncidentId)
            .HasConversion(id => id.Value, value => new IncidentId(value))
            .IsRequired();
        builder.Entity<IncidentSafetyVerification>().Property(verification => verification.Reason).HasMaxLength(1000).IsRequired();
        builder.Entity<IncidentSafetyVerification>().Property(verification => verification.RuleSnapshot).HasMaxLength(2000);
        builder.Entity<IncidentSafetyVerification>().HasIndex(verification => new { verification.IncidentId, verification.VerifiedAt });

        return builder;
    }
}

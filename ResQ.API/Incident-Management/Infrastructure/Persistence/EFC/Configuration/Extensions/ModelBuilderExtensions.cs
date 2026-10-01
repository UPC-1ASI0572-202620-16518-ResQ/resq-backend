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

        return builder;
    }
}
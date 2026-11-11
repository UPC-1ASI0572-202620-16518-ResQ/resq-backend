using Microsoft.EntityFrameworkCore;
using ResQ.API.Connectivity_Management.Domain.Model.Aggregates;
using ResQ.API.Connectivity_Management.Domain.Model.Entities;
using ResQ.API.Connectivity_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Connectivity_Management.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring the Connectivity Management persistence model.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the Connectivity Management entity configurations.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    public static ModelBuilder ApplyConnectivityManagementConfiguration(this ModelBuilder builder)
    {
        // SensorConnection Aggregate
        builder.Entity<SensorConnection>().ToTable("SensorConnections");
        builder.Entity<SensorConnection>().HasKey(c => c.Id);
        builder.Entity<SensorConnection>().Property(c => c.Id).ValueGeneratedNever();
        builder.Entity<SensorConnection>().Property(c => c.OrganizationId).IsRequired();
        builder.Entity<SensorConnection>().Property(c => c.SensorId).IsRequired();
        builder.Entity<SensorConnection>().Property(c => c.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Entity<SensorConnection>().Property(c => c.LastHeartbeat).HasPrecision(6).IsRequired();
        builder.Entity<SensorConnection>().Property(c => c.StatusChangedAt).HasPrecision(6).IsRequired();
        builder.Entity<SensorConnection>().Property(c => c.SignalStrength)
            .HasConversion(signal => signal == null ? (int?)null : signal.Dbm,
                dbm => dbm.HasValue ? SignalStrength.Create(dbm.Value) : null);
        builder.Entity<SensorConnection>().Ignore(c => c.DomainEvents);
        builder.Entity<SensorConnection>().HasIndex(c => new { c.OrganizationId, c.SensorId }).IsUnique();

        // HeartbeatRecord belongs to the SensorConnection Aggregate
        builder.Entity<HeartbeatRecord>().ToTable("HeartbeatRecords");
        builder.Entity<HeartbeatRecord>().HasKey(h => h.Id);
        builder.Entity<HeartbeatRecord>().Property(h => h.Id).ValueGeneratedNever();
        builder.Entity<HeartbeatRecord>().Property<Guid>("SensorConnectionId").HasColumnName("sensor_connection_id");
        builder.Entity<HeartbeatRecord>().Property(h => h.Timestamp).HasPrecision(6).IsRequired();
        builder.Entity<HeartbeatRecord>().Property(h => h.SignalStrength)
            .HasConversion(signal => signal == null ? (int?)null : signal.Dbm,
                dbm => dbm.HasValue ? SignalStrength.Create(dbm.Value) : null);
        builder.Entity<SensorConnection>().HasMany(c => c.Heartbeats).WithOne()
            .HasForeignKey("SensorConnectionId").OnDelete(DeleteBehavior.Cascade);

        return builder;
    }
}

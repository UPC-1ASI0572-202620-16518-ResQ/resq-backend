using Microsoft.EntityFrameworkCore;
using ResQ.API.Monitoring.Domain.Model.Aggregates;

namespace ResQ.API.Monitoring.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Entity Framework Core configuration for the Monitoring bounded context.
/// </summary>
public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyMonitoringConfiguration(this ModelBuilder builder)
    {
        // Measurement Aggregate
        builder.Entity<Measurement>().ToTable("Measurements");
        builder.Entity<Measurement>().HasKey(m => m.Id);
        builder.Entity<Measurement>().Property(m => m.Id).ValueGeneratedNever();
        builder.Entity<Measurement>().Property(m => m.OrganizationId).IsRequired();
        builder.Entity<Measurement>().Property(m => m.DeviceId).IsRequired();
        builder.Entity<Measurement>().Property(m => m.BuildingId).IsRequired();
        builder.Entity<Measurement>().Property(m => m.ZoneId);
        builder.Entity<Measurement>().Property(m => m.VariableType).HasMaxLength(50).IsRequired();
        builder.Entity<Measurement>().Property(m => m.MeasuredAt).HasPrecision(6).IsRequired();
        builder.Entity<Measurement>().Property(m => m.RecordedAt).HasPrecision(6).IsRequired();

        builder.Entity<Measurement>().OwnsOne(m => m.MeasurementValue, value =>
        {
            value.Property(v => v.Value)
                .HasColumnName("Value")
                .HasPrecision(18, 4)
                .IsRequired();

            value.Property(v => v.Unit)
                .HasColumnName("Unit")
                .HasMaxLength(30)
                .IsRequired();
        });
        builder.Entity<Measurement>().Navigation(m => m.MeasurementValue).IsRequired();

        builder.Entity<Measurement>()
            .HasIndex(m => new { m.OrganizationId, m.DeviceId, m.MeasuredAt });

        builder.Entity<Measurement>()
            .HasIndex(m => new { m.OrganizationId, m.ZoneId, m.MeasuredAt });

        // Device Monitoring State
        builder.Entity<DeviceMonitoringState>().ToTable("DeviceMonitoringStates");
        builder.Entity<DeviceMonitoringState>().HasKey(s => s.Id);
        builder.Entity<DeviceMonitoringState>().Property(s => s.Id).ValueGeneratedNever();
        builder.Entity<DeviceMonitoringState>().Property(s => s.OrganizationId).IsRequired();
        builder.Entity<DeviceMonitoringState>().Property(s => s.DeviceId).IsRequired();
        builder.Entity<DeviceMonitoringState>().Property(s => s.ZoneId);
        builder.Entity<DeviceMonitoringState>().Property(s => s.Availability)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Entity<DeviceMonitoringState>().Property(s => s.LastMeasurementAt).HasPrecision(6);
        builder.Entity<DeviceMonitoringState>().Property(s => s.UpdatedAt).HasPrecision(6).IsRequired();
        builder.Entity<DeviceMonitoringState>()
            .HasIndex(s => new { s.OrganizationId, s.DeviceId })
            .IsUnique();

        // Zone Monitoring State
        builder.Entity<ZoneMonitoringState>().ToTable("ZoneMonitoringStates");
        builder.Entity<ZoneMonitoringState>().HasKey(s => s.Id);
        builder.Entity<ZoneMonitoringState>().Property(s => s.Id).ValueGeneratedNever();
        builder.Entity<ZoneMonitoringState>().Property(s => s.OrganizationId).IsRequired();
        builder.Entity<ZoneMonitoringState>().Property(s => s.ZoneId).IsRequired();
        builder.Entity<ZoneMonitoringState>().Property(s => s.BuildingId).IsRequired();
        builder.Entity<ZoneMonitoringState>().Property(s => s.LastUpdatedAt).HasPrecision(6).IsRequired();
        builder.Entity<ZoneMonitoringState>().Property(s => s.ActiveRiskId);
        builder.Entity<ZoneMonitoringState>().Property(s => s.ConditionCode).HasMaxLength(50).IsRequired();
        builder.Entity<ZoneMonitoringState>()
            .HasIndex(s => new { s.OrganizationId, s.ZoneId })
            .IsUnique();

        return builder;
    }
}
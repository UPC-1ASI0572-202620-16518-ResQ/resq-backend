using Microsoft.EntityFrameworkCore;
using ResQ.API.Alert_Management.Domain.Model.Aggregates;
using ResQ.API.Alert_Management.Domain.Model.Entities;

namespace ResQ.API.Alert_Management.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring the Alert Management persistence model.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the Alert Management entity configurations.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    public static ModelBuilder ApplyAlertManagementConfiguration(this ModelBuilder builder)
    {
        // Alert Aggregate
        builder.Entity<Alert>().ToTable("Alerts");
        builder.Entity<Alert>().HasKey(a => a.Id);
        builder.Entity<Alert>().Property(a => a.Id).ValueGeneratedNever();
        builder.Entity<Alert>().Property(a => a.OrganizationId).IsRequired();
        builder.Entity<Alert>().Property(a => a.GeneratedAt).HasPrecision(6).IsRequired();

        // Embedded Pattern for AlertContext
        builder.Entity<Alert>().OwnsOne(a => a.Context, context =>
        {
            context.Property(c => c.RiskDetectionId).HasColumnName("RiskDetectionId").HasMaxLength(64).IsRequired();
            context.Property(c => c.RiskTypeCode).HasColumnName("RiskTypeCode").HasMaxLength(50).IsRequired();
            context.Property(c => c.SeverityCode).HasColumnName("SeverityCode").HasMaxLength(20).IsRequired();
            context.Property(c => c.BuildingId).HasColumnName("BuildingId");
            context.Property(c => c.ZoneId).HasColumnName("ZoneId");
            context.Property(c => c.DetectedAt).HasColumnName("DetectedAt").HasPrecision(6).IsRequired();
        });
        builder.Entity<Alert>().Navigation(a => a.Context).IsRequired();

        // NotificationDelivery belongs to the Alert Aggregate
        builder.Entity<NotificationDelivery>().ToTable("NotificationDeliveries");
        builder.Entity<NotificationDelivery>().HasKey(d => d.Id);
        builder.Entity<NotificationDelivery>().Property(d => d.Id).ValueGeneratedNever();
        builder.Entity<NotificationDelivery>().Property<Guid>("AlertId").HasColumnName("alert_id");
        builder.Entity<NotificationDelivery>().Property(d => d.RecipientUserId).HasMaxLength(64).IsRequired();
        builder.Entity<NotificationDelivery>().Property(d => d.Channel).HasMaxLength(30).IsRequired();
        builder.Entity<NotificationDelivery>().Property(d => d.Destination).HasMaxLength(200).IsRequired();
        builder.Entity<NotificationDelivery>().Property(d => d.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Entity<NotificationDelivery>().Property(d => d.RequestedAt).HasPrecision(6).IsRequired();
        builder.Entity<NotificationDelivery>().Property(d => d.CompletedAt).HasPrecision(6);
        builder.Entity<NotificationDelivery>().Property(d => d.FailureReason).HasMaxLength(500);
        builder.Entity<Alert>().HasMany(a => a.Deliveries).WithOne().HasForeignKey("AlertId").OnDelete(DeleteBehavior.Cascade);

        // ResponseExecution Aggregate
        builder.Entity<ResponseExecution>().ToTable("ResponseExecutions");
        builder.Entity<ResponseExecution>().HasKey(e => e.Id);
        builder.Entity<ResponseExecution>().Property(e => e.Id).ValueGeneratedNever();
        builder.Entity<ResponseExecution>().Property(e => e.OrganizationId).IsRequired();
        builder.Entity<ResponseExecution>().Property(e => e.AlertId).IsRequired();
        builder.Entity<ResponseExecution>().Property(e => e.RiskDetectionId).HasMaxLength(64).IsRequired();
        builder.Entity<ResponseExecution>().Property(e => e.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Entity<ResponseExecution>().Property(e => e.RequestedAt).HasPrecision(6).IsRequired();
        builder.Entity<ResponseExecution>().HasIndex(e => new { e.OrganizationId, e.AlertId });

        // Embedded Pattern for ResponseActionSnapshot
        builder.Entity<ResponseExecution>().OwnsOne(e => e.Action, action =>
        {
            action.Property(a => a.ActionId).HasColumnName("ActionId").IsRequired();
            action.Property(a => a.ActionCode).HasColumnName("ActionCode").HasMaxLength(80).IsRequired();
            action.Property(a => a.TargetDeviceId).HasColumnName("TargetDeviceId").IsRequired();
            action.Property(a => a.TargetCapabilityCode).HasColumnName("TargetCapabilityCode").HasMaxLength(80).IsRequired();
            action.Property(a => a.AuthorizationMode).HasColumnName("AuthorizationMode").HasConversion<string>().HasMaxLength(20).IsRequired();
            action.Property(a => a.Critical).HasColumnName("Critical").IsRequired();
        });
        builder.Entity<ResponseExecution>().Navigation(e => e.Action).IsRequired();

        // Embedded Pattern for the optional ResponseAuthorization
        builder.Entity<ResponseExecution>().OwnsOne(e => e.Authorization, authorization =>
        {
            authorization.Property(a => a.AuthorizationId).HasColumnName("AuthorizationId").IsRequired();
            authorization.Property(a => a.Decision).HasColumnName("AuthorizationDecision").HasConversion<string>().HasMaxLength(16).IsRequired();
            authorization.Property(a => a.DecidedByUserId).HasColumnName("DecidedByUserId").HasMaxLength(64).IsRequired();
            authorization.Property(a => a.DecidedAt).HasColumnName("DecidedAt").HasPrecision(6).IsRequired();
        });

        return builder;
    }
}

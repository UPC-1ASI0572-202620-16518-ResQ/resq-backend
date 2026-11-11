using Microsoft.EntityFrameworkCore;
using ResQ.API.Risk_Detection.Domain.Model.Aggregates;
using ResQ.API.Risk_Detection.Domain.Model.Entities;

namespace ResQ.API.Risk_Detection.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring the Risk Detection persistence model.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies the Risk Detection entity configurations.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    public static ModelBuilder ApplyRiskDetectionConfiguration(this ModelBuilder builder)
    {
        // DetectionRule Aggregate
        builder.Entity<DetectionRule>().ToTable("DetectionRules");
        builder.Entity<DetectionRule>().HasKey(r => r.Id);
        builder.Entity<DetectionRule>().Property(r => r.Id).ValueGeneratedNever();
        builder.Entity<DetectionRule>().Property(r => r.OrganizationId).IsRequired();
        builder.Entity<DetectionRule>().Property(r => r.RiskTypeCode).HasMaxLength(50).IsRequired();
        builder.Entity<DetectionRule>().Property(r => r.SeverityCode).HasMaxLength(20).IsRequired();
        builder.Entity<DetectionRule>().Property(r => r.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Entity<DetectionRule>().Property(r => r.ConfiguredAt).HasPrecision(6).IsRequired();

        // Embedded Pattern for DetectionCondition
        builder.Entity<DetectionRule>().OwnsOne(r => r.Condition, condition =>
        {
            condition.Property(c => c.VariableType).HasColumnName("VariableType").HasMaxLength(50).IsRequired();
            condition.Property(c => c.Operator).HasColumnName("Operator").HasConversion<string>().HasMaxLength(30).IsRequired();
            condition.Property(c => c.Threshold).HasColumnName("Threshold").HasPrecision(18, 4).IsRequired();
        });
        builder.Entity<DetectionRule>().Navigation(r => r.Condition).IsRequired();

        // RiskDetection Aggregate
        builder.Entity<RiskDetection>().ToTable("RiskDetections");
        builder.Entity<RiskDetection>().HasKey(d => d.Id);
        builder.Entity<RiskDetection>().Property(d => d.Id).ValueGeneratedNever();
        builder.Entity<RiskDetection>().Property(d => d.OrganizationId).IsRequired();
        builder.Entity<RiskDetection>().Property(d => d.RuleId).IsRequired();
        builder.Entity<RiskDetection>().Property(d => d.RiskTypeCode).HasMaxLength(50).IsRequired();
        builder.Entity<RiskDetection>().Property(d => d.CurrentSeverityCode).HasMaxLength(20).IsRequired();
        builder.Entity<RiskDetection>().Property(d => d.DetectedAt).HasPrecision(6).IsRequired();
        builder.Entity<RiskDetection>().Property(d => d.LastEvidenceAt).HasPrecision(6).IsRequired();
        builder.Entity<RiskDetection>().Ignore(d => d.DomainEvents);
        builder.Entity<RiskDetection>().HasIndex(d => new { d.OrganizationId, d.RiskTypeCode });

        // Embedded Pattern for RiskLocation
        builder.Entity<RiskDetection>().OwnsOne(d => d.Location, location =>
        {
            location.Property(l => l.BuildingId).HasColumnName("BuildingId");
            location.Property(l => l.ZoneId).HasColumnName("ZoneId");
            location.Property(l => l.ResolutionStatus).HasColumnName("LocationResolutionStatus")
                .HasConversion<string>().HasMaxLength(16).IsRequired();
        });
        builder.Entity<RiskDetection>().Navigation(d => d.Location).IsRequired();

        // DetectionEvidence belongs to the RiskDetection Aggregate
        builder.Entity<DetectionEvidence>().ToTable("DetectionEvidences");
        builder.Entity<DetectionEvidence>().HasKey(e => e.Id);
        builder.Entity<DetectionEvidence>().Property(e => e.Id).ValueGeneratedNever();
        builder.Entity<DetectionEvidence>().Property<Guid>("RiskDetectionId").HasColumnName("risk_detection_id");
        builder.Entity<DetectionEvidence>().Property(e => e.MeasurementId).IsRequired();
        builder.Entity<DetectionEvidence>().Property(e => e.DeviceId).IsRequired();
        builder.Entity<DetectionEvidence>().Property(e => e.VariableType).HasMaxLength(50).IsRequired();
        builder.Entity<DetectionEvidence>().Property(e => e.Value).HasPrecision(18, 4).IsRequired();
        builder.Entity<DetectionEvidence>().Property(e => e.MeasuredAt).HasPrecision(6).IsRequired();
        builder.Entity<RiskDetection>().HasMany(d => d.Evidence).WithOne()
            .HasForeignKey("RiskDetectionId").OnDelete(DeleteBehavior.Cascade);

        // SeverityChange belongs to the RiskDetection Aggregate
        builder.Entity<SeverityChange>().ToTable("SeverityChanges");
        builder.Entity<SeverityChange>().HasKey(c => c.Id);
        builder.Entity<SeverityChange>().Property(c => c.Id).ValueGeneratedNever();
        builder.Entity<SeverityChange>().Property<Guid>("RiskDetectionId").HasColumnName("risk_detection_id");
        builder.Entity<SeverityChange>().Property(c => c.PreviousSeverityCode).HasMaxLength(20).IsRequired();
        builder.Entity<SeverityChange>().Property(c => c.NewSeverityCode).HasMaxLength(20).IsRequired();
        builder.Entity<SeverityChange>().Property(c => c.ChangedAt).HasPrecision(6).IsRequired();
        builder.Entity<RiskDetection>().HasMany(d => d.SeverityChanges).WithOne()
            .HasForeignKey("RiskDetectionId").OnDelete(DeleteBehavior.Cascade);

        return builder;
    }
}

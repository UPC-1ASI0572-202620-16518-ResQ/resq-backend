using Microsoft.EntityFrameworkCore;
using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Entities;
using ResQ.API.Building_Management.Domain.Model.ValueObjects;

namespace ResQ.API.Building_Management.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring the Building Management persistence model.
/// </summary>
public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplyBuildingManagementConfiguration(this ModelBuilder builder)
    {
        // ── Building ──
        builder.Entity<Building>().ToTable("Buildings");
        builder.Entity<Building>().HasKey(b => b.Id);
        builder.Entity<Building>().Property(b => b.Id).ValueGeneratedNever();
        builder.Entity<Building>().Property(b => b.OrganizationId).IsRequired();

        // BuildingCode value object
        builder.Entity<Building>()
            .Property(b => b.BuildingCode)
            .HasConversion(
                code => code.Value,
                value => BuildingCode.Create(value))
            .HasColumnName("BuildingCode")
            .HasMaxLength(64)
            .IsRequired();

        builder.Entity<Building>().Property(b => b.Name).HasMaxLength(120).IsRequired();
        builder.Entity<Building>().Property(b => b.Description).HasMaxLength(500);

        // BuildingAddress owned value object
        builder.Entity<Building>().OwnsOne(b => b.Address, address =>
        {
            address.Property(a => a.StreetAddress).HasColumnName("StreetAddress").HasMaxLength(200).IsRequired();
            address.Property(a => a.District).HasColumnName("District").HasMaxLength(100).IsRequired();
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(100).IsRequired();
            address.Property(a => a.CountryCode).HasColumnName("CountryCode").HasMaxLength(2).IsRequired();
        });

        builder.Entity<Building>()
            .Property(b => b.AdministrativeStatus)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Entity<Building>().Property(b => b.CreatedAt).IsRequired();
        builder.Entity<Building>().Property(b => b.UpdatedAt).IsRequired();
        builder.Entity<Building>().Property(b => b.Version).IsRequired();

        // UQ1: (organization_id, building_code)
        builder.Entity<Building>()
            .HasIndex(b => new { b.OrganizationId, b.BuildingCode })
            .IsUnique();

        // ── Zone ──
        builder.Entity<Zone>().ToTable("Zones");
        builder.Entity<Zone>().HasKey(z => z.Id);
        builder.Entity<Zone>().Property(z => z.Id).ValueGeneratedNever();

        // ZoneCode value object
        builder.Entity<Zone>()
            .Property(z => z.ZoneCode)
            .HasConversion(
                code => code.Value,
                value => ZoneCode.Create(value))
            .HasColumnName("ZoneCode")
            .HasMaxLength(64)
            .IsRequired();

        builder.Entity<Zone>().Property(z => z.Name).HasMaxLength(120).IsRequired();
        builder.Entity<Zone>().Property(z => z.Description).HasMaxLength(500);
        builder.Entity<Zone>().Property(z => z.FloorLabel).HasMaxLength(50);

        builder.Entity<Zone>()
            .Property(z => z.AdministrativeStatus)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Entity<Zone>().Property(z => z.CreatedAt).IsRequired();
        builder.Entity<Zone>().Property(z => z.UpdatedAt).IsRequired();

        // Zone properties
        builder.Entity<Zone>().Property(z => z.BuildingId).HasColumnName("building_id").IsRequired();

        // Building -> Zones relationship
        builder.Entity<Building>()
            .HasMany(b => b.Zones)
            .WithOne()
            .HasForeignKey(z => z.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        // UQ2: (building_id, zone_code)
        builder.Entity<Zone>()
            .HasIndex(z => new { z.BuildingId, z.ZoneCode })
            .IsUnique();

        return builder;
    }
}

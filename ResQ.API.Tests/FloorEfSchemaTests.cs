using Microsoft.EntityFrameworkCore;
using ResQ.API.Building_Management.Domain.Model.Aggregates;
using ResQ.API.Building_Management.Domain.Model.Entities;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Xunit;

namespace ResQ.API.Tests;

public sealed class FloorEfSchemaTests
{
    [Fact]
    public void FloorRelationalModelMatchesIncrementalSchemaContract()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySQL("server=localhost;database=model_only;user=model;password=model")
            .Options;
        using var context = new AppDbContext(options);
        var relational = context.Model.GetRelationalModel();
        var table = Assert.Single(relational.Tables, table => table.Name == "floors");

        Assert.Equal(6, table.Columns.Count());
        AssertColumn(table, "id", "char(36)", false);
        AssertColumn(table, "building_id", "char(36)", false);
        AssertColumn(table, "label", "varchar(50)", false);
        AssertColumn(table, "level", "int", false);
        AssertColumn(table, "created_at", "datetime(6)", false);
        AssertColumn(table, "updated_at", "datetime(6)", false);

        var entity = context.Model.FindEntityType(typeof(Floor))!;
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique && index.GetDatabaseName() == "ix_floors_building_id_label");
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique && index.GetDatabaseName() == "ix_floors_building_id_level");
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.DeleteBehavior == DeleteBehavior.Cascade &&
            foreignKey.PrincipalEntityType.ClrType.Name == "Building");

        var layouts = Assert.Single(relational.Tables, table => table.Name == "floor_plan_layouts");
        Assert.Equal(8, layouts.Columns.Count());
        AssertColumn(layouts, "id", "char(36)", false);
        AssertColumn(layouts, "organization_id", "char(36)", false);
        AssertColumn(layouts, "building_id", "char(36)", false);
        AssertColumn(layouts, "floor_label", "varchar(50)", false);
        AssertColumn(layouts, "layout_json", "longtext", false);
        AssertColumn(layouts, "updated_by_user_id", "int", false);
        AssertColumn(layouts, "updated_at", "datetime(6)", false);
        AssertColumn(layouts, "version", "bigint", false);
        Assert.True(context.Model.FindEntityType(typeof(FloorPlanLayout))!
            .FindProperty(nameof(FloorPlanLayout.Version))!.IsConcurrencyToken);
    }

    [Theory]
    [InlineData("utf8mb4", "utf8mb4_unicode_ci")]
    [InlineData("ascii", "ascii_general_ci")]
    public void IncrementalFloorTableUsesExistingBuildingKeyEncoding(string characterSet, string collation)
    {
        var sql = FloorSchemaDefinition.CreateTableSql(characterSet, collation);

        Assert.Contains($"`building_id` char(36) CHARACTER SET {characterSet} COLLATE {collation} NOT NULL", sql);
        Assert.Contains("FOREIGN KEY (`building_id`) REFERENCES `buildings` (`id`)", sql);
        Assert.Contains("CREATE TABLE IF NOT EXISTS `floors`", sql);
    }

    [Fact]
    public void IncrementalFloorTableRejectsUnsafeSqlIdentifiers()
    {
        Assert.Throws<ArgumentException>(() =>
            FloorSchemaDefinition.CreateTableSql("utf8mb4", "utf8mb4_unicode_ci; DROP TABLE buildings"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public void LegacyFloorPlanUpdatedAtGetsSafePrecisionUpgrade(int existingPrecision)
    {
        var sql = FloorSchemaDefinition.GetLayoutTimestampPrecisionUpgradeSql(existingPrecision);

        Assert.Equal(
            "ALTER TABLE `floor_plan_layouts` MODIFY COLUMN `updated_at` datetime(6) NOT NULL;",
            sql);
    }

    [Fact]
    public void FloorPlanWithCurrentTimestampPrecisionIsNotAltered()
    {
        Assert.Null(FloorSchemaDefinition.GetLayoutTimestampPrecisionUpgradeSql(6));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(null)]
    public void FloorPlanWithUnexpectedTimestampPrecisionIsRejected(int? precision)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FloorSchemaDefinition.GetLayoutTimestampPrecisionUpgradeSql(precision));
    }

    private static void AssertColumn(
        Microsoft.EntityFrameworkCore.Metadata.ITable table,
        string name,
        string storeType,
        bool nullable)
    {
        var column = table.Columns.SingleOrDefault(column => column.Name == name);
        Assert.True(column is not null,
            $"Column `{name}` was not found. Actual columns: {string.Join(", ", table.Columns.Select(item => item.Name))}");
        Assert.Equal(storeType, column!.StoreType, ignoreCase: true);
        Assert.Equal(nullable, column.IsNullable);
    }
}

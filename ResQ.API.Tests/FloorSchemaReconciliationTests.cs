using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using Xunit;

namespace ResQ.API.Tests;

public sealed class FloorSchemaReconciliationTests
{
    [Fact]
    public void PlansMissingFloorsFromZonesAndLayoutsWithoutDuplicates()
    {
        var existing = new[]
        {
            new ExistingFloorSchemaRow("building-1", "Ground", 0),
            new ExistingFloorSchemaRow("building-1", "Floor 2", 2),
        };
        var legacy = new[]
        {
            new LegacyFloorSchemaSource("building-1", "Ground"),
            new LegacyFloorSchemaSource("building-1", "Floor 1"),
            new LegacyFloorSchemaSource("building-1", "floor 1"),
            new LegacyFloorSchemaSource("building-1", "Floor 3"),
            new LegacyFloorSchemaSource("building-2", "Basement"),
        };

        var planned = FloorSchemaReconciliation.Plan(existing, legacy);

        Assert.Equal(
            new[]
            {
                new MissingFloorSchemaRow("building-1", "Floor 1", 3),
                new MissingFloorSchemaRow("building-1", "Floor 3", 4),
                new MissingFloorSchemaRow("building-2", "Basement", 0),
            },
            planned);
    }

    [Fact]
    public void ReconciliationIsIdempotentAfterPlannedRowsArePersisted()
    {
        var existing = new[] { new ExistingFloorSchemaRow("building-1", "Ground", 0) };
        var sources = new[]
        {
            new LegacyFloorSchemaSource("building-1", "Ground"),
            new LegacyFloorSchemaSource("building-1", "Floor 1"),
        };
        var first = FloorSchemaReconciliation.Plan(existing, sources);
        var persisted = existing.Concat(first.Select(row =>
            new ExistingFloorSchemaRow(row.BuildingId, row.Label, row.Level)));

        var second = FloorSchemaReconciliation.Plan(persisted, sources);

        Assert.Empty(second);
    }
}

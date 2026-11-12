namespace ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

public sealed record ExistingFloorSchemaRow(string BuildingId, string Label, int Level);
public sealed record LegacyFloorSchemaSource(string BuildingId, string Label);
public sealed record MissingFloorSchemaRow(string BuildingId, string Label, int Level);

/// <summary>
/// Produces deterministic, non-destructive floor rows from legacy zone/layout labels.
/// Existing labels are preserved and new levels are appended after the current maximum.
/// </summary>
public static class FloorSchemaReconciliation
{
    public static IReadOnlyList<MissingFloorSchemaRow> Plan(
        IEnumerable<ExistingFloorSchemaRow> existing,
        IEnumerable<LegacyFloorSchemaSource> sources)
    {
        var rows = existing.ToList();
        var labelsByBuilding = rows
            .GroupBy(row => row.BuildingId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => new HashSet<string>(group.Select(row => row.Label.Trim()), StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        var nextLevelByBuilding = rows
            .GroupBy(row => row.BuildingId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Select(row => row.Level).DefaultIfEmpty(-1).Max() + 1,
                StringComparer.OrdinalIgnoreCase);

        var result = new List<MissingFloorSchemaRow>();
        foreach (var source in sources
                     .Where(source => !string.IsNullOrWhiteSpace(source.BuildingId) && !string.IsNullOrWhiteSpace(source.Label))
                     .Select(source => new LegacyFloorSchemaSource(source.BuildingId.Trim(), source.Label.Trim()))
                     .DistinctBy(source => $"{source.BuildingId}\0{source.Label}", StringComparer.OrdinalIgnoreCase)
                     .OrderBy(source => source.BuildingId, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(source => source.Label, StringComparer.OrdinalIgnoreCase))
        {
            if (!labelsByBuilding.TryGetValue(source.BuildingId, out var labels))
            {
                labels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                labelsByBuilding[source.BuildingId] = labels;
            }
            if (!labels.Add(source.Label)) continue;

            var level = nextLevelByBuilding.GetValueOrDefault(source.BuildingId, 0);
            result.Add(new MissingFloorSchemaRow(source.BuildingId, source.Label, level));
            nextLevelByBuilding[source.BuildingId] = level + 1;
        }

        return result;
    }
}

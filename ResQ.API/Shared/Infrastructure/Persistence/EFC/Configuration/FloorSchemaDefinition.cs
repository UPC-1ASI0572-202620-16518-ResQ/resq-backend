using System.Text.RegularExpressions;

namespace ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Creates a missing floors table using the exact charset/collation of buildings.Id.
/// Different existing installations may have been created by EF or legacy SQL.
/// </summary>
public static class FloorSchemaDefinition
{
    public static string CreateTableSql(string buildingIdCharacterSet, string buildingIdCollation)
    {
        // These values originate in information_schema but are SQL identifiers,
        // not bindable values. Whitelist identifier characters before interpolation.
        if (!IsSafeIdentifier(buildingIdCharacterSet) || !IsSafeIdentifier(buildingIdCollation))
            throw new ArgumentException("Invalid building ID character set or collation.");

        return $@"CREATE TABLE IF NOT EXISTS `floors` (
    `id` char(36) CHARACTER SET {buildingIdCharacterSet} COLLATE {buildingIdCollation} NOT NULL,
    `building_id` char(36) CHARACTER SET {buildingIdCharacterSet} COLLATE {buildingIdCollation} NOT NULL,
    `label` varchar(50) NOT NULL,
    `level` int NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ix_floors_building_id_label` (`building_id`, `label`),
    UNIQUE KEY `ix_floors_building_id_level` (`building_id`, `level`),
    CONSTRAINT `fk_floors_buildings_building_id`
        FOREIGN KEY (`building_id`) REFERENCES `buildings` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    }

    /// <summary>
    /// Upgrades legacy floor plan timestamps (datetime with 0-5 fractional digits)
    /// to the current EF datetime(6) mapping. Returns null when already up to date.
    /// </summary>
    public static string? GetLayoutTimestampPrecisionUpgradeSql(int? existingPrecision)
    {
        if (existingPrecision is null or < 0 or > 6)
            throw new ArgumentOutOfRangeException(nameof(existingPrecision), existingPrecision,
                "Expected a MySQL datetime precision between 0 and 6.");

        return existingPrecision == 6
            ? null
            : "ALTER TABLE `floor_plan_layouts` MODIFY COLUMN `updated_at` datetime(6) NOT NULL;";
    }

    private static bool IsSafeIdentifier(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= 64 &&
        Regex.IsMatch(value, @"\A[A-Za-z0-9_]+\z");
}

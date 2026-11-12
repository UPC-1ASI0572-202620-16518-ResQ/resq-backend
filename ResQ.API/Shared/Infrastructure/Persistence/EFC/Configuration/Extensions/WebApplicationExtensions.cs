using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data;
using ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class WebApplicationExtensions
{
    public static void UseDatabaseCreationAssurance(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            // Creates a brand-new database only. Every upgrade for an existing database is
            // applied explicitly below; EnsureCreated does not evolve existing schemas.
            context.Database.EnsureCreated();
        }
        catch (Exception exception)
        {
            app.Logger.LogCritical(exception, "Database creation assurance could not connect or initialize the database.");
            throw;
        }

        EnsureTableExists(context, "buildings", @"
CREATE TABLE IF NOT EXISTS `buildings` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `organization_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `building_code` varchar(64) NOT NULL,
    `name` varchar(120) NOT NULL,
    `description` varchar(500) NULL,
    `street_address` varchar(200) NOT NULL,
    `district` varchar(100) NOT NULL,
    `city` varchar(100) NOT NULL,
    `country_code` varchar(2) NOT NULL,
    `administrative_status` varchar(16) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    `version` bigint NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ix_buildings_organization_id_building_code` (`organization_id`, `building_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "zones", @"
CREATE TABLE IF NOT EXISTS `zones` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `building_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `zone_code` varchar(64) NOT NULL,
    `name` varchar(120) NOT NULL,
    `description` varchar(500) NULL,
    `floor_label` varchar(50) NULL,
    `administrative_status` varchar(16) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ix_zones_building_id_zone_code` (`building_id`, `zone_code`),
    CONSTRAINT `fk_zones_buildings_building_id` FOREIGN KEY (`building_id`) REFERENCES `buildings` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        // Existing installations can use a different collation for buildings.Id than
        // databases created from the EF model. A textual MySQL FK must match the
        // referenced key's character set and collation exactly.
        var buildingIdColumn = ReadBuildingIdColumn(context);
        EnsureTableExists(context, "floors", FloorSchemaDefinition.CreateTableSql(
            buildingIdColumn.CharacterSet!, buildingIdColumn.Collation!));

        EnsureIndexExists(context, "floors", "ix_floors_building_id_label",
            "CREATE UNIQUE INDEX `ix_floors_building_id_label` ON `floors` (`building_id`, `label`);");
        EnsureIndexExists(context, "floors", "ix_floors_building_id_level",
            "CREATE UNIQUE INDEX `ix_floors_building_id_level` ON `floors` (`building_id`, `level`);");
        EnsureForeignKeyExists(context, "floors", "fk_floors_buildings_building_id",
            "ALTER TABLE `floors` ADD CONSTRAINT `fk_floors_buildings_building_id` " +
            "FOREIGN KEY (`building_id`) REFERENCES `buildings` (`id`) ON DELETE CASCADE;");
        ValidateFloorSchema(context, buildingIdColumn);

        EnsureTableExists(context, "devices", @"
CREATE TABLE IF NOT EXISTS `devices` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `organization_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `device_code` varchar(64) NOT NULL,
    `name` varchar(120) NOT NULL,
    `description` varchar(500) NULL,
    `administrative_status` varchar(20) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    `version` bigint NOT NULL,
    `manufacturer` varchar(100) NULL,
    `model` varchar(100) NULL,
    `serial_number` varchar(100) NULL,
    `building_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `zone_id` char(36) COLLATE ascii_general_ci NULL,
    `external_source_system` varchar(80) NULL,
    `external_device_id` varchar(120) NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ix_devices_organization_id_device_code` (`organization_id`, `device_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "device_capabilities", @"
CREATE TABLE IF NOT EXISTS `device_capabilities` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `device_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `code` varchar(80) NOT NULL,
    `kind` varchar(20) NOT NULL,
    `unit` varchar(30) NULL,
    PRIMARY KEY (`id`),
    CONSTRAINT `fk_device_capabilities_devices_device_id` FOREIGN KEY (`device_id`) REFERENCES `devices` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "detection_rules", @"
CREATE TABLE IF NOT EXISTS `detection_rules` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `risk_type_code` varchar(50) NOT NULL,
    `severity_code` varchar(20) NOT NULL,
    `variable_type` varchar(50) NOT NULL,
    `operator` varchar(30) NOT NULL,
    `threshold` decimal(18,4) NOT NULL,
    `status` varchar(16) NOT NULL,
    `configured_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_detection_rules_organization_id_variable_type` (`organization_id`, `variable_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "risk_detections", @"
CREATE TABLE IF NOT EXISTS `risk_detections` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `rule_id` char(36) NOT NULL,
    `risk_type_code` varchar(50) NOT NULL,
    `current_severity_code` varchar(20) NOT NULL,
    `building_id` char(36) NULL,
    `zone_id` char(36) NULL,
    `location_resolution_status` varchar(16) NOT NULL,
    `detected_at` datetime(6) NOT NULL,
    `last_evidence_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_risk_detections_organization_id_risk_type_code` (`organization_id`, `risk_type_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "detection_evidences", @"
CREATE TABLE IF NOT EXISTS `detection_evidences` (
    `id` char(36) NOT NULL,
    `risk_detection_id` char(36) NOT NULL,
    `measurement_id` char(36) NOT NULL,
    `device_id` char(36) NOT NULL,
    `variable_type` varchar(50) NOT NULL,
    `value` decimal(18,4) NOT NULL,
    `measured_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_detection_evidences_risk_detection_id` (`risk_detection_id`),
    CONSTRAINT `fk_detection_evidences_risk_detections_risk_detection_id` FOREIGN KEY (`risk_detection_id`) REFERENCES `risk_detections` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "severity_changes", @"
CREATE TABLE IF NOT EXISTS `severity_changes` (
    `id` char(36) NOT NULL,
    `risk_detection_id` char(36) NOT NULL,
    `previous_severity_code` varchar(20) NOT NULL,
    `new_severity_code` varchar(20) NOT NULL,
    `changed_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_severity_changes_risk_detection_id` (`risk_detection_id`),
    CONSTRAINT `fk_severity_changes_risk_detections_risk_detection_id` FOREIGN KEY (`risk_detection_id`) REFERENCES `risk_detections` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "alerts", @"
CREATE TABLE IF NOT EXISTS `alerts` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `risk_detection_id` varchar(64) NOT NULL,
    `risk_type_code` varchar(50) NOT NULL,
    `severity_code` varchar(20) NOT NULL,
    `building_id` char(36) NULL,
    `zone_id` char(36) NULL,
    `detected_at` datetime(6) NOT NULL,
    `generated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "notification_deliveries", @"
CREATE TABLE IF NOT EXISTS `notification_deliveries` (
    `id` char(36) NOT NULL,
    `alert_id` char(36) NOT NULL,
    `recipient_user_id` varchar(64) NOT NULL,
    `channel` varchar(30) NOT NULL,
    `destination` varchar(200) NOT NULL,
    `status` varchar(20) NOT NULL,
    `requested_at` datetime(6) NOT NULL,
    `completed_at` datetime(6) NULL,
    `failure_reason` varchar(500) NULL,
    PRIMARY KEY (`id`),
    KEY `ix_notification_deliveries_alert_id` (`alert_id`),
    CONSTRAINT `fk_notification_deliveries_alerts_alert_id` FOREIGN KEY (`alert_id`) REFERENCES `alerts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "response_executions", @"
CREATE TABLE IF NOT EXISTS `response_executions` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `alert_id` char(36) NOT NULL,
    `risk_detection_id` varchar(64) NOT NULL,
    `action_id` char(36) NOT NULL,
    `action_code` varchar(80) NOT NULL,
    `target_device_id` char(36) NOT NULL,
    `target_capability_code` varchar(80) NOT NULL,
    `authorization_mode` varchar(20) NOT NULL,
    `critical` tinyint(1) NOT NULL,
    `status` varchar(30) NOT NULL,
    `requested_at` datetime(6) NOT NULL,
    `authorization_id` char(36) NULL,
    `authorization_decision` varchar(16) NULL,
    `decided_by_user_id` varchar(64) NULL,
    `decided_at` datetime(6) NULL,
    PRIMARY KEY (`id`),
    KEY `ix_response_executions_organization_id_alert_id` (`organization_id`, `alert_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "sensor_connections", @"
CREATE TABLE IF NOT EXISTS `sensor_connections` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `sensor_id` char(36) NOT NULL,
    `status` varchar(16) NOT NULL,
    `last_heartbeat` datetime(6) NOT NULL,
    `signal_strength` int NULL,
    `status_changed_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ix_sensor_connections_organization_id_sensor_id` (`organization_id`, `sensor_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "heartbeat_records", @"
CREATE TABLE IF NOT EXISTS `heartbeat_records` (
    `id` char(36) NOT NULL,
    `sensor_connection_id` char(36) NOT NULL,
    `timestamp` datetime(6) NOT NULL,
    `signal_strength` int NULL,
    PRIMARY KEY (`id`),
    KEY `ix_heartbeat_records_sensor_connection_id` (`sensor_connection_id`),
    CONSTRAINT `fk_heartbeat_records_sensor_connections_sensor_connection_id` FOREIGN KEY (`sensor_connection_id`) REFERENCES `sensor_connections` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "measurements", @"
CREATE TABLE IF NOT EXISTS `measurements` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `organization_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `device_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `building_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `zone_id` char(36) COLLATE ascii_general_ci NULL,
    `variable_type` varchar(50) NOT NULL,
    `value` decimal(18,4) NOT NULL,
    `unit` varchar(30) NOT NULL,
    `measured_at` datetime(6) NOT NULL,
    `recorded_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_measurements_organization_id_device_id_measured_at` (`organization_id`, `device_id`, `measured_at`),
    KEY `ix_measurements_organization_id_zone_id_measured_at` (`organization_id`, `zone_id`, `measured_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "device_monitoring_states", @"
CREATE TABLE IF NOT EXISTS `device_monitoring_states` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `organization_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `device_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `zone_id` char(36) COLLATE ascii_general_ci NULL,
    `availability` varchar(20) NOT NULL,
    `last_measurement_at` datetime(6) NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ix_device_monitoring_states_organization_id_device_id` (`organization_id`, `device_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "zone_monitoring_states", @"
CREATE TABLE IF NOT EXISTS `zone_monitoring_states` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `organization_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `zone_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `building_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `last_updated_at` datetime(6) NOT NULL,
    `active_risk_id` char(36) COLLATE ascii_general_ci NULL,
    `condition_code` varchar(50) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ix_zone_monitoring_states_organization_id_zone_id` (`organization_id`, `zone_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureColumnExists(context, "incidents", "organization_id", "char(36) NULL");
        EnsureColumnExists(context, "incidents", "risk_detection_id", "char(36) NULL");
        EnsureColumnExists(context, "incidents", "alert_id", "char(36) NULL");
        EnsureColumnExists(context, "incidents", "assigned_user_id", "int NULL");
        EnsureColumnExists(context, "incidents", "assigned_at", "datetime(6) NULL");
        EnsureColumnExists(context, "incidents", "resolved_by_user_id", "int NULL");
        EnsureColumnExists(context, "incidents", "closed_at", "datetime(6) NULL");
        EnsureColumnExists(context, "incidents", "closed_by_user_id", "int NULL");
        EnsureColumnExists(context, "detection_rules", "zone_id", "char(36) NULL");

        var developmentOrganizationId = app.Configuration["DevelopmentSettings:OrganizationId"];
        if (Guid.TryParse(developmentOrganizationId, out _))
        {
            ExecuteNonQuery(context,
                $"UPDATE `incidents` SET `organization_id` = '{developmentOrganizationId}' " +
                "WHERE `organization_id` IS NULL OR `organization_id` = '00000000-0000-0000-0000-000000000000';");
            ExecuteNonQuery(context, "ALTER TABLE `incidents` MODIFY `organization_id` char(36) NOT NULL;");
        }

        EnsureIndexExists(context, "incidents", "ux_incidents_organization_risk_detection",
            "CREATE UNIQUE INDEX `ux_incidents_organization_risk_detection` ON `incidents` (`organization_id`, `risk_detection_id`);");

        EnsureTableExists(context, "incident_human_actions", @"
CREATE TABLE IF NOT EXISTS `incident_human_actions` (
    `id` char(36) NOT NULL,
    `incident_id` char(36) NOT NULL,
    `user_id` int NOT NULL,
    `action_code` varchar(80) NOT NULL,
    `notes` varchar(1000) NULL,
    `performed_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_incident_human_actions_incident_performed` (`incident_id`, `performed_at`),
    CONSTRAINT `fk_incident_human_actions_incident_id` FOREIGN KEY (`incident_id`) REFERENCES `incidents` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "incident_safety_verifications", @"
CREATE TABLE IF NOT EXISTS `incident_safety_verifications` (
    `id` char(36) NOT NULL,
    `incident_id` char(36) NOT NULL,
    `verified_by_user_id` int NOT NULL,
    `safe` tinyint(1) NOT NULL,
    `reason` varchar(1000) NOT NULL,
    `device_id` char(36) NULL,
    `measurement_id` char(36) NULL,
    `measured_value` decimal(18,4) NULL,
    `measured_at` datetime(6) NULL,
    `rule_snapshot` varchar(2000) NULL,
    `verified_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_incident_safety_verifications_incident_verified` (`incident_id`, `verified_at`),
    CONSTRAINT `fk_incident_safety_verifications_incident_id` FOREIGN KEY (`incident_id`) REFERENCES `incidents` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "floor_plan_layouts", @"
CREATE TABLE IF NOT EXISTS `floor_plan_layouts` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `building_id` char(36) NOT NULL,
    `floor_label` varchar(50) NOT NULL,
    `layout_json` longtext NOT NULL,
    `updated_by_user_id` int NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    `version` bigint NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_floor_plan_layouts_scope` (`organization_id`, `building_id`, `floor_label`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureColumnExists(context, "floor_plan_layouts", "version", "bigint NOT NULL DEFAULT 1");
        UpgradeLegacyFloorPlanUpdatedAtPrecision(context);
        ValidateFloorPlanLayoutSchema(context);
        ReconcileFloors(context);

        EnsureTableExists(context, "response_policies", @"
CREATE TABLE IF NOT EXISTS `response_policies` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `zone_id` char(36) NULL,
    `risk_type_code` varchar(50) NOT NULL,
    `severity_code` varchar(20) NOT NULL,
    `actions_json` longtext NOT NULL,
    `active` tinyint(1) NOT NULL,
    `updated_by_user_id` int NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    PRIMARY KEY (`id`),
    UNIQUE KEY `ux_response_policies_scope` (`organization_id`, `zone_id`, `risk_type_code`, `severity_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
    }

    private static DatabaseColumn ReadBuildingIdColumn(AppDbContext context)
    {
        var columns = ReadColumns(context, "buildings");
        RequireColumn(context, columns, "id", "char", 36, false);
        var id = columns["id"];
        if (string.IsNullOrWhiteSpace(id.CharacterSet) || string.IsNullOrWhiteSpace(id.Collation))
            FailSchemaValidation(context, "`buildings.Id` has no usable character set/collation.");
        return id;
    }

    private static void ValidateFloorSchema(AppDbContext context, DatabaseColumn buildingIdColumn)
    {
        var columns = ReadColumns(context, "floors");
        if (columns.Count != 6)
            FailSchemaValidation(context, $"`floors` must contain exactly 6 columns but contains {columns.Count}.");

        RequireColumn(context, columns, "id", "char", 36, false);
        RequireColumn(context, columns, "building_id", "char", 36, false, null, buildingIdColumn.Collation);
        if (!string.Equals(columns["building_id"].CharacterSet, buildingIdColumn.CharacterSet,
                StringComparison.OrdinalIgnoreCase))
            FailSchemaValidation(context, "`floors.building_id` character set does not match `buildings.Id`.");
        RequireColumn(context, columns, "label", "varchar", 50, false);
        RequireColumn(context, columns, "level", "int", null, false);
        RequireColumn(context, columns, "created_at", "datetime", null, false, 6);
        RequireColumn(context, columns, "updated_at", "datetime", null, false, 6);
    }

    // Existing EF-created databases may have `updated_at datetime` (precision 0),
    // whereas the current FloorPlanLayout mapping requires datetime(6). Widening
    // the fractional precision preserves existing timestamp values and is safe to
    // repeat. Never silently change a different column type, nullability or default.
    private static void UpgradeLegacyFloorPlanUpdatedAtPrecision(AppDbContext context)
    {
        var columns = ReadColumns(context, "floor_plan_layouts");
        if (!columns.TryGetValue("updated_at", out var column))
            FailSchemaValidation(context, "Required column `floor_plan_layouts.updated_at` is missing.");
        if (!string.Equals(column!.DataType, "datetime", StringComparison.OrdinalIgnoreCase) || column.Nullable)
            FailSchemaValidation(context,
                "`floor_plan_layouts.updated_at` must be a non-nullable datetime before precision upgrade.");

        // A column default would be lost by MODIFY COLUMN unless preserved explicitly.
        if (column.DateTimePrecision != 6 && column.DefaultValue is not null)
            FailSchemaValidation(context,
                "`floor_plan_layouts.updated_at` has a default; refusing to alter its definition automatically.");

        string? alterSql;
        try
        {
            alterSql = FloorSchemaDefinition.GetLayoutTimestampPrecisionUpgradeSql(column.DateTimePrecision);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            FailSchemaValidation(context, ex.Message);
            return;
        }

        if (alterSql is not null)
            ExecuteNonQuery(context, alterSql);
    }

    private static void ValidateFloorPlanLayoutSchema(AppDbContext context)
    {
        var columns = ReadColumns(context, "floor_plan_layouts");
        if (columns.Count != 8)
            FailSchemaValidation(context,
                $"`floor_plan_layouts` must contain exactly 8 columns but contains {columns.Count}.");
        RequireColumn(context, columns, "id", "char", 36, false);
        RequireColumn(context, columns, "organization_id", "char", 36, false);
        RequireColumn(context, columns, "building_id", "char", 36, false);
        RequireColumn(context, columns, "floor_label", "varchar", 50, false);
        RequireColumn(context, columns, "layout_json", "longtext", 4_294_967_295L, false);
        RequireColumn(context, columns, "updated_by_user_id", "int", null, false);
        RequireColumn(context, columns, "updated_at", "datetime", null, false, 6);
        RequireColumn(context, columns, "version", "bigint", null, false);
        var defaultValue = columns["version"].DefaultValue;
        if (!string.Equals(defaultValue, "1", StringComparison.OrdinalIgnoreCase))
            FailSchemaValidation(context,
                $"`floor_plan_layouts.version` must default to 1 but defaults to `{defaultValue ?? "NULL"}`.");
    }

    private static Dictionary<string, DatabaseColumn> ReadColumns(AppDbContext context, string tableName)
    {
        var result = new Dictionary<string, DatabaseColumn>(StringComparer.OrdinalIgnoreCase);
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        try
        {
            if (!wasOpen) connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = @"
SELECT column_name, data_type, character_maximum_length, datetime_precision,
       is_nullable, collation_name, column_default, character_set_name
FROM information_schema.columns
WHERE table_schema = DATABASE() AND table_name = @table_name
ORDER BY ordinal_position;";
            AddParameter(command, "@table_name", tableName);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                result[name] = new DatabaseColumn(
                    reader.GetString(1),
                    reader.IsDBNull(2) ? null : Convert.ToInt64(reader.GetValue(2)),
                    reader.IsDBNull(3) ? null : Convert.ToInt32(reader.GetValue(3)),
                    string.Equals(reader.GetString(4), "YES", StringComparison.OrdinalIgnoreCase),
                    reader.IsDBNull(5) ? null : reader.GetString(5),
                    reader.IsDBNull(6) ? null : Convert.ToString(reader.GetValue(6)),
                    reader.IsDBNull(7) ? null : reader.GetString(7));
            }
            return result;
        }
        catch (Exception exception)
        {
            ThrowSchemaFailure(context, $"inspect columns for `{tableName}`", exception);
            return result;
        }
        finally
        {
            if (!wasOpen && connection.State != ConnectionState.Closed) connection.Close();
        }
    }

    private static void RequireColumn(
        AppDbContext context,
        IReadOnlyDictionary<string, DatabaseColumn> columns,
        string name,
        string dataType,
        long? maximumLength,
        bool nullable,
        int? dateTimePrecision = null,
        string? collation = null)
    {
        if (!columns.TryGetValue(name, out var column))
            FailSchemaValidation(context, $"Required column `{name}` is missing.");
        if (!string.Equals(column!.DataType, dataType, StringComparison.OrdinalIgnoreCase) ||
            column.MaximumLength != maximumLength ||
            column.Nullable != nullable ||
            column.DateTimePrecision != dateTimePrecision ||
            (collation is not null && !string.Equals(column.Collation, collation, StringComparison.OrdinalIgnoreCase)))
            FailSchemaValidation(context,
                $"Column `{name}` does not match the EF model. Actual: type={column.DataType}, " +
                $"length={column.MaximumLength?.ToString() ?? "-"}, precision={column.DateTimePrecision?.ToString() ?? "-"}, " +
                $"nullable={column.Nullable}, collation={column.Collation ?? "-"}.");
    }

    private static void ReconcileFloors(AppDbContext context)
    {
        var existing = new List<ExistingFloorSchemaRow>();
        var sources = new List<LegacyFloorSchemaSource>();
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        try
        {
            if (!wasOpen) connection.Open();
            using (var existingCommand = connection.CreateCommand())
            {
                existingCommand.CommandText = "SELECT `building_id`, `label`, `level` FROM `floors`;";
                using var reader = existingCommand.ExecuteReader();
                while (reader.Read())
                    existing.Add(new ExistingFloorSchemaRow(
                        Convert.ToString(reader.GetValue(0)) ?? string.Empty,
                        reader.GetString(1),
                        reader.GetInt32(2)));
            }

            using (var sourceCommand = connection.CreateCommand())
            {
                sourceCommand.CommandText = @"
SELECT `building_id`, TRIM(`floor_label`) AS `floor_label`
FROM `zones`
WHERE `floor_label` IS NOT NULL AND TRIM(`floor_label`) <> ''
UNION
SELECT `building_id`, TRIM(`floor_label`) AS `floor_label`
FROM `floor_plan_layouts`
WHERE `floor_label` IS NOT NULL AND TRIM(`floor_label`) <> '';";
                using var reader = sourceCommand.ExecuteReader();
                while (reader.Read())
                    sources.Add(new LegacyFloorSchemaSource(
                        Convert.ToString(reader.GetValue(0)) ?? string.Empty,
                        reader.GetString(1)));
            }

            foreach (var missing in FloorSchemaReconciliation.Plan(existing, sources))
            {
                using var insert = connection.CreateCommand();
                insert.CommandText = @"
INSERT INTO `floors` (`id`, `building_id`, `label`, `level`, `created_at`, `updated_at`)
VALUES (@id, @building_id, @label, @level, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6));";
                AddParameter(insert, "@id", Guid.NewGuid().ToString());
                AddParameter(insert, "@building_id", missing.BuildingId);
                AddParameter(insert, "@label", missing.Label);
                AddParameter(insert, "@level", missing.Level, DbType.Int32);
                insert.ExecuteNonQuery();
            }
        }
        catch (Exception exception)
        {
            ThrowSchemaFailure(context, "reconcile persisted floors from legacy zones and floor plans", exception);
        }
        finally
        {
            if (!wasOpen && connection.State != ConnectionState.Closed) connection.Close();
        }
    }

    private static void AddParameter(System.Data.Common.DbCommand command, string name, object value, DbType type = DbType.String)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.DbType = type;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static void FailSchemaValidation(AppDbContext context, string message) =>
        ThrowSchemaFailure(context, "validate incremental floor schema", new InvalidOperationException(message));

    private static void EnsureColumnExists(AppDbContext context, string tableName, string columnName, string definition)
    {
        EnsureSchemaObject(context,
            $"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = '{tableName}' AND column_name = '{columnName}';",
            $"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {definition};");
    }

    private static void EnsureIndexExists(AppDbContext context, string tableName, string indexName, string createSql)
    {
        EnsureSchemaObject(context,
            $"SELECT COUNT(*) FROM information_schema.statistics WHERE table_schema = DATABASE() AND table_name = '{tableName}' AND index_name = '{indexName}';",
            createSql);
    }

    private static void EnsureForeignKeyExists(
        AppDbContext context, string tableName, string constraintName, string createSql)
    {
        EnsureSchemaObject(context,
            $"SELECT COUNT(*) FROM information_schema.referential_constraints " +
            $"WHERE constraint_schema = DATABASE() AND table_name = '{tableName}' " +
            $"AND constraint_name = '{constraintName}';",
            createSql);
    }

    private static void EnsureSchemaObject(AppDbContext context, string existsSql, string createSql)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        try
        {
            if (!wasOpen) connection.Open();
            using var exists = connection.CreateCommand();
            exists.CommandText = existsSql;
            if (Convert.ToInt64(exists.ExecuteScalar()) == 0)
            {
                using var create = connection.CreateCommand();
                create.CommandText = createSql;
                create.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            ThrowSchemaFailure(context, createSql, ex);
        }
        finally
        {
            if (!wasOpen && connection.State != ConnectionState.Closed) connection.Close();
        }
    }

    private static void ExecuteNonQuery(AppDbContext context, string sql)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        try
        {
            if (!wasOpen) connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            ThrowSchemaFailure(context, sql, ex);
        }
        finally
        {
            if (!wasOpen && connection.State != ConnectionState.Closed) connection.Close();
        }
    }

    private static void EnsureTableExists(AppDbContext context, string tableName, string createTableSql)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        try
        {
            if (!wasOpen) connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = '{tableName}';";
            var result = Convert.ToInt64(cmd.ExecuteScalar());

            if (result == 0)
            {
                using var createCmd = connection.CreateCommand();
                createCmd.CommandText = createTableSql;
                createCmd.ExecuteNonQuery();
            }

        }
        catch (Exception ex)
        {
            ThrowSchemaFailure(context, $"create or verify table `{tableName}`", ex);
        }
        finally
        {
            if (!wasOpen && connection.State != ConnectionState.Closed) connection.Close();
        }
    }

    private static void ThrowSchemaFailure(AppDbContext context, string operation, Exception exception)
    {
        var logger = context.GetService<ILoggerFactory>().CreateLogger("DatabaseSchemaAssurance");
        logger.LogCritical(exception, "Critical database schema operation failed: {Operation}", operation);
        throw new InvalidOperationException(
            $"Critical database schema operation failed: {operation}. Startup was stopped to prevent runtime data errors.",
            exception);
    }

    private sealed record DatabaseColumn(
        string DataType,
        long? MaximumLength,
        int? DateTimePrecision,
        bool Nullable,
        string? Collation,
        string? DefaultValue,
        string? CharacterSet);
}

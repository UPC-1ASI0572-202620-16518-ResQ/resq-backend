using Microsoft.EntityFrameworkCore;

namespace ResQ.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class WebApplicationExtensions
{
    public static void UseDatabaseCreationAssurance(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

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

        // Same DDL that EF Core generates for Incident Management (keeps the "Id" column name used by the model)
        EnsureTableExists(context, "incidents", @"
CREATE TABLE IF NOT EXISTS `incidents` (
    `Id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `zone_id` char(36) NOT NULL,
    `type` longtext NOT NULL,
    `level` longtext NOT NULL,
    `status` longtext NOT NULL,
    `assigned_to` char(36) NULL,
    `created_at` datetime NOT NULL,
    `resolved_at` datetime NULL,
    `resolution_notes` longtext NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        // Same DDL that EF Core generates for Subscriptions
        EnsureTableExists(context, "subscriptions", @"
CREATE TABLE IF NOT EXISTS `subscriptions` (
    `Id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `status` longtext NOT NULL,
    `start_date` datetime(6) NOT NULL,
    `end_date` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        // IAM organizations (same DDL that EF Core generates)
        EnsureTableExists(context, "organizations", @"
CREATE TABLE IF NOT EXISTS `organizations` (
    `Id` char(36) NOT NULL,
    `name` varchar(120) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        // Columns added after the tables already existed in local databases
        EnsureColumnExists(context, "users", "organization_id", "char(36) NULL");
        EnsureColumnExists(context, "incidents", "organization_id", "char(36) NULL");

        AssignLegacyRowsToOrganization(context);
    }

    /// <summary>
    ///     One-time upgrade for databases created before organizations existed: users and incidents
    ///     without organization are moved to the organization that already owns the legacy data
    ///     (the former single development organization), and those users become its admins.
    ///     It does nothing on new databases or once every row has an organization.
    /// </summary>
    private static void AssignLegacyRowsToOrganization(AppDbContext context)
    {
        const string legacyOrganizationId = "11111111-1111-1111-1111-111111111111";
        try
        {
            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen) connection.Open();

            using var check = connection.CreateCommand();
            check.CommandText = @"SELECT
    (SELECT COUNT(*) FROM `users` WHERE `organization_id` IS NULL OR `organization_id` = '') +
    (SELECT COUNT(*) FROM `incidents` WHERE `organization_id` IS NULL OR `organization_id` = '');";
            var pending = Convert.ToInt64(check.ExecuteScalar());

            if (pending > 0)
            {
                using var upgrade = connection.CreateCommand();
                upgrade.CommandText = $@"
INSERT IGNORE INTO `organizations` (`Id`, `name`, `created_at`)
VALUES ('{legacyOrganizationId}', 'Legacy development organization', UTC_TIMESTAMP(6));
UPDATE `users` SET `organization_id` = '{legacyOrganizationId}', `role` = 'Admin'
WHERE `organization_id` IS NULL OR `organization_id` = '';
UPDATE `incidents` SET `organization_id` = '{legacyOrganizationId}'
WHERE `organization_id` IS NULL OR `organization_id` = '';";
                upgrade.ExecuteNonQuery();
                Console.WriteLine($"Assigned {pending} legacy rows to organization {legacyOrganizationId}.");
            }

            if (!wasOpen) connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error assigning legacy rows to an organization: {ex.Message}");
        }
    }

    private static void EnsureColumnExists(AppDbContext context, string tableName, string columnName,
        string columnDefinition)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
            if (!wasOpen) connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = '{tableName}' AND column_name = '{columnName}';";
            var exists = Convert.ToInt64(cmd.ExecuteScalar()) > 0;

            if (!exists)
            {
                using var alter = connection.CreateCommand();
                alter.CommandText = $"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {columnDefinition};";
                alter.ExecuteNonQuery();
            }

            if (!wasOpen) connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ensuring column {tableName}.{columnName}: {ex.Message}");
        }
    }

    private static void EnsureTableExists(AppDbContext context, string tableName, string createTableSql)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;
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

            if (!wasOpen) connection.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ensuring table {tableName}: {ex.Message}");
        }
    }
}
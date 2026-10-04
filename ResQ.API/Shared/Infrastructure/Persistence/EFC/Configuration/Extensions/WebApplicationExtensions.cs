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

        EnsureTableExists(context, "alerts", @"
CREATE TABLE IF NOT EXISTS `alerts` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `risk_detection_id` varchar(64) NOT NULL,
    `risk_type_code` varchar(50) NOT NULL,
    `severity_code` varchar(20) NOT NULL,
    `building_id` char(36) NULL,
    `zone_id` char(36) NULL,
    `detected_at` datetime NOT NULL,
    `generated_at` datetime NOT NULL,
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
    `requested_at` datetime NOT NULL,
    `completed_at` datetime NULL,
    `failure_reason` varchar(500) NULL,
    PRIMARY KEY (`id`),
    KEY `ix_notification_deliveries_alert_id` (`alert_id`),
    CONSTRAINT `fk_notification_deliveries_alerts_alert_id` FOREIGN KEY (`alert_id`) REFERENCES `alerts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "response_policies", @"
CREATE TABLE IF NOT EXISTS `response_policies` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `risk_type_code` varchar(50) NOT NULL,
    `status` varchar(16) NOT NULL,
    `created_at` datetime NOT NULL,
    `updated_at` datetime NOT NULL,
    `version` bigint NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_response_policies_organization_id_risk_type_code` (`organization_id`, `risk_type_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "response_actions", @"
CREATE TABLE IF NOT EXISTS `response_actions` (
    `id` char(36) NOT NULL,
    `policy_id` char(36) NOT NULL,
    `action_code` varchar(80) NOT NULL,
    `target_device_id` char(36) NOT NULL,
    `target_capability_code` varchar(80) NOT NULL,
    `authorization_mode` varchar(20) NOT NULL,
    `critical` tinyint(1) NOT NULL,
    PRIMARY KEY (`id`),
    KEY `ix_response_actions_policy_id` (`policy_id`),
    CONSTRAINT `fk_response_actions_response_policies_policy_id` FOREIGN KEY (`policy_id`) REFERENCES `response_policies` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

        EnsureTableExists(context, "response_executions", @"
CREATE TABLE IF NOT EXISTS `response_executions` (
    `id` char(36) NOT NULL,
    `organization_id` char(36) NOT NULL,
    `alert_id` char(36) NOT NULL,
    `risk_detection_id` varchar(64) NOT NULL,
    `policy_id` char(36) NOT NULL,
    `action_id` char(36) NOT NULL,
    `action_code` varchar(80) NOT NULL,
    `target_device_id` char(36) NOT NULL,
    `target_capability_code` varchar(80) NOT NULL,
    `authorization_mode` varchar(20) NOT NULL,
    `critical` tinyint(1) NOT NULL,
    `status` varchar(30) NOT NULL,
    `requested_at` datetime NOT NULL,
    `authorization_id` char(36) NULL,
    `authorization_decision` varchar(16) NULL,
    `decided_by_user_id` varchar(64) NULL,
    `decided_at` datetime NULL,
    `result_successful` tinyint(1) NULL,
    `result_code` varchar(80) NULL,
    `result_message` varchar(500) NULL,
    `result_completed_at` datetime NULL,
    PRIMARY KEY (`id`),
    KEY `ix_response_executions_organization_id_risk_detection_id` (`organization_id`, `risk_detection_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");
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
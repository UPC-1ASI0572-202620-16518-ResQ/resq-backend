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
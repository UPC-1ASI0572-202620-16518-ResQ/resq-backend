using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace ResQ.API.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    risk_detection_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    risk_type_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    severity_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    building_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    detected_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_alerts", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "buildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    building_code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    street_address = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    district = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    country_code = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    administrative_status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_buildings", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "detection_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    risk_type_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    severity_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    variable_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    @operator = table.Column<string>(name: "operator", type: "varchar(30)", maxLength: 30, nullable: false),
                    threshold = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    configured_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_detection_rules", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "device_monitoring_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    device_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    availability = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    last_measurement_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_device_monitoring_states", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    device_code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    manufacturer = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    model = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    serial_number = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    building_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    external_source_system = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true),
                    external_device_id = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true),
                    administrative_status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_devices", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    type = table.Column<string>(type: "longtext", nullable: false),
                    level = table.Column<string>(type: "longtext", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    assigned_to = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    resolution_notes = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_incidents", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "measurements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    device_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    building_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    variable_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    unit = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    measured_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_measurements", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_organizations", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "response_executions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    alert_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    risk_detection_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    action_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    action_code = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    target_device_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    target_capability_code = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    authorization_mode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    critical = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    authorization_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    authorization_decision = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true),
                    decided_by_user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_response_executions", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "risk_detections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    rule_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    risk_type_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    current_severity_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    building_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    location_resolution_status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    detected_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    last_evidence_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_risk_detections", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sensor_connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    sensor_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    last_heartbeat = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    signal_strength = table.Column<int>(type: "int", nullable: true),
                    status_changed_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sensor_connections", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_subscriptions", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    time_zone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "UTC"),
                    receive_sms_alerts = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_user_profiles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "longtext", nullable: false),
                    password_hash = table.Column<string>(type: "longtext", nullable: false),
                    role = table.Column<string>(type: "longtext", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_users", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "zone_monitoring_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    zone_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    building_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    last_updated_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    active_risk_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    condition_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_zone_monitoring_states", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notification_deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    recipient_user_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    channel = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    destination = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: true),
                    failure_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    alert_id = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_notification_deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "f_k_notification_deliveries_alerts_alert_id",
                        column: x => x.alert_id,
                        principalTable: "alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    building_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    zone_code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    floor_label = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    administrative_status = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_zones", x => x.Id);
                    table.ForeignKey(
                        name: "f_k_zones_buildings_building_id",
                        column: x => x.building_id,
                        principalTable: "buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "device_capabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    code = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    kind = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    unit = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    device_id = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_device_capabilities", x => x.Id);
                    table.ForeignKey(
                        name: "f_k_device_capabilities_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "detection_evidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    measurement_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    device_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    variable_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    measured_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    risk_detection_id = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_detection_evidences", x => x.Id);
                    table.ForeignKey(
                        name: "f_k_detection_evidences_risk_detections_risk_detection_id",
                        column: x => x.risk_detection_id,
                        principalTable: "risk_detections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "severity_changes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    previous_severity_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    new_severity_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    risk_detection_id = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_severity_changes", x => x.Id);
                    table.ForeignKey(
                        name: "f_k_severity_changes_risk_detections_risk_detection_id",
                        column: x => x.risk_detection_id,
                        principalTable: "risk_detections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "heartbeat_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "datetime(6)", precision: 6, nullable: false),
                    signal_strength = table.Column<int>(type: "int", nullable: true),
                    sensor_connection_id = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_heartbeat_records", x => x.Id);
                    table.ForeignKey(
                        name: "f_k_heartbeat_records_sensor_connections_sensor_connection_id",
                        column: x => x.sensor_connection_id,
                        principalTable: "sensor_connections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i_x_buildings_organization_id_building_code",
                table: "buildings",
                columns: new[] { "organization_id", "building_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_detection_evidences_risk_detection_id",
                table: "detection_evidences",
                column: "risk_detection_id");

            migrationBuilder.CreateIndex(
                name: "i_x_device_capabilities_device_id",
                table: "device_capabilities",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "i_x_device_monitoring_states_organization_id_device_id",
                table: "device_monitoring_states",
                columns: new[] { "organization_id", "device_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_devices_organization_id_device_code",
                table: "devices",
                columns: new[] { "organization_id", "device_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_heartbeat_records_sensor_connection_id",
                table: "heartbeat_records",
                column: "sensor_connection_id");

            migrationBuilder.CreateIndex(
                name: "i_x_measurements_organization_id_device_id_measured_at",
                table: "measurements",
                columns: new[] { "organization_id", "device_id", "measured_at" });

            migrationBuilder.CreateIndex(
                name: "i_x_measurements_organization_id_zone_id_measured_at",
                table: "measurements",
                columns: new[] { "organization_id", "zone_id", "measured_at" });

            migrationBuilder.CreateIndex(
                name: "i_x_notification_deliveries_alert_id",
                table: "notification_deliveries",
                column: "alert_id");

            migrationBuilder.CreateIndex(
                name: "i_x_response_executions_organization_id_alert_id",
                table: "response_executions",
                columns: new[] { "organization_id", "alert_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_risk_detections_organization_id_risk_type_code",
                table: "risk_detections",
                columns: new[] { "organization_id", "risk_type_code" });

            migrationBuilder.CreateIndex(
                name: "i_x_sensor_connections_organization_id_sensor_id",
                table: "sensor_connections",
                columns: new[] { "organization_id", "sensor_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_severity_changes_risk_detection_id",
                table: "severity_changes",
                column: "risk_detection_id");

            migrationBuilder.CreateIndex(
                name: "i_x_zone_monitoring_states_organization_id_zone_id",
                table: "zone_monitoring_states",
                columns: new[] { "organization_id", "zone_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_zones_building_id_zone_code",
                table: "zones",
                columns: new[] { "building_id", "zone_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detection_evidences");

            migrationBuilder.DropTable(
                name: "detection_rules");

            migrationBuilder.DropTable(
                name: "device_capabilities");

            migrationBuilder.DropTable(
                name: "device_monitoring_states");

            migrationBuilder.DropTable(
                name: "heartbeat_records");

            migrationBuilder.DropTable(
                name: "incidents");

            migrationBuilder.DropTable(
                name: "measurements");

            migrationBuilder.DropTable(
                name: "notification_deliveries");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "response_executions");

            migrationBuilder.DropTable(
                name: "severity_changes");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "zone_monitoring_states");

            migrationBuilder.DropTable(
                name: "zones");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "sensor_connections");

            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "risk_detections");

            migrationBuilder.DropTable(
                name: "buildings");
        }
    }
}

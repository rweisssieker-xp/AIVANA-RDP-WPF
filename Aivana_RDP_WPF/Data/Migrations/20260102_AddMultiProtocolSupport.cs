using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aivana_RDP_WPF.Data.Migrations;

/// <inheritdoc />
public partial class AddMultiProtocolSupport : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ProtocolType",
            table: "ConnectionProfiles",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "ProtocolSpecificSettings",
            table: "ConnectionProfiles",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "MacAddress",
            table: "ConnectionProfiles",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "EnableWakeOnLan",
            table: "ConnectionProfiles",
            type: "BOOLEAN",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "WakeTimeoutSeconds",
            table: "ConnectionProfiles",
            type: "INTEGER",
            nullable: false,
            defaultValue: 30);

        migrationBuilder.AddColumn<bool>(
            name: "UseSshTunnel",
            table: "ConnectionProfiles",
            type: "BOOLEAN",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "PerformanceMetrics",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                ConnectionProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                LatencyMs = table.Column<double>(type: "REAL", nullable: true),
                BandwidthMbps = table.Column<double>(type: "REAL", nullable: true),
                PacketLossPercent = table.Column<double>(type: "REAL", nullable: true),
                FrameRate = table.Column<double>(type: "REAL", nullable: true),
                CpuUsagePercent = table.Column<double>(type: "REAL", nullable: true),
                MemoryUsageMB = table.Column<double>(type: "REAL", nullable: true),
                QualityScore = table.Column<int>(type: "INTEGER", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                table.ForeignKey(
                    name: "FK_PerformanceMetrics_ConnectionProfiles_ConnectionProfileId",
                    column: x => x.ConnectionProfileId,
                    principalTable: "ConnectionProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Settings",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Key = table.Column<string>(type: "TEXT", nullable: false),
                Value = table.Column<string>(type: "TEXT", nullable: false),
                Category = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Settings", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PerformanceMetrics_ConnectionProfileId",
            table: "PerformanceMetrics",
            column: "ConnectionProfileId");

        migrationBuilder.CreateIndex(
            name: "IX_Settings_Key",
            table: "Settings",
            column: "Key",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PerformanceMetrics");

        migrationBuilder.DropTable(
            name: "Settings");

        migrationBuilder.DropColumn(
            name: "ProtocolType",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "ProtocolSpecificSettings",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "MacAddress",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "EnableWakeOnLan",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "WakeTimeoutSeconds",
            table: "ConnectionProfiles");

        migrationBuilder.DropColumn(
            name: "UseSshTunnel",
            table: "ConnectionProfiles");
    }
}

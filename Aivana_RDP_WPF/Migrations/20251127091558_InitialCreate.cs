using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aivana_RDP_WPF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectionProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ServerAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 3389),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Domain = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    GroupName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]"),
                    Settings = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastConnectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ConnectionCount = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConnectionProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DisconnectedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DurationTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionHistories_ConnectionProfiles_ConnectionProfileId",
                        column: x => x.ConnectionProfileId,
                        principalTable: "ConnectionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionProfiles_GroupName",
                table: "ConnectionProfiles",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionProfiles_IsFavorite",
                table: "ConnectionProfiles",
                column: "IsFavorite");

            migrationBuilder.CreateIndex(
                name: "IX_SessionHistories_ConnectedAt",
                table: "SessionHistories",
                column: "ConnectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionHistories_ConnectionProfileId",
                table: "SessionHistories",
                column: "ConnectionProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionHistories");

            migrationBuilder.DropTable(
                name: "ConnectionProfiles");
        }
    }
}

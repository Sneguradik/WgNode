using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WgNode.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Peers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "BLOB", maxLength: 32, nullable: false),
                    PrivateKey = table.Column<byte[]>(type: "BLOB", maxLength: 32, nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    IpAddress = table.Column<byte[]>(type: "BLOB", maxLength: 4, nullable: false),
                    LatestHandShake = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BytesReceived = table.Column<ulong>(type: "INTEGER", nullable: false),
                    BytesSent = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ServerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "BLOB", maxLength: 32, nullable: false),
                    PrivateKey = table.Column<byte[]>(type: "BLOB", maxLength: 32, nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Peers");

            migrationBuilder.DropTable(
                name: "Servers");
        }
    }
}

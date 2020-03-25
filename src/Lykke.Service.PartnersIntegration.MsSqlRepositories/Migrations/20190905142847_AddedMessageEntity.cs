using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.PartnersIntegration.MsSqlRepositories.Migrations
{
    public partial class AddedMessageEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "partners_integration");

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "partners_integration",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTimestamp = table.Column<DateTime>(nullable: false),
                    PartnerId = table.Column<string>(maxLength: 100, nullable: false),
                    CustomerId = table.Column<string>(maxLength: 100, nullable: false),
                    Subject = table.Column<string>(maxLength: 100, nullable: false),
                    ExternalLocationId = table.Column<string>(maxLength: 100, nullable: true),
                    PosId = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages",
                schema: "partners_integration");
        }
    }
}

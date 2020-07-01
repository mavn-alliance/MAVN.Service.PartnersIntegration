using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.PartnersIntegration.MsSqlRepositories.Migrations
{
    public partial class Initial : Migration
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
                    table.PrimaryKey("PK_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payment_processed_callback_url",
                schema: "partners_integration",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PaymentRequestId = table.Column<string>(maxLength: 100, nullable: false),
                    Url = table.Column<string>(maxLength: 512, nullable: false),
                    RequestAuthToken = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_processed_callback_url", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_processed_callback_url_PaymentRequestId",
                schema: "partners_integration",
                table: "payment_processed_callback_url",
                column: "PaymentRequestId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "messages",
                schema: "partners_integration");

            migrationBuilder.DropTable(
                name: "payment_processed_callback_url",
                schema: "partners_integration");
        }
    }
}

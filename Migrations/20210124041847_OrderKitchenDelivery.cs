using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GreenStop.API.Migrations
{
    public partial class OrderKitchenDelivery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderDeliveryDetails",
                columns: table => new
                {
                    OrderId = table.Column<string>(maxLength: 30, nullable: false),
                    DeliveryUserId = table.Column<string>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDeliveryDetails", x => new { x.OrderId, x.DeliveryUserId });
                });

            migrationBuilder.CreateTable(
                name: "OrderKitchenDetails",
                columns: table => new
                {
                    OrderId = table.Column<string>(maxLength: 30, nullable: false),
                    KitchenUserId = table.Column<string>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderKitchenDetails", x => new { x.OrderId, x.KitchenUserId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDeliveryDetails");

            migrationBuilder.DropTable(
                name: "OrderKitchenDetails");
        }
    }
}

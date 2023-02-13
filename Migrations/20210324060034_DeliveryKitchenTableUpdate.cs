using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GreenStop.API.Migrations
{
    public partial class DeliveryKitchenTableUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "OrderKitchenDetails",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeliveryTimeId",
                table: "OrderKitchenDetails",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "OrderDeliveryDetails",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeliveryTimeId",
                table: "OrderDeliveryDetails",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "OrderKitchenDetails");

            migrationBuilder.DropColumn(
                name: "DeliveryTimeId",
                table: "OrderKitchenDetails");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "OrderDeliveryDetails");

            migrationBuilder.DropColumn(
                name: "DeliveryTimeId",
                table: "OrderDeliveryDetails");
        }
    }
}

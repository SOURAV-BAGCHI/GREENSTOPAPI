using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GreenStop.API.Migrations
{
    public partial class OrderDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryTimeDetails",
                columns: table => new
                {
                    DeliveryTimeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeSlot = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryTimeDetails", x => x.DeliveryTimeId);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderId = table.Column<string>(maxLength: 30, nullable: false),
                    UserId = table.Column<string>(maxLength: 450, nullable: false),
                    CustomerInfo = table.Column<string>(nullable: false),
                    ItemDetails = table.Column<string>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    OrderStatus = table.Column<int>(nullable: false),
                    OrderStatusLog = table.Column<string>(nullable: false),
                    PaymentStatus = table.Column<bool>(nullable: false),
                    DeliveryDate = table.Column<DateTime>(nullable: false),
                    DeliveryTimeId = table.Column<int>(nullable: false),
                    PaymentInfo = table.Column<string>(nullable: true),
                    SubTotal = table.Column<double>(nullable: false),
                    Tax = table.Column<double>(nullable: false),
                    DeliveryCharges = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderId);
                });

            migrationBuilder.InsertData(
                table: "DeliveryTimeDetails",
                columns: new[] { "DeliveryTimeId", "TimeSlot" },
                values: new object[,]
                {
                    { 1, "11 am - 12 pm" },
                    { 2, "12 pm - 1 pm" },
                    { 3, "1 pm - 2 pm" },
                    { 4, "3 pm - 4 pm" },
                    { 5, "4 pm - 5 pm" },
                    { 6, "5 pm - 6 pm" },
                    { 7, "6 pm - 7 pm" },
                    { 8, "7 pm - 8 pm" },
                    { 9, "8 pm - 9 pm" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryTimeDetails");

            migrationBuilder.DropTable(
                name: "OrderDetails");
        }
    }
}

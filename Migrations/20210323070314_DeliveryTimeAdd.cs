using Microsoft.EntityFrameworkCore.Migrations;

namespace GreenStop.API.Migrations
{
    public partial class DeliveryTimeAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 4,
                column: "TimeSlot",
                value: "2 pm - 3 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 5,
                column: "TimeSlot",
                value: "3 pm - 4 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 6,
                column: "TimeSlot",
                value: "4 pm - 5 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 7,
                column: "TimeSlot",
                value: "5 pm - 6 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 8,
                column: "TimeSlot",
                value: "6 pm - 7 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 9,
                column: "TimeSlot",
                value: "7 pm - 8 pm");

            migrationBuilder.InsertData(
                table: "DeliveryTimeDetails",
                columns: new[] { "DeliveryTimeId", "TimeSlot" },
                values: new object[,]
                {
                    { 10, "8 pm - 9 pm" },
                    { 11, "9 pm - 10 pm" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 11);

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 4,
                column: "TimeSlot",
                value: "3 pm - 4 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 5,
                column: "TimeSlot",
                value: "4 pm - 5 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 6,
                column: "TimeSlot",
                value: "5 pm - 6 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 7,
                column: "TimeSlot",
                value: "6 pm - 7 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 8,
                column: "TimeSlot",
                value: "7 pm - 8 pm");

            migrationBuilder.UpdateData(
                table: "DeliveryTimeDetails",
                keyColumn: "DeliveryTimeId",
                keyValue: 9,
                column: "TimeSlot",
                value: "8 pm - 9 pm");
        }
    }
}

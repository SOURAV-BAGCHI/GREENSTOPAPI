using Microsoft.EntityFrameworkCore.Migrations;

namespace GreenStop.API.Migrations
{
    public partial class newRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "7");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "ChiefManager", "CHIEFMANAGER" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "CustomerService", "CUSTOMERSERVICE" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "KitchenManager", "KITCHENMANAGER" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Delivery", "DELIVERY" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "OperationManager", "OPERATIONMANAGER" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Accounts", "ACCOUNTS" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "5",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "DeliveryManager", "DELIVERYMANAGER" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Kitchen", "KITCHEN" });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "7", null, "DeliveryAgents", "DELIVERYAGENTS" },
                    { "8", null, "CustomerService", "CUSTOMERSERVICE" }
                });
        }
    }
}

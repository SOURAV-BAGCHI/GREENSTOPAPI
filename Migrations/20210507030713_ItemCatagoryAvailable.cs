using Microsoft.EntityFrameworkCore.Migrations;

namespace GreenStop.API.Migrations
{
    public partial class ItemCatagoryAvailable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Available",
                table: "ItemDetails",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Available",
                table: "CatagoryDetails",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Available",
                table: "ItemDetails");

            migrationBuilder.DropColumn(
                name: "Available",
                table: "CatagoryDetails");
        }
    }
}

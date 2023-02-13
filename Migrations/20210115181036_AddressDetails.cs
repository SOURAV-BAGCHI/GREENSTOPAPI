using Microsoft.EntityFrameworkCore.Migrations;

namespace GreenStop.API.Migrations
{
    public partial class AddressDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressDetails",
                columns: table => new
                {
                    AddressId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(maxLength: 450, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    AddressLine1 = table.Column<string>(maxLength: 200, nullable: false),
                    AddressLine2 = table.Column<string>(maxLength: 200, nullable: true),
                    Landmark = table.Column<string>(maxLength: 200, nullable: false),
                    Phone = table.Column<string>(maxLength: 15, nullable: false),
                    Pincode = table.Column<string>(maxLength: 6, nullable: false),
                    AlternatePhone = table.Column<string>(maxLength: 15, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressDetails", x => x.AddressId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressDetails");
        }
    }
}

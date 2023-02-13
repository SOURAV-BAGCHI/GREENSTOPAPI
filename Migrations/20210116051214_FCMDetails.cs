using Microsoft.EntityFrameworkCore.Migrations;

namespace GreenStop.API.Migrations
{
    public partial class FCMDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerFCMDetails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServerApiKey = table.Column<string>(nullable: false),
                    SenderId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerFCMDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserFCMDetails",
                columns: table => new
                {
                    FCMDetailsId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(maxLength: 450, nullable: false),
                    FCMToken = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFCMDetails", x => x.FCMDetailsId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerFCMDetails");

            migrationBuilder.DropTable(
                name: "UserFCMDetails");
        }
    }
}

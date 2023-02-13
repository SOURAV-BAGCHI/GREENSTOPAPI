using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GreenStop.API.Migrations
{
    public partial class userFeedback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFeedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rating = table.Column<short>(nullable: false),
                    CustomerName = table.Column<string>(maxLength: 200, nullable: false),
                    ReviewTitle = table.Column<string>(maxLength: 50, nullable: true),
                    Review = table.Column<string>(maxLength: 500, nullable: true),
                    OrderId = table.Column<string>(nullable: false),
                    FeedbackDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFeedbacks", x => x.FeedbackId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFeedbacks");
        }
    }
}

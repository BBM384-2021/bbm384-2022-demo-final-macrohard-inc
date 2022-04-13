using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkedHUCENGv2.Migrations
{
    public partial class Migration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Notifications",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AccountId",
                table: "Notifications",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_AccountId",
                table: "Notifications",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_AccountId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_AccountId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Notifications");
        }
    }
}

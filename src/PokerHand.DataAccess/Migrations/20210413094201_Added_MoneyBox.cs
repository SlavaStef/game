using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerHand.DataAccess.Migrations
{
    public partial class Added_MoneyBox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoneyBoxAmount",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoneyBoxAmount",
                table: "AspNetUsers");
        }
    }
}

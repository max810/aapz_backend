using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class ChangedCompanyFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Drivers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Managers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Drivers",
                nullable: false,
                defaultValue: 0);
        }
    }
}

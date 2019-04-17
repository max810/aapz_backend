using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class ChangedCompanyKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Companies_CompanyId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Companies_CompanyId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_CompanyId",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_CompanyId",
                table: "Drivers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Companies",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Companies");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Managers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Drivers",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Companies",
                table: "Companies",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_CompanyName",
                table: "Managers",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CompanyName",
                table: "Drivers",
                column: "CompanyName");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Companies_CompanyName",
                table: "Drivers",
                column: "CompanyName",
                principalTable: "Companies",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Companies_CompanyName",
                table: "Managers",
                column: "CompanyName",
                principalTable: "Companies",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Companies_CompanyName",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Companies_CompanyName",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Managers_CompanyName",
                table: "Managers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_CompanyName",
                table: "Drivers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Companies",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Managers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Drivers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Companies",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Companies",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Companies",
                table: "Companies",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Managers_CompanyId",
                table: "Managers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CompanyId",
                table: "Drivers",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Companies_CompanyId",
                table: "Drivers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Companies_CompanyId",
                table: "Managers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

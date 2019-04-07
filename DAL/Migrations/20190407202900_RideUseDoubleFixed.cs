using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class RideUseDoubleFixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Drivers_DriverUserId",
                table: "Rides");

            migrationBuilder.DropIndex(
                name: "IX_Rides_DriverUserId",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "DriverUserId",
                table: "Rides");

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.CreateIndex(
                name: "IX_Rides_DriverId",
                table: "Rides",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Drivers_DriverId",
                table: "Rides",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Drivers_DriverId",
                table: "Rides");

            migrationBuilder.DropIndex(
                name: "IX_Rides_DriverId",
                table: "Rides");

            migrationBuilder.AlterColumn<double>(
                name: "DriverId",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "DriverUserId",
                table: "Rides",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rides_DriverUserId",
                table: "Rides",
                column: "DriverUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Drivers_DriverUserId",
                table: "Rides",
                column: "DriverUserId",
                principalTable: "Drivers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

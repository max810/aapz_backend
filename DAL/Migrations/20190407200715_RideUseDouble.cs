using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class RideUseDouble : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Drivers_DriverId",
                table: "Rides");

            migrationBuilder.DropIndex(
                name: "IX_Rides_DriverId",
                table: "Rides");

            migrationBuilder.RenameColumn(
                name: "RideId",
                table: "Rides",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Identifier",
                table: "Drivers",
                newName: "IdentifierHashB64");

            migrationBuilder.AlterColumn<double>(
                name: "TalkingSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "SearchSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "RadioSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "NormalDrivingSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "MobileRightSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "MobileRightHeadSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "MobileLeftSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "MobileLeftHeadSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "MakeupSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "DriverId",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "DrinkSeconds",
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

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Rides",
                newName: "RideId");

            migrationBuilder.RenameColumn(
                name: "IdentifierHashB64",
                table: "Drivers",
                newName: "Identifier");

            migrationBuilder.AlterColumn<int>(
                name: "TalkingSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "SearchSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "RadioSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "NormalDrivingSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "MobileRightSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "MobileRightHeadSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "MobileLeftSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "MobileLeftHeadSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "MakeupSeconds",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "Rides",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "DrinkSeconds",
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
    }
}

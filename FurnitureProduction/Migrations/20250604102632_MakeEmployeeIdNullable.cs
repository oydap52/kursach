using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureProduction.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmployeeIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Dimensions",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FurnitureVariant",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductionTimeDays",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WeightPerUnit",
                table: "Materials");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "FurnitureTypeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "BaseProductionTimeDays",
                value: 0);

            migrationBuilder.UpdateData(
                table: "FurnitureTypeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "BaseProductionTimeDays",
                value: 0);

            migrationBuilder.UpdateData(
                table: "FurnitureTypeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "BaseProductionTimeDays",
                value: 0);

            migrationBuilder.UpdateData(
                table: "FurnitureTypeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "BaseProductionTimeDays",
                value: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dimensions",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FurnitureVariant",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProductionTimeDays",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Orders",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightPerUnit",
                table: "Materials",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "FurnitureTypeConfigs",
                keyColumn: "Id",
                keyValue: 1,
                column: "BaseProductionTimeDays",
                value: 10);

            migrationBuilder.UpdateData(
                table: "FurnitureTypeConfigs",
                keyColumn: "Id",
                keyValue: 2,
                column: "BaseProductionTimeDays",
                value: 8);

            migrationBuilder.UpdateData(
                table: "FurnitureTypeConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "BaseProductionTimeDays",
                value: 5);

            migrationBuilder.UpdateData(
                table: "FurnitureTypeConfigs",
                keyColumn: "Id",
                keyValue: 4,
                column: "BaseProductionTimeDays",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 1,
                column: "WeightPerUnit",
                value: 2m);

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 2,
                column: "WeightPerUnit",
                value: 0.5m);

            migrationBuilder.UpdateData(
                table: "Materials",
                keyColumn: "Id",
                keyValue: 3,
                column: "WeightPerUnit",
                value: 10m);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Employees_EmployeeId",
                table: "Orders",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureProduction.Migrations
{
    /// <inheritdoc />
    public partial class AddFurnitureTypeConfigAndOrderDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}

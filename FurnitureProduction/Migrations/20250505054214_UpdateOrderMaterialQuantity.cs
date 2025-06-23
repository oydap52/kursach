using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FurnitureProduction.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderMaterialQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuantityUsed",
                table: "OrderMaterials",
                newName: "Quantity");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "OrderMaterials",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "OrderMaterials");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "OrderMaterials",
                newName: "QuantityUsed");
        }
    }
}

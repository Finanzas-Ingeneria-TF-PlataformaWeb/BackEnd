using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiVivienda.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyTypeToProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PropertyType",
                table: "Properties",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PropertyType",
                table: "Properties");
        }
    }
}

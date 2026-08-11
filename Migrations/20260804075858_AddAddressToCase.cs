using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LongTermCareMatching.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressToCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Cases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Cases");
        }
    }
}

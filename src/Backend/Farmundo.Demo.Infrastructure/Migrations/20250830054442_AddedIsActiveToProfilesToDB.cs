using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Farmundo.Demo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsActiveToProfilesToDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Subscription",
                table: "UserProfiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "Subscription",
                table: "UserProfiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}

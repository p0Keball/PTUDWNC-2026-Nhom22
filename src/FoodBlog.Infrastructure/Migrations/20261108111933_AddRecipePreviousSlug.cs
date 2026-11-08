using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipePreviousSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousSlug",
                table: "Recipes",
                type: "varchar(220)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_PreviousSlug",
                table: "Recipes",
                column: "PreviousSlug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipes_PreviousSlug",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "PreviousSlug",
                table: "Recipes");
        }
    }
}

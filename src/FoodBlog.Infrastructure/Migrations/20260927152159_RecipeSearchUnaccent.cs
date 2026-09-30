using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecipeSearchUnaccent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipes_searchvector ON ""Recipes"";");
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION recipes_searchvector_update() RETURNS trigger AS $$
                BEGIN
                  NEW."SearchVector" := to_tsvector('english',
                    unaccent(coalesce(NEW."Title", '')) || ' ' || unaccent(coalesce(NEW."Description", '')));
                  RETURN NEW;
                END $$ LANGUAGE plpgsql;
                """);
            migrationBuilder.Sql("""
                CREATE TRIGGER trg_recipes_searchvector
                BEFORE INSERT OR UPDATE OF "Title", "Description" ON "Recipes"
                FOR EACH ROW EXECUTE FUNCTION recipes_searchvector_update();
                """);
            migrationBuilder.Sql(@"UPDATE ""Recipes"" SET ""Title"" = ""Title"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipes_searchvector ON ""Recipes"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS recipes_searchvector_update();");
            migrationBuilder.Sql("""
                CREATE TRIGGER trg_recipes_searchvector
                BEFORE INSERT OR UPDATE OF "Title", "Description" ON "Recipes"
                FOR EACH ROW EXECUTE FUNCTION
                tsvector_update_trigger("SearchVector", 'pg_catalog.english', "Title", "Description");
                """);
        }
    }
}

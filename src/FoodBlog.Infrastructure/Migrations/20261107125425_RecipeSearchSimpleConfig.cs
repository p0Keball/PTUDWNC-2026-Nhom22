using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecipeSearchSimpleConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FR-SRCH-001 + Câu hỏi ôn tập #20: tiếng Việt không dùng stemming
            // tiếng Anh, nên cấu hình text search đúng là 'simple'.
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_recipes_searchvector ON ""Recipes"";");
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION recipes_searchvector_update() RETURNS trigger AS $$
                BEGIN
                  NEW."SearchVector" := to_tsvector('simple',
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
    }
}

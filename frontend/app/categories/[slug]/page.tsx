import { notFound } from "next/navigation";
import { getCategoryBySlug } from "@/lib/api";
import { Container } from "@/components/Container";
import { PageHeader } from "@/components/PageHeader";
import RecipeCard from "@/components/RecipeCard";

export const dynamic = "force-dynamic";

export default async function CategoryPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  const data = await getCategoryBySlug(decodeURIComponent(slug));
  if (!data) notFound();
  return (
    <Container>
      <PageHeader title={data.category.name} description={data.category.description ?? "Các công thức trong danh mục này."} />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {data.recipes.items.map((recipe) => <RecipeCard key={recipe.id} recipe={recipe} />)}
      </div>
    </Container>
  );
}

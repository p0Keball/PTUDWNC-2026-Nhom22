import Link from "next/link";
import { getCategories } from "@/lib/api";
import { Container } from "@/components/Container";
import { PageHeader } from "@/components/PageHeader";

export const metadata = { title: "Danh mục công thức" };
export const dynamic = "force-dynamic";

export default async function CategoriesPage() {
  const categories = await getCategories();
  return (
    <Container>
      <PageHeader title="Danh mục công thức" description="Khám phá món ăn theo chủ đề yêu thích." />
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {categories.map((category) => (
          <Link key={category.id} href={`/categories/${encodeURIComponent(category.slug)}`} className="rounded-xl border bg-card p-5 transition hover:-translate-y-0.5 hover:shadow-md">
            <h2 className="font-semibold">{category.name}</h2>
            <p className="mt-2 line-clamp-2 text-sm text-muted-foreground">{category.description ?? "Khám phá các công thức trong danh mục này."}</p>
            <p className="mt-4 text-sm text-primary">{category.recipeCount} công thức →</p>
          </Link>
        ))}
      </div>
    </Container>
  );
}

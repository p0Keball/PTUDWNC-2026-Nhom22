import Link from "next/link";

import { Container } from "@/components/Container";
import { PageHeader } from "@/components/PageHeader";
import { EmptyState } from "@/components/EmptyState";
import { Button } from "@/components/ui/button";
import { getCategories, type Category } from "@/lib/api";

export const revalidate = 3600;

export const metadata = {
  title: "Danh mục công thức",
  description: "Danh sách tất cả danh mục công thức nấu ăn trên FoodBlog.",
};

export default async function CategoriesPage() {
  const categories = await getCategories().catch(() => [] as Category[]);

  return (
    <Container className="py-10">
      <PageHeader
        title="Danh mục công thức"
        description={`Khám phá ${categories.length} danh mục ẩm thực trên FoodBlog.`}
      />

      {categories.length === 0 ? (
        <EmptyState
          title="Chưa có danh mục nào"
          description="Hiện hệ thống chưa có danh mục công thức."
          action={
            <Button asChild variant="outline">
              <Link href="/">Về trang chủ</Link>
            </Button>
          }
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
          {categories.map((category) => (
            <Link
              key={category.id}
              href={`/categories/${encodeURIComponent(category.slug)}`}
              className="group block"
            >
              <div className="overflow-hidden rounded-xl border bg-card p-6 shadow-sm transition hover:shadow-md">
                <div className="flex items-center justify-between mb-2">
                  <h3 className="text-lg font-semibold group-hover:text-primary">
                    {category.name}
                  </h3>
                  <span className="text-xs font-medium rounded-full bg-muted px-2 py-1">
                    {category.recipeCount} công thức
                  </span>
                </div>
                {category.description && (
                  <p className="text-sm text-muted-foreground line-clamp-2 mb-4">
                    {category.description}
                  </p>
                )}
                <div className="flex items-center justify-between pt-4 border-t">
                  <Link
                    href={`/categories/${encodeURIComponent(category.slug)}`}
                    className="text-sm font-medium text-primary hover:underline"
                  >
                    Xem công thức
                  </Link>
                </div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </Container>
  );
}

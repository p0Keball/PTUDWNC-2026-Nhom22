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
        description={`Khám phá ${categories?.length || 0} danh mục ẩm thực trên FoodBlog.`}
      />

      {!categories || categories.length === 0 ? (
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
          {categories.map((category, index) => {
            // ✅ Tạo đường dẫn an toàn, chống bị undefined/null gây crash Link
            const safeSlug = category.slug || category.id || "";
            const categoryHref = safeSlug
              ? `/categories/${encodeURIComponent(safeSlug)}`
              : "/categories";

            return (
              <Link
                key={category.id || category.slug || index}
                href={categoryHref}
                className="group block"
              >
                <div className="overflow-hidden rounded-xl border bg-card p-6 shadow-sm transition hover:shadow-md">
                  <div className="mb-2 flex items-center justify-between">
                    <h3 className="text-lg font-semibold group-hover:text-primary">
                      {category.name || "Danh mục"}
                    </h3>
                    <span className="rounded-full bg-muted px-2 py-1 text-xs font-medium">
                      {category.recipeCount ?? 0} công thức
                    </span>
                  </div>
                  {category.description && (
                    <p className="mb-4 line-clamp-2 text-sm text-muted-foreground">
                      {category.description}
                    </p>
                  )}
                  <div className="flex items-center justify-between border-t pt-4">
                    {/* ✅ Đổi từ <Link> sang <span> để tránh lỗi lồng thẻ <Link> gây crash */}
                    <span className="text-sm font-medium text-primary group-hover:underline">
                      Xem công thức &rarr;
                    </span>
                  </div>
                </div>
              </Link>
            );
          })}
        </div>
      )}
    </Container>
  );
}
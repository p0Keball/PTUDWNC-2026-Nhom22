import Link from "next/link";
import { notFound } from "next/navigation";

import { Container } from "@/components/Container";
import { PageHeader } from "@/components/PageHeader";
import { ErrorState } from "@/components/ErrorState";
import { EmptyState } from "@/components/EmptyState";
import RecipeCard from "@/components/RecipeCard";
import Pagination from "@/components/Pagination";
import { Button } from "@/components/ui/button";
import {
  getCategoryBySlug,
  type CategoryDetail,
} from "@/lib/api";

export const dynamic = "force-dynamic";

export async function generateMetadata({
  params,
}: { params: Promise<{ slug: string }> }) {
  const { slug } = await params;
  return {
    title: `Danh mục: ${slug}`,
    description: `Xem danh sách công thức trong danh mục ${slug}.`,
  };
}

export default async function CategoryDetailPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  const decodedSlug = decodeURIComponent(slug);

  let data: CategoryDetail | null;
  try {
    data = await getCategoryBySlug(decodedSlug).catch(() => null);
  } catch {
    return (
      <Container>
        <PageHeader title="Danh mục công thức" />
        <ErrorState
          title="Không tải được danh mục"
          message="Máy chủ dữ liệu có thể chưa sẵn sàng. Vui lòng thử lại sau."
          retryHref={`/categories/${decodedSlug}`}
          homeHref="/"
        />
      </Container>
    );
  }

  if (!data) notFound();

  return (
    <Container className="py-10">
      <PageHeader
        title={data.category.name}
        description={data.category.description ?? `Danh mục ${data.category.name}`}
        actions={
          <Link href="/categories" className="text-sm font-medium hover:underline">
            ← Quay lại danh sách danh mục
          </Link>
        }
      />

      <p className="text-sm text-muted-foreground mb-6">
        {data.recipes.totalCount} công thức trong danh mục này.
      </p>

      {data.recipes.items.length === 0 ? (
        <EmptyState
          title="Chưa có công thức nào"
          description="Danh mục này chưa có công thức được xuất bản."
          action={
            <Button asChild variant="outline">
              <Link href="/recipes">Xem tất cả công thức</Link>
            </Button>
          }
        />
      ) : (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {data.recipes.items.map((recipe) => (
              <RecipeCard key={recipe.id} recipe={recipe} showStatus />
            ))}
          </div>

          <Pagination
            page={data.recipes.page}
            totalPages={Math.ceil(data.recipes.totalCount / data.recipes.pageSize)}
            basePath={`/categories/${decodedSlug}`}
            query={{ page: data.recipes.page }}
          />
        </>
      )}
    </Container>
  );
}

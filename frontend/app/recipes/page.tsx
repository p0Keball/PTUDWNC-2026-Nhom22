import Link from "next/link";
import { SearchX } from "lucide-react";

import { Container } from "@/components/Container";
import { EmptyState } from "@/components/EmptyState";
import { ErrorState } from "@/components/ErrorState";
import { PageHeader } from "@/components/PageHeader";
import Pagination from "@/components/Pagination";
import RecipeCard from "@/components/RecipeCard";
import RecipeFilterBar from "@/components/RecipeFilterBar";
import { Button } from "@/components/ui/button";
import {
  getCategories,
  getRecipes,
  type RecipeListParams,
  type RecipeListResponse,
} from "@/lib/api";

export const dynamic = "force-dynamic";

export const metadata = {
  title: "Tất cả công thức",
  description:
    "Danh sách công thức nấu ăn với bộ lọc theo danh mục, độ khó, thời gian nấu và sắp xếp.",
};

interface RecipesPageProps {
  searchParams: Promise<{
    page?: string;
    categoryId?: string;
    difficulty?: string;
    maxCookTime?: string;
    sort?: string;
  }>;
}

export default async function RecipesPage({ searchParams }: RecipesPageProps) {
  const params = await searchParams;
  const page = Math.max(Number.parseInt(params.page ?? "1", 10) || 1, 1);
  const pageSize = 12;

  const filters: RecipeListParams = {
    categoryId: params.categoryId || undefined,
    difficulty: params.difficulty
      ? Number.parseInt(params.difficulty, 10)
      : undefined,
    maxCookTime: params.maxCookTime
      ? Number.parseInt(params.maxCookTime, 10)
      : undefined,
    sort: params.sort || undefined,
  };

  const categoriesPromise = getCategories().catch(() => []);

  let data: RecipeListResponse;
  try {
    data = await getRecipes(page, pageSize, filters, { noStore: true });
  } catch {
    return (
      <Container>
        <PageHeader
          title="Tất cả công thức"
          description="Danh sách công thức nấu ăn dành cho mọi người."
        />
        <ErrorState
          title="Không tải được công thức"
          message="Máy chủ dữ liệu có thể chưa sẵn sàng. Vui lòng thử lại sau."
          retryHref="/recipes"
          homeHref="/"
        />
      </Container>
    );
  }

  const categories = await categoriesPromise;
  const totalPages = Math.max(Math.ceil(data.totalCount / data.pageSize), 1);

  return (
    <Container>
      <PageHeader
        title="Tất cả công thức"
        description={`${data.totalCount} công thức đã xuất bản. Lọc theo danh mục, độ khó hoặc thời gian nấu để tìm món phù hợp.`}
      />

      <RecipeFilterBar
        categories={categories}
        initialFilters={{
          categoryId: params.categoryId,
          difficulty: params.difficulty,
          maxCookTime: params.maxCookTime,
          sort: params.sort,
        }}
        totalCount={data.totalCount}
      />

      {data.items.length === 0 ? (
        <EmptyState
          icon={<SearchX className="size-6" />}
          title="Không tìm thấy công thức phù hợp"
          description="Thử thay đổi bộ lọc hoặc xem tất cả công thức hiện có."
          action={
            <Button asChild variant="outline">
              <Link href="/recipes">Xem tất cả công thức</Link>
            </Button>
          }
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {data.items.map((recipe) => (
            <RecipeCard key={recipe.id} recipe={recipe} />
          ))}
        </div>
      )}

      <Pagination
        page={data.page}
        totalPages={totalPages}
        basePath="/recipes"
        query={{
          categoryId: filters.categoryId,
          difficulty: filters.difficulty,
          maxCookTime: filters.maxCookTime,
          sort: filters.sort,
        }}
      />
    </Container>
  );
}

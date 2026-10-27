import Link from "next/link";
import { Filter, SearchX } from "lucide-react";

import { Container } from "@/components/Container";
import { EmptyState } from "@/components/EmptyState";
import { ErrorState } from "@/components/ErrorState";
import { PageHeader } from "@/components/PageHeader";
import Pagination from "@/components/Pagination";
import RecipeCard from "@/components/RecipeCard";
import { Button } from "@/components/ui/button";
import { Select } from "@/components/ui/select";
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

const DIFFICULTIES = [
  { value: "1", label: "Dễ" },
  { value: "2", label: "Trung bình" },
  { value: "3", label: "Khó" },
  { value: "4", label: "Chuyên gia" },
];

const COOK_TIMES = [
  { value: "15", label: "≤ 15 phút" },
  { value: "30", label: "≤ 30 phút" },
  { value: "45", label: "≤ 45 phút" },
  { value: "60", label: "≤ 60 phút" },
  { value: "120", label: "≤ 120 phút" },
];

const SORTS = [
  { value: "-createdAt", label: "Mới nhất" },
  { value: "createdAt", label: "Cũ nhất" },
  { value: "title", label: "Tên A → Z" },
  { value: "-title", label: "Tên Z → A" },
];

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
  const hasFilters = Object.values(filters).some(Boolean);

  return (
    <Container>
      <PageHeader
        title="Tất cả công thức"
        description={`${data.totalCount} công thức đã xuất bản. Lọc theo danh mục, độ khó hoặc thời gian nấu để tìm món phù hợp.`}
      />

      <form
        action="/recipes"
        className="grid gap-3 rounded-xl border bg-card p-4 shadow-sm sm:grid-cols-2 lg:grid-cols-5"
      >
        <div className="flex flex-col gap-1.5">
          <label htmlFor="categoryId" className="text-xs font-medium text-muted-foreground">
            Danh mục
          </label>
          <Select
            id="categoryId"
            name="categoryId"
            defaultValue={filters.categoryId ?? ""}
          >
            <option value="">Tất cả danh mục</option>
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </Select>
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="difficulty" className="text-xs font-medium text-muted-foreground">
            Độ khó
          </label>
          <Select
            id="difficulty"
            name="difficulty"
            defaultValue={filters.difficulty?.toString() ?? ""}
          >
            <option value="">Tất cả độ khó</option>
            {DIFFICULTIES.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </Select>
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="maxCookTime" className="text-xs font-medium text-muted-foreground">
            Thời gian nấu
          </label>
          <Select
            id="maxCookTime"
            name="maxCookTime"
            defaultValue={filters.maxCookTime?.toString() ?? ""}
          >
            <option value="">Không giới hạn</option>
            {COOK_TIMES.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </Select>
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="sort" className="text-xs font-medium text-muted-foreground">
            Sắp xếp
          </label>
          <Select
            id="sort"
            name="sort"
            defaultValue={filters.sort ?? "-createdAt"}
          >
            {SORTS.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </Select>
        </div>

        <div className="flex items-end gap-2">
          <Button type="submit" className="flex-1">
            <Filter />
            Lọc
          </Button>
          {hasFilters && (
            <Button asChild variant="outline">
              <Link href="/recipes">Xóa lọc</Link>
            </Button>
          )}
        </div>
      </form>

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

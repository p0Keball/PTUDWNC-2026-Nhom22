import Link from "next/link";
import { Search } from "lucide-react";

import { Container } from "@/components/Container";
import { PageHeader } from "@/components/PageHeader";
import { EmptyState } from "@/components/EmptyState";
import RecipeCard from "@/components/RecipeCard";
import Pagination from "@/components/Pagination";
import {
  searchRecipes,
  getCategories,
  type Category,
} from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";

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

const SERVINGS = [
  { value: "1", label: "≥ 1 khẩu phần" },
  { value: "2", label: "≥ 2 khẩu phần" },
  { value: "4", label: "≥ 4 khẩu phần" },
  { value: "6", label: "≥ 6 khẩu phần" },
  { value: "8", label: "≥ 8 khẩu phần" },
];

const SORTS = [
  { value: "-createdAt", label: "Mới nhất" },
  { value: "createdAt", label: "Cũ nhất" },
  { value: "title", label: "Tên A → Z" },
  { value: "-title", label: "Tên Z → A" },
];

export const dynamic = "force-dynamic";

export async function generateMetadata({
  searchParams,
}: { searchParams: Promise<{ q?: string }> }) {
  const params = await searchParams;
  const q = params.q;
  return {
    title: q ? `Tìm kiếm: ${q}` : "Tìm kiếm công thức",
    description: q
      ? `Kết quả tìm kiếm cho "${q}"`
      : "Tìm kiếm công thức nấu ăn trên FoodBlog.",
  };
}

interface SearchPageProps {
  searchParams: Promise<{
    q?: string;
    page?: string;
    categoryId?: string;
    difficulty?: string;
    maxCookTime?: string;
    minServings?: string;
    sort?: string;
  }>;
}

export default async function SearchPage({
  searchParams,
}: SearchPageProps) {
  const params = await searchParams;
  const page = Math.max(Number.parseInt(params.page ?? "1", 10) || 1, 1);
  const pageSize = 12;

  const q = params.q ?? "";
  const categoryId = params.categoryId ?? "";
  const difficulty = params.difficulty ?? "";
  const maxCookTime = params.maxCookTime ?? "";
  const minServings = params.minServings ?? "";
  const sort = params.sort ?? "-createdAt";

  const hasFilters =
    Boolean(q || categoryId || difficulty || maxCookTime || minServings) ||
    sort !== "-createdAt";

  const [data, categories] = await Promise.all([
    q
      ? searchRecipes({
          q,
          page,
          pageSize,
          categoryId: categoryId || undefined,
          difficulty: difficulty || undefined,
          maxCookTime: maxCookTime ? Number.parseInt(maxCookTime, 10) : undefined,
          minServings: minServings ? Number.parseInt(minServings, 10) : undefined,
          sort,
        }).catch(() => null)
      : null,
    getCategories().catch(() => [] as Category[]),
  ]);

  const totalPages = data ? Math.max(Math.ceil(data.totalCount / data.pageSize), 1) : 1;

  return (
    <Container className="py-10">
      <PageHeader
        title={q ? `Kết quả tìm kiếm: "${q}"` : "Tìm kiếm công thức"}
        description={
          q
            ? `Tìm thấy ${data?.totalCount ?? 0} công thức phù hợp.`
            : "Nhập từ khóa để tìm công thức nấu ăn bạn muốn."
        }
      />

      <form
        action="/search"
        className="grid gap-3 rounded-xl border bg-card p-4 shadow-sm sm:grid-cols-2 lg:grid-cols-4 mb-6"
      >
        <div className="flex flex-col gap-1.5 lg:col-span-2">
          <label htmlFor="q" className="text-xs font-medium text-muted-foreground">
            Tìm kiếm
          </label>
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              id="q"
              defaultValue={q}
              name="q"
              placeholder="Tên món ăn, nguyên liệu…"
              className="pl-10 w-full"
            />
          </div>
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="categoryId" className="text-xs font-medium text-muted-foreground">
            Danh mục
          </label>
          <Select
            id="categoryId"
            name="categoryId"
            defaultValue={categoryId}
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
            defaultValue={difficulty}
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
            defaultValue={maxCookTime}
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
          <label htmlFor="minServings" className="text-xs font-medium text-muted-foreground">
            Khẩu phần
          </label>
          <Select
            id="minServings"
            name="minServings"
            defaultValue={minServings}
          >
            <option value="">Tất cả khẩu phần</option>
            {SERVINGS.map((item) => (
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
            defaultValue={sort}
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
            <Search size={18} />
            Tìm kiếm
          </Button>
          {hasFilters && (
            <Button asChild variant="outline">
              <Link href="/search">Xóa lọc</Link>
            </Button>
          )}
        </div>
      </form>

      {!q && (
        <div className="text-center py-12">
          <div className="flex flex-col items-center gap-3">
            <Search className="size-12 text-muted-foreground" />
            <p className="text-sm text-muted-foreground">
              Nhập từ khóa vào ô tìm kiếm ở trên để bắt đầu khám phá công thức.
            </p>
          </div>
          <Link href="/recipes" className="text-sm font-medium text-primary hover:underline">
            Xem tất cả công thức
          </Link>
        </div>
      )}

      {q && data && data.items.length === 0 && (
        <EmptyState
          icon={<Search className="size-6" />}
          title="Không tìm thấy công thức phù hợp"
          description="Thử thay đổi từ khóa hoặc bộ lọc tìm kiếm."
          action={
            <Button asChild variant="outline">
              <Link href="/search">Xóa lọc</Link>
            </Button>
          }
        />
      )}

      {q && data && data.items.length > 0 && (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {data.items.map((recipe) => (
              <RecipeCard key={recipe.id} recipe={recipe} />
            ))}
          </div>

          <Pagination
            page={data.page}
            totalPages={totalPages}
            basePath="/search"
            query={{
              q,
              categoryId,
              difficulty,
              maxCookTime,
              minServings,
              sort,
            }}
          />
        </>
      )}
    </Container>
  );
}

import Link from "next/link";
import RecipeCard from "@/components/RecipeCard";
import { getRecipes } from "@/lib/api";
import { getCategories } from "@/lib/api";
import { Container } from "@/components/Container";

export const metadata = {
  title: "Culinary Blog — Công thức nấu ăn",
  description: "Khám phá công thức nấu ăn mới nhất.",
};
export const dynamic = "force-dynamic";

export default async function Home({
  searchParams,
}: {
  searchParams: Promise<{ page?: string }>;
}) {
  const { page: pageParam } = await searchParams;
  const page = Math.max(parseInt(pageParam ?? "1", 10) || 1, 1);
  const pageSize = 12;

  const [data, categories] = await Promise.all([getRecipes(page, pageSize), getCategories()]);
  const totalPages = Math.max(Math.ceil(data.totalCount / data.pageSize), 1);

  return (
    <Container className="max-w-7xl lg:flex-row lg:items-start">
      <aside className="hidden w-56 shrink-0 lg:block">
        <div className="sticky top-24 rounded-xl border bg-card p-4">
          <h2 className="font-semibold">Danh mục</h2>
          <nav className="mt-3 flex flex-col gap-2 text-sm">
            {categories.slice(0, 10).map((category) => (
              <Link key={category.id} href={`/categories/${encodeURIComponent(category.slug)}`} className="flex justify-between hover:text-primary">
                <span>{category.name}</span><span className="text-muted-foreground">{category.recipeCount}</span>
              </Link>
            ))}
            <Link href="/categories" className="mt-2 text-primary hover:underline">Xem tất cả →</Link>
          </nav>
        </div>
      </aside>
      <main className="min-w-0 flex-1">
      <div className="flex items-center justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-bold sm:text-3xl">Công thức mới nhất</h1>
          <p className="text-sm text-zinc-500">
            {data.totalCount} công thức đã xuất bản
          </p>
        </div>
        <div className="flex shrink-0 items-center gap-2">
          <Link
            href="/login"
            className="rounded-full border border-zinc-200 px-4 py-2 text-sm font-medium hover:bg-zinc-100 dark:border-zinc-700 dark:hover:bg-zinc-800"
          >
            Đăng nhập
          </Link>
          <Link
            href="/profile"
            className="rounded-full bg-zinc-900 px-4 py-2 text-sm font-medium text-white hover:bg-zinc-700 dark:bg-zinc-100 dark:text-zinc-900 dark:hover:bg-white"
          >
            Hồ sơ
          </Link>
          <Link
            href="/recipes/create"
            className="hidden rounded-full bg-orange-500 px-4 py-2 text-sm font-medium text-white hover:bg-orange-600 sm:inline-block"
          >
            Tạo công thức
          </Link>
        </div>
      </div>

      {data.items.length === 0 ? (
        <p className="text-zinc-500">Chưa có công thức nào.</p>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {data.items.map((r) => (
            <RecipeCard key={r.id} recipe={r} />
          ))}
        </div>
      )}

      <div className="flex items-center justify-between pt-2 text-sm">
        <span>
          Trang {data.page} / {totalPages}
        </span>
        <div className="flex gap-2">
          {page > 1 && (
            <Link
              href={`/?page=${page - 1}`}
              className="rounded-full border px-4 py-2 hover:bg-zinc-100 dark:hover:bg-zinc-800"
            >
              Trước
            </Link>
          )}
          {page < totalPages && (
            <Link
              href={`/?page=${page + 1}`}
              className="rounded-full border px-4 py-2 hover:bg-zinc-100 dark:hover:bg-zinc-800"
            >
              Sau
            </Link>
          )}
        </div>
      </div>
      </main>
    </Container>
  );
}

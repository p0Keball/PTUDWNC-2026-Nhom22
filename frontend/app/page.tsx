import Link from "next/link";
import RecipeCard from "@/components/RecipeCard";
import { getRecipes } from "@/lib/api";

export const metadata = {
  title: "Culinary Blog — Công thức nấu ăn",
  description: "Khám phá công thức nấu ăn mới nhất.",
};

export default async function Home({
  searchParams,
}: {
  searchParams: Promise<{ page?: string }>;
}) {
  const { page: pageParam } = await searchParams;
  const page = Math.max(parseInt(pageParam ?? "1", 10) || 1, 1);
  const pageSize = 12;

  const data = await getRecipes(page, pageSize);
  const totalPages = Math.max(Math.ceil(data.totalCount / data.pageSize), 1);

  return (
    <main className="mx-auto flex w-full max-w-6xl flex-1 flex-col gap-6 p-4 sm:p-6">
      <div className="flex items-center justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-bold sm:text-3xl">Công thức mới nhất</h1>
          <p className="text-sm text-zinc-500">
            {data.totalCount} công thức đã xuất bản
          </p>
        </div>
        <Link
          href="/recipes/create"
          className="shrink-0 rounded-full bg-zinc-900 px-4 py-2 text-sm font-medium text-white hover:bg-zinc-700 dark:bg-zinc-100 dark:text-zinc-900 dark:hover:bg-white"
        >
          Tạo công thức
        </Link>
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
  );
}

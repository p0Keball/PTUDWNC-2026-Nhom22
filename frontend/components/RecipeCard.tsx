import Link from "next/link";
import type { RecipeSummary } from "@/lib/api";

export default function RecipeCard({ recipe }: { recipe: RecipeSummary }) {
  return (
    <Link
      href={`/recipes/${encodeURIComponent(recipe.slug)}`}
      className="group flex flex-col overflow-hidden rounded-xl border border-zinc-200 bg-white transition hover:shadow-md dark:border-zinc-800 dark:bg-zinc-900"
    >
      <div className="aspect-[16/10] w-full overflow-hidden bg-zinc-100 dark:bg-zinc-800">
        {recipe.primaryImageUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={recipe.primaryImageUrl}
            alt={recipe.title}
            loading="lazy"
            className="h-full w-full object-cover transition group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-sm text-zinc-400">
            Chưa có ảnh
          </div>
        )}
      </div>
      <div className="flex flex-1 flex-col gap-2 p-4">
        <h3 className="line-clamp-2 font-semibold leading-snug group-hover:underline">
          {recipe.title}
        </h3>
        <p className="line-clamp-2 text-sm text-zinc-500 dark:text-zinc-400">
          {recipe.description}
        </p>
        <div className="mt-auto flex flex-wrap gap-2 pt-2 text-xs text-zinc-500 dark:text-zinc-400">
          <span className="rounded-full bg-zinc-100 px-2 py-1 dark:bg-zinc-800">
            {recipe.difficulty}
          </span>
          <span className="rounded-full bg-zinc-100 px-2 py-1 dark:bg-zinc-800">
            {recipe.prepTimeMinutes + recipe.cookTimeMinutes} phút
          </span>
          <span className="rounded-full bg-zinc-100 px-2 py-1 dark:bg-zinc-800">
            {recipe.servings} khẩu phần
          </span>
        </div>
      </div>
    </Link>
  );
}

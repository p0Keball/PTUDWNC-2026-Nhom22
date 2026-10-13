export function RecipeCardSkeleton() {
  return (
    <div className="flex animate-pulse flex-col overflow-hidden rounded-xl border border-zinc-200 dark:border-zinc-800">
      <div className="aspect-[16/10] w-full bg-zinc-200 dark:bg-zinc-800" />
      <div className="flex flex-col gap-2 p-4">
        <div className="h-5 w-3/4 rounded bg-zinc-200 dark:bg-zinc-800" />
        <div className="h-4 w-full rounded bg-zinc-200 dark:bg-zinc-800" />
        <div className="h-4 w-2/3 rounded bg-zinc-200 dark:bg-zinc-800" />
        <div className="flex gap-2 pt-2">
          <div className="h-6 w-16 rounded-full bg-zinc-200 dark:bg-zinc-800" />
          <div className="h-6 w-16 rounded-full bg-zinc-200 dark:bg-zinc-800" />
        </div>
      </div>
    </div>
  );
}

export function RecipeListSkeleton({ count = 6 }: { count?: number }) {
  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: count }).map((_, i) => (
        <RecipeCardSkeleton key={i} />
      ))}
    </div>
  );
}

export function RecipeDetailSkeleton() {
  return (
    <div className="mx-auto flex w-full max-w-3xl animate-pulse flex-col gap-4 p-4">
      <div className="h-8 w-2/3 rounded bg-zinc-200 dark:bg-zinc-800" />
      <div className="aspect-[16/9] w-full rounded-xl bg-zinc-200 dark:bg-zinc-800" />
      <div className="h-4 w-full rounded bg-zinc-200 dark:bg-zinc-800" />
      <div className="h-4 w-5/6 rounded bg-zinc-200 dark:bg-zinc-800" />
      <div className="h-4 w-4/6 rounded bg-zinc-200 dark:bg-zinc-800" />
    </div>
  );
}

export function RecipeFormSkeleton() {
  return (
    <div className="flex animate-pulse flex-col gap-4" aria-label="Đang tải biểu mẫu">
      <div className="flex flex-col gap-1.5">
        <div className="h-4 w-20 rounded bg-zinc-200 dark:bg-zinc-800" />
        <div className="h-10 w-full rounded-lg bg-zinc-200 dark:bg-zinc-800" />
      </div>
      <div className="flex flex-col gap-1.5">
        <div className="h-4 w-24 rounded bg-zinc-200 dark:bg-zinc-800" />
        <div className="h-10 w-full rounded-lg bg-zinc-200 dark:bg-zinc-800" />
      </div>
      <div className="flex flex-col gap-1.5">
        <div className="h-4 w-16 rounded bg-zinc-200 dark:bg-zinc-800" />
        <div className="h-20 w-full rounded-lg bg-zinc-200 dark:bg-zinc-800" />
      </div>
      <div className="flex flex-col gap-1.5">
        <div className="h-4 w-28 rounded bg-zinc-200 dark:bg-zinc-800" />
        <div className="h-32 w-full rounded-lg bg-zinc-200 dark:bg-zinc-800" />
      </div>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className="h-10 w-full rounded-lg bg-zinc-200 dark:bg-zinc-800" />
        ))}
      </div>
      <div className="h-11 w-full rounded-lg bg-zinc-200 dark:bg-zinc-800" />
    </div>
  );
}

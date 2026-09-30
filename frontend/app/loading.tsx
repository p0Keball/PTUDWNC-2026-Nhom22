import { RecipeListSkeleton } from "@/components/RecipeSkeleton";

export default function Loading() {
  return (
    <main className="mx-auto flex w-full max-w-6xl flex-1 flex-col gap-6 p-4 sm:p-6">
      <div className="h-8 w-48 animate-pulse rounded bg-zinc-200 dark:bg-zinc-800" />
      <RecipeListSkeleton count={6} />
    </main>
  );
}

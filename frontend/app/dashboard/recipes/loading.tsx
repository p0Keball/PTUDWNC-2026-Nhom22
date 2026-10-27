import { RecipeListSkeleton } from "@/components/RecipeSkeleton";
import { Skeleton } from "@/components/ui/skeleton";

export default function Loading() {
  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-2">
        <Skeleton className="h-9 w-64" />
        <Skeleton className="h-4 w-56" />
      </div>
      <Skeleton className="h-9 w-80 max-w-full" />
      <RecipeListSkeleton count={6} />
    </div>
  );
}

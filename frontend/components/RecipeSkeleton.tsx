import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";

const FORM_LABEL_WIDTHS = ["w-20", "w-24", "w-16", "w-28"];

export function RecipeCardSkeleton() {
  return (
    <div className="flex flex-col overflow-hidden rounded-xl border bg-card">
      <Skeleton className="aspect-[16/10] w-full rounded-none" />
      <div className="flex flex-col gap-2 p-4">
        <Skeleton className="h-5 w-3/4" />
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-2/3" />
        <div className="flex gap-2 pt-2">
          <Skeleton className="h-5 w-16 rounded-full" />
          <Skeleton className="h-5 w-16 rounded-full" />
        </div>
      </div>
    </div>
  );
}

export function RecipeListSkeleton({ count = 6 }: { count?: number }) {
  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: count }).map((_, index) => (
        <RecipeCardSkeleton key={index} />
      ))}
    </div>
  );
}

export function RecipeDetailSkeleton() {
  return (
    <div className="mx-auto flex w-full max-w-4xl flex-col gap-4 p-4 sm:p-6">
      <Skeleton className="h-5 w-40" />
      <Skeleton className="h-9 w-2/3" />
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-5/6" />
      <Skeleton className="aspect-[16/9] w-full rounded-xl" />
      <div className="flex gap-2">
        <Skeleton className="h-7 w-20 rounded-full" />
        <Skeleton className="h-7 w-20 rounded-full" />
        <Skeleton className="h-7 w-20 rounded-full" />
      </div>
      <Skeleton className="h-4 w-full" />
      <Skeleton className="h-4 w-4/6" />
    </div>
  );
}

export function RecipeFormSkeleton() {
  return (
    <div className="flex flex-col gap-4" aria-label="Đang tải biểu mẫu">
      {FORM_LABEL_WIDTHS.map((width, index) => (
        <div key={width} className="flex flex-col gap-1.5">
          <Skeleton className={cn("h-4", width)} />
          <Skeleton
            className={index === 2 || index === 3 ? "h-24 w-full" : "h-10 w-full"}
          />
        </div>
      ))}
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        {Array.from({ length: 4 }).map((_, index) => (
          <Skeleton key={index} className="h-10 w-full" />
        ))}
      </div>
      <Skeleton className="h-11 w-full" />
    </div>
  );
}

import { Container } from "@/components/Container";
import { RecipeListSkeleton } from "@/components/RecipeSkeleton";
import { Skeleton } from "@/components/ui/skeleton";

export default function Loading() {
  return (
    <Container className="gap-6 lg:flex-row lg:items-start">
      <div className="flex w-full flex-col gap-3 lg:w-56">
        <Skeleton className="h-16 w-full rounded-xl" />
        <Skeleton className="h-9 w-full" />
        <Skeleton className="h-9 w-full" />
        <Skeleton className="h-9 w-full" />
      </div>
      <div className="flex w-full flex-col gap-4">
        <Skeleton className="h-9 w-64" />
        <Skeleton className="h-4 w-96 max-w-full" />
        <RecipeListSkeleton count={6} />
      </div>
    </Container>
  );
}

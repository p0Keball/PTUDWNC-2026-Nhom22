import { Container } from "@/components/Container";
import { RecipeListSkeleton } from "@/components/RecipeSkeleton";
import { Skeleton } from "@/components/ui/skeleton";

export default function Loading() {
  return (
    <Container>
      <div className="flex flex-col gap-2">
        <Skeleton className="h-8 w-64" />
        <Skeleton className="h-4 w-full max-w-2xl" />
      </div>
      <Skeleton className="h-28 w-full rounded-xl" />
      <RecipeListSkeleton count={6} />
    </Container>
  );
}

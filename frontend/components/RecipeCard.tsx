import Link from "next/link";
import { Clock, ImageOff, Users } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import type { RecipeSummary } from "@/lib/api";
import { difficultyLabel, statusLabel, totalTime } from "@/lib/utils";

interface RecipeCardProps {
  recipe: RecipeSummary;
  showStatus?: boolean;
}

export default function RecipeCard({ recipe, showStatus = false }: RecipeCardProps) {
  return (
    <Card className="group gap-0 overflow-hidden p-0 transition hover:-translate-y-0.5 hover:shadow-md">
      <Link
        href={`/recipes/${encodeURIComponent(recipe.slug)}`}
        className="flex flex-1 flex-col"
      >
        <div className="relative aspect-[16/10] w-full overflow-hidden bg-muted">
          {recipe.primaryImageUrl ? (
            // Ảnh có thể đến từ MinIO với domain thay đổi theo môi trường.
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={recipe.primaryImageUrl}
              alt={recipe.title}
              loading="lazy"
              className="h-full w-full object-cover transition duration-300 group-hover:scale-105"
            />
          ) : (
            <div className="flex h-full w-full flex-col items-center justify-center gap-2 text-muted-foreground">
              <ImageOff className="size-6" />
              <span className="text-xs">Chưa có ảnh</span>
            </div>
          )}
          <div className="absolute left-3 top-3 flex flex-wrap gap-1.5">
            <Badge variant="secondary" className="bg-background/90 backdrop-blur">
              {difficultyLabel(recipe.difficulty)}
            </Badge>
            {showStatus && (
              <Badge variant="outline" className="bg-background/90 backdrop-blur">
                {statusLabel(recipe.status)}
              </Badge>
            )}
          </div>
        </div>

        <div className="flex flex-1 flex-col gap-2 p-4">
          <h3 className="line-clamp-2 text-base font-semibold leading-snug group-hover:text-primary">
            {recipe.title}
          </h3>
          <p className="line-clamp-2 text-sm text-muted-foreground">
            {recipe.description}
          </p>
          <div className="mt-auto flex flex-wrap items-center gap-x-4 gap-y-1 pt-2 text-xs text-muted-foreground">
            <span className="inline-flex items-center gap-1">
              <Clock className="size-3.5" />
              {totalTime(recipe.prepTimeMinutes, recipe.cookTimeMinutes)}
            </span>
            <span className="inline-flex items-center gap-1">
              <Users className="size-3.5" />
              {recipe.servings} khẩu phần
            </span>
          </div>
        </div>
      </Link>
    </Card>
  );
}

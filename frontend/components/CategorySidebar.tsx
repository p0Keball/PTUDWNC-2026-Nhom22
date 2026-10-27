import Link from "next/link";
import { ArrowRight, FolderOpen } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import type { Category } from "@/lib/api";
import { cn } from "@/lib/utils";

interface CategorySidebarProps {
  categories: Category[];
  totalCount?: number;
  activeCategoryId?: string;
  className?: string;
}

export default function CategorySidebar({
  categories,
  totalCount,
  activeCategoryId,
  className,
}: CategorySidebarProps) {
  return (
    <Card className={cn("gap-0 overflow-hidden py-0", className)}>
      <CardHeader className="border-b bg-muted/30 px-4 py-3">
        <CardTitle className="flex items-center justify-between gap-2 text-sm">
          <span className="inline-flex items-center gap-2">
            <FolderOpen className="size-4" />
            Danh mục
          </span>
          {typeof totalCount === "number" && (
            <Badge variant="secondary" className="tabular-nums">
              {totalCount} công thức
            </Badge>
          )}
        </CardTitle>
      </CardHeader>

      <CardContent className="p-2">
        {categories.length === 0 ? (
          <p className="px-3 py-4 text-sm text-muted-foreground">
            Chưa có danh mục nào.
          </p>
        ) : (
          <nav aria-label="Danh mục công thức">
            <ul className="flex flex-col gap-0.5">
              {categories.map((category) => {
                const active = activeCategoryId === category.id;
                return (
                  <li key={category.id}>
                    <Link
                      href={`/recipes?categoryId=${encodeURIComponent(category.id)}`}
                      aria-current={active ? "page" : undefined}
                      className={cn(
                        "flex items-center justify-between gap-2 rounded-lg px-3 py-2 text-sm transition-colors hover:bg-accent hover:text-accent-foreground",
                        active &&
                          "bg-secondary font-medium text-secondary-foreground"
                      )}
                    >
                      <span className="truncate">{category.name}</span>
                      <Badge
                        variant={active ? "default" : "secondary"}
                        className="tabular-nums"
                      >
                        {category.recipeCount}
                      </Badge>
                    </Link>
                  </li>
                );
              })}
            </ul>
          </nav>
        )}
      </CardContent>

      <div className="border-t p-2">
        <Button asChild variant="ghost" className="w-full justify-between">
          <Link href="/recipes">
            Tất cả công thức
            <ArrowRight />
          </Link>
        </Button>
      </div>
    </Card>
  );
}

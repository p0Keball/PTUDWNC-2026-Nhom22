import Link from "next/link";
import { ArrowRight, BookOpen, ChefHat, Clock3, Sparkles } from "lucide-react";

import { Container } from "@/components/Container";
import { EmptyState } from "@/components/EmptyState";
import RecipeCard from "@/components/RecipeCard";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { getCategories, getRecipes } from "@/lib/api";

export const revalidate = 3600;

export const metadata = {
  title: "Culinary Blog — Công thức nấu ăn",
  description:
    "Khám phá công thức nấu ăn mới nhất, được chia sẻ bởi cộng đồng yêu bếp.",
};

export default async function HomePage() {
  const [recipes, categories] = await Promise.all([
    // Build/SSR vẫn chạy được khi Backend chưa khởi động.
    getRecipes(1, 6, {}, { revalidate: 3600 }).catch(() => ({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 6,
    })),
    getCategories().catch(() => []),
  ]);

  return (
    <>
      <section className="border-b bg-gradient-to-b from-muted/60 to-background">
        <Container className="gap-8 py-14 sm:py-20">
          <div className="flex max-w-3xl flex-col gap-4">
            <Badge variant="secondary" className="w-fit gap-1.5">
              <Sparkles className="size-3.5" />
              Blog Ẩm thực &amp; Nấu ăn
            </Badge>
            <h1 className="text-4xl font-bold tracking-tight sm:text-5xl">
              Nấu món ngon mỗi ngày,{" "}
              <span className="text-primary">bắt đầu từ một công thức.</span>
            </h1>
            <p className="max-w-2xl text-base text-muted-foreground sm:text-lg">
              Khám phá hàng trăm công thức từ nhiều nền ẩm thực, xem nguyên liệu,
              các bước thực hiện và thông tin dinh dưỡng — tất cả ở một nơi.
            </p>
            <div className="flex flex-wrap items-center gap-3 pt-2">
              <Button asChild size="lg">
                <Link href="/recipes">
                  Khám phá công thức
                  <ArrowRight />
                </Link>
              </Button>
              <Button asChild size="lg" variant="outline">
                <Link href="/dashboard/recipes/new">
                  <ChefHat />
                  Chia sẻ công thức
                </Link>
              </Button>
            </div>
          </div>

          <div className="grid gap-3 sm:grid-cols-3">
            {[
              {
                icon: BookOpen,
                label: "Công thức đa dạng",
                value: `${recipes.totalCount}+`,
                note: "Món ăn đang được chia sẻ",
              },
              {
                icon: Clock3,
                label: "Thời gian rõ ràng",
                value: "Prep & cook",
                note: "Biết trước khi bắt tay vào bếp",
              },
              {
                icon: ChefHat,
                label: "Dành cho tác giả",
                value: "Dashboard",
                note: "Quản lý và xuất bản công thức",
              },
            ].map((item) => (
              <div
                key={item.label}
                className="flex items-start gap-3 rounded-xl border bg-card p-4 shadow-sm"
              >
                <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-muted">
                  <item.icon className="size-5" />
                </div>
                <div className="flex flex-col">
                  <span className="text-sm font-medium">{item.value}</span>
                  <span className="text-sm text-muted-foreground">{item.note}</span>
                </div>
              </div>
            ))}
          </div>
        </Container>
      </section>

      <Container className="py-10">
        {categories.length > 0 && (
          <div className="flex flex-col gap-3">
            <h2 className="text-lg font-semibold">Danh mục nổi bật</h2>
            <div className="flex flex-wrap gap-2">
              {categories.slice(0, 8).map((category) => (
                <Button key={category.id} asChild variant="outline" size="sm">
                  <Link href={`/recipes?categoryId=${category.id}`}>
                    {category.name}
                    <Badge variant="secondary" className="ml-1">
                      {category.recipeCount}
                    </Badge>
                  </Link>
                </Button>
              ))}
            </div>
          </div>
        )}

        <div className="mt-10 flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <h2 className="text-2xl font-bold tracking-tight">
              Công thức mới nhất
            </h2>
            <p className="text-sm text-muted-foreground">
              Những món ăn vừa được cộng đồng chia sẻ.
            </p>
          </div>
          <Button asChild variant="ghost">
            <Link href="/recipes">
              Xem tất cả
              <ArrowRight />
            </Link>
          </Button>
        </div>

        <div className="mt-4 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {recipes.items.map((recipe) => (
            <RecipeCard key={recipe.id} recipe={recipe} />
          ))}
        </div>

        {recipes.items.length === 0 && (
          <div className="mt-4">
            <EmptyState
              title="Chưa tải được công thức"
              description="Backend có thể chưa khởi động. Hãy thử lại sau hoặc mở trang danh sách công thức."
              action={
                <Button asChild variant="outline">
                  <Link href="/recipes">Mở trang công thức</Link>
                </Button>
              }
            />
          </div>
        )}
      </Container>
    </>
  );
}

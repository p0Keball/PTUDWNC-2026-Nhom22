import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import {
  ArrowLeft,
  CalendarDays,
  ChefHat,
  Clock,
  Flame,
  Timer,
  Users,
} from "lucide-react";

import { Container } from "@/components/Container";
import { ErrorState } from "@/components/ErrorState";
import RecipeGallery from "@/components/RecipeGallery";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { getRecipeBySlug, type RecipeDetail } from "@/lib/api";
import {
  difficultyLabel,
  formatDate,
  formatMinutes,
  totalTime,
} from "@/lib/utils";

interface RecipePageProps {
  params: Promise<{ slug: string }>;
}

export const revalidate = 300;

function primaryImage(recipe: RecipeDetail) {
  return recipe.images.find((image) => image.isPrimary) ?? recipe.images[0] ?? null;
}

function ingredientText(item: RecipeDetail["ingredients"][number]) {
  const amount = [item.quantity, item.unit].filter(Boolean).join(" ");
  return [amount, item.name, item.notes ? `(${item.notes})` : null]
    .filter(Boolean)
    .join(" ")
    .trim();
}

export async function generateMetadata({
  params,
}: RecipePageProps): Promise<Metadata> {
  const { slug } = await params;
  const recipe = await getRecipeBySlug(decodeURIComponent(slug)).catch(
    () => null
  );

  if (!recipe) {
    return {
      title: "Không tìm thấy công thức",
      robots: { index: false, follow: false },
    };
  }

  const image = primaryImage(recipe);
  const description =
    recipe.description ||
    `Công thức ${recipe.title} với ${recipe.servings} khẩu phần.`;

  return {
    title: recipe.title,
    description,
    alternates: { canonical: `/recipes/${recipe.slug}` },
    openGraph: {
      type: "article",
      title: recipe.title,
      description,
      url: `/recipes/${recipe.slug}`,
      publishedTime: recipe.publishedAt ?? undefined,
      images: image
        ? [{ url: image.originalUrl, alt: image.altText ?? recipe.title }]
        : undefined,
    },
    twitter: {
      card: "summary_large_image",
      title: recipe.title,
      description,
      images: image ? [image.originalUrl] : undefined,
    },
  };
}

export default async function RecipeDetailPage({ params }: RecipePageProps) {
  const { slug } = await params;
  let recipe: RecipeDetail | null;

  try {
    recipe = await getRecipeBySlug(decodeURIComponent(slug));
  } catch {
    return (
      <Container className="max-w-4xl">
        <ErrorState
          title="Không tải được công thức"
          message="Máy chủ dữ liệu có thể chưa sẵn sàng. Vui lòng thử lại sau."
          retryHref={`/recipes/${encodeURIComponent(slug)}`}
          homeHref="/recipes"
        />
      </Container>
    );
  }

  if (!recipe) notFound();

  const image = primaryImage(recipe);
  const totalMinutes = recipe.prepTimeMinutes + recipe.cookTimeMinutes;

  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "Recipe",
    name: recipe.title,
    description: recipe.description,
    image: recipe.images.map((item) => item.originalUrl),
    author: { "@type": "Organization", name: "Culinary Blog" },
    datePublished: recipe.publishedAt ?? undefined,
    prepTime: `PT${recipe.prepTimeMinutes}M`,
    cookTime: `PT${recipe.cookTimeMinutes}M`,
    totalTime: `PT${totalMinutes}M`,
    recipeYield: `${recipe.servings} khẩu phần`,
    recipeIngredient: recipe.ingredients.map(ingredientText),
    recipeInstructions: recipe.steps.map((step) => ({
      "@type": "HowToStep",
      position: step.stepNumber,
      name: step.title,
      text: step.description,
    })),
    nutrition: recipe.nutrition
      ? {
          "@type": "NutritionInformation",
          calories: recipe.nutrition.calories
            ? `${recipe.nutrition.calories} kcal`
            : undefined,
          proteinContent: recipe.nutrition.protein
            ? `${recipe.nutrition.protein} g`
            : undefined,
          carbohydrateContent: recipe.nutrition.carbohydrates
            ? `${recipe.nutrition.carbohydrates} g`
            : undefined,
          fatContent: recipe.nutrition.fat
            ? `${recipe.nutrition.fat} g`
            : undefined,
          fiberContent: recipe.nutrition.fiber
            ? `${recipe.nutrition.fiber} g`
            : undefined,
          sodiumContent: recipe.nutrition.sodium
            ? `${recipe.nutrition.sodium} mg`
            : undefined,
        }
      : undefined,
  };

  return (
    <>
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: JSON.stringify(jsonLd).replace(/</g, "\\u003c"),
        }}
      />

      <Container className="max-w-4xl">
        <nav className="flex items-center gap-1.5 text-sm text-muted-foreground">
          <Link href="/" className="hover:text-foreground">
            Trang chủ
          </Link>
          <span>/</span>
          <Link href="/recipes" className="hover:text-foreground">
            Công thức
          </Link>
          <span>/</span>
          <span className="line-clamp-1 text-foreground">{recipe.title}</span>
        </nav>

        <div className="flex flex-col gap-4">
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant="secondary">
              {difficultyLabel(recipe.difficulty)}
            </Badge>
            <Badge variant="outline">
              <Clock />
              {totalTime(recipe.prepTimeMinutes, recipe.cookTimeMinutes)}
            </Badge>
            <Badge variant="outline">
              <Users />
              {recipe.servings} khẩu phần
            </Badge>
            {recipe.publishedAt && (
              <Badge variant="outline">
                <CalendarDays />
                {formatDate(recipe.publishedAt)}
              </Badge>
            )}
          </div>

          <div className="flex flex-col gap-3">
            <h1 className="text-3xl font-bold tracking-tight sm:text-4xl">
              {recipe.title}
            </h1>
            <p className="max-w-3xl text-muted-foreground">
              {recipe.description}
            </p>
          </div>
        </div>

        {image ? (
          <RecipeGallery images={recipe.images} />
        ) : (
          <div className="flex aspect-[16/9] w-full items-center justify-center rounded-xl border bg-muted text-muted-foreground">
            <ChefHat className="size-10" />
          </div>
        )}

        <div className="grid gap-6 lg:grid-cols-[300px_1fr]">
          <div className="flex flex-col gap-4">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-base">
                  <ChefHat className="size-4" />
                  Thông tin nhanh
                </CardTitle>
              </CardHeader>
              <CardContent className="flex flex-col gap-3 text-sm">
                <div className="flex items-center justify-between gap-2">
                  <span className="inline-flex items-center gap-1.5 text-muted-foreground">
                    <Timer className="size-4" />
                    Chuẩn bị
                  </span>
                  <span className="font-medium">
                    {formatMinutes(recipe.prepTimeMinutes)}
                  </span>
                </div>
                <div className="flex items-center justify-between gap-2">
                  <span className="inline-flex items-center gap-1.5 text-muted-foreground">
                    <Flame className="size-4" />
                    Nấu
                  </span>
                  <span className="font-medium">
                    {formatMinutes(recipe.cookTimeMinutes)}
                  </span>
                </div>
                <Separator />
                <div className="flex items-center justify-between gap-2">
                  <span className="text-muted-foreground">Tổng thời gian</span>
                  <span className="font-semibold">
                    {formatMinutes(totalMinutes)}
                  </span>
                </div>
                <div className="flex items-center justify-between gap-2">
                  <span className="text-muted-foreground">Khẩu phần</span>
                  <span className="font-medium">{recipe.servings}</span>
                </div>
                <div className="flex items-center justify-between gap-2">
                  <span className="text-muted-foreground">Độ khó</span>
                  <span className="font-medium">
                    {difficultyLabel(recipe.difficulty)}
                  </span>
                </div>
              </CardContent>
            </Card>

            {recipe.nutrition && (
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Dinh dưỡng</CardTitle>
                </CardHeader>
                <CardContent className="grid grid-cols-2 gap-3 text-sm">
                  {[
                    ["Calories", recipe.nutrition.calories, "kcal"],
                    ["Protein", recipe.nutrition.protein, "g"],
                    ["Carbs", recipe.nutrition.carbohydrates, "g"],
                    ["Fat", recipe.nutrition.fat, "g"],
                    ["Fiber", recipe.nutrition.fiber, "g"],
                    ["Sodium", recipe.nutrition.sodium, "mg"],
                  ].map(([label, value, unit]) => (
                    <div key={label as string} className="rounded-lg bg-muted/50 p-2">
                      <p className="text-xs text-muted-foreground">{label}</p>
                      <p className="font-medium">
                        {value == null ? "—" : `${value} ${unit}`}
                      </p>
                    </div>
                  ))}
                </CardContent>
              </Card>
            )}
          </div>

          <div className="flex flex-col gap-6">
            <Card>
              <CardHeader>
                <CardTitle>Nguyên liệu</CardTitle>
              </CardHeader>
              <CardContent>
                {recipe.ingredients.length === 0 ? (
                  <p className="text-sm text-muted-foreground">
                    Công thức chưa có nguyên liệu.
                  </p>
                ) : (
                  <ul className="grid gap-2 sm:grid-cols-2">
                    {recipe.ingredients.map((item) => (
                      <li
                        key={item.id}
                        className="flex items-start gap-2 rounded-lg border bg-muted/20 px-3 py-2 text-sm"
                      >
                        <span className="mt-1.5 size-1.5 shrink-0 rounded-full bg-primary" />
                        {ingredientText(item)}
                      </li>
                    ))}
                  </ul>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Các bước thực hiện</CardTitle>
              </CardHeader>
              <CardContent>
                {recipe.steps.length === 0 ? (
                  <div className="flex flex-col gap-2 text-sm text-muted-foreground">
                    {recipe.instructions ? (
                      <p className="whitespace-pre-line">{recipe.instructions}</p>
                    ) : (
                      <p>Công thức chưa có hướng dẫn.</p>
                    )}
                  </div>
                ) : (
                  <ol className="flex flex-col gap-4">
                    {[...recipe.steps]
                      .sort((a, b) => a.stepNumber - b.stepNumber)
                      .map((step) => (
                        <li key={step.id} className="flex gap-3">
                          <span className="flex size-8 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-semibold text-primary-foreground">
                            {step.stepNumber}
                          </span>
                          <div className="flex flex-col gap-1">
                            <p className="font-medium">{step.title}</p>
                            <p className="text-sm text-muted-foreground">
                              {step.description}
                            </p>
                            {step.timerMinutes != null && (
                              <span className="inline-flex w-fit items-center gap-1 rounded-full bg-muted px-2 py-0.5 text-xs text-muted-foreground">
                                <Timer className="size-3" />
                                {formatMinutes(step.timerMinutes)}
                              </span>
                            )}
                          </div>
                        </li>
                      ))}
                  </ol>
                )}
              </CardContent>
            </Card>

            <div className="flex flex-wrap items-center gap-2">
              <Link
                href="/recipes"
                className="inline-flex items-center gap-1 text-sm text-muted-foreground hover:text-foreground"
              >
                <ArrowLeft className="size-4" />
                Xem thêm công thức
              </Link>
            </div>
          </div>
        </div>
      </Container>
    </>
  );
}

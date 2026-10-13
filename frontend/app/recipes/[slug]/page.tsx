import Link from "next/link";
import { notFound } from "next/navigation";
import { getRecipeBySlug } from "@/lib/api";

export default async function RecipeDetailPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  const recipe = await getRecipeBySlug(decodeURIComponent(slug));
  if (!recipe) notFound();

  const primary =
    recipe.images.find((i) => i.isPrimary) ?? recipe.images[0] ?? null;

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-6 p-4 sm:p-6">
      <div className="flex items-start justify-between gap-4">
        <div className="flex flex-col gap-2">
          <h1 className="text-2xl font-bold sm:text-3xl">{recipe.title}</h1>
          <div className="flex flex-wrap gap-2 text-xs text-zinc-500">
          <span className="rounded-full bg-zinc-100 px-2 py-1 dark:bg-zinc-800">
            {recipe.difficulty}
          </span>
          <span className="rounded-full bg-zinc-100 px-2 py-1 dark:bg-zinc-800">
            Chuẩn bị {recipe.prepTimeMinutes} phút
          </span>
          <span className="rounded-full bg-zinc-100 px-2 py-1 dark:bg-zinc-800">
            Nấu {recipe.cookTimeMinutes} phút
          </span>
          <span className="rounded-full bg-zinc-100 px-2 py-1 dark:bg-zinc-800">
            {recipe.servings} khẩu phần
            </span>
          </div>
        </div>
        <Link
          href={`/recipes/${encodeURIComponent(recipe.slug)}/edit`}
          className="shrink-0 rounded-full border px-4 py-2 text-sm hover:bg-zinc-100 dark:hover:bg-zinc-800"
        >
          Sửa
        </Link>
      </div>

      {primary && (
        // eslint-disable-next-line @next/next/no-img-element
        <img
          src={primary.originalUrl}
          alt={primary.altText ?? recipe.title}
          className="aspect-[16/9] w-full rounded-xl object-cover"
        />
      )}

      <p className="leading-relaxed text-zinc-700 dark:text-zinc-300">
        {recipe.description}
      </p>

      <section className="flex flex-col gap-2">
        <h2 className="text-lg font-semibold">Cách làm</h2>
        <p className="whitespace-pre-line leading-relaxed">{recipe.instructions}</p>
      </section>

      {recipe.ingredients.length > 0 && (
        <section className="flex flex-col gap-2">
          <h2 className="text-lg font-semibold">
            Nguyên liệu ({recipe.ingredients.length})
          </h2>
          <ul className="list-disc space-y-1 pl-5">
            {recipe.ingredients.map((ing) => (
              <li key={ing.id}>
                {ing.name}
                {ing.quantity != null && ` — ${ing.quantity}${ing.unit ? ` ${ing.unit}` : ""}`}
                {ing.notes && ` (${ing.notes})`}
              </li>
            ))}
          </ul>
        </section>
      )}

      {recipe.steps.length > 0 && (
        <section className="flex flex-col gap-3">
          <h2 className="text-lg font-semibold">
            Các bước ({recipe.steps.length})
          </h2>
          <ol className="flex flex-col gap-3">
            {recipe.steps.map((s) => (
              <li
                key={s.id}
                className="rounded-xl border border-zinc-200 p-4 dark:border-zinc-800"
              >
                <p className="font-medium">
                  Bước {s.stepNumber}: {s.title}
                </p>
                <p className="mt-1 text-sm text-zinc-600 dark:text-zinc-400">
                  {s.description}
                </p>
                {s.timerMinutes != null && (
                  <p className="mt-1 text-xs text-zinc-500">
                    Hẹn giờ: {s.timerMinutes} phút
                  </p>
                )}
              </li>
            ))}
          </ol>
        </section>
      )}

      {recipe.nutrition && (
        <section className="flex flex-col gap-2">
          <h2 className="text-lg font-semibold">Dinh dưỡng</h2>
          <div className="grid grid-cols-2 gap-2 text-sm sm:grid-cols-3">
            {(
              [
                ["Calories", recipe.nutrition.calories],
                ["Protein", recipe.nutrition.protein],
                ["Carbs", recipe.nutrition.carbohydrates],
                ["Fat", recipe.nutrition.fat],
                ["Fiber", recipe.nutrition.fiber],
                ["Sodium", recipe.nutrition.sodium],
              ] as const
            ).map(([label, value]) => (
              <div
                key={label}
                className="rounded-lg bg-zinc-100 px-3 py-2 dark:bg-zinc-800"
              >
                {label}: {value ?? "—"}
              </div>
            ))}
          </div>
        </section>
      )}
    </main>
  );
}

"use client";

import { useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import RecipeForm, {
  emptyRecipeForm,
  mapBackendErrors,
  validateRecipeForm,
  type RecipeFormErrors,
  type RecipeFormValues,
} from "@/components/RecipeForm";
import { RecipeFormSkeleton } from "@/components/RecipeSkeleton";
import {
  ApiError,
  createRecipe,
  getCategories,
  type Category,
} from "@/lib/api";

function getAccessToken(): string | undefined {
  if (typeof window === "undefined") return undefined;
  return (
    window.localStorage.getItem("accessToken") ??
    window.localStorage.getItem("access_token") ??
    undefined
  );
}

export default function CreateRecipePage() {
  const router = useRouter();
  const [values, setValues] = useState<RecipeFormValues>(emptyRecipeForm);
  const [errors, setErrors] = useState<RecipeFormErrors>({});
  const [categories, setCategories] = useState<Category[]>([]);
  const [loadingCats, setLoadingCats] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getCategories()
      .then((cats) => {
        if (!cancelled) setCategories(cats);
      })
      .catch(() => {
        if (!cancelled)
          setServerError("Không tải được danh mục. Vui lòng thử lại.");
      })
      .finally(() => {
        if (!cancelled) setLoadingCats(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const clientErrors = validateRecipeForm(values);
    setErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0) return;

    setSubmitting(true);
    setServerError(null);
    try {
      const created = await createRecipe(
        {
          title: values.title.trim(),
          description: values.description,
          instructions: values.instructions,
          categoryId: values.categoryId,
          prepTimeMinutes: Number(values.prepTimeMinutes),
          cookTimeMinutes: Number(values.cookTimeMinutes),
          servings: Number(values.servings),
          difficulty: Number(values.difficulty),
        },
        getAccessToken()
      );
      router.push(`/recipes/${encodeURIComponent(created.slug)}`);
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 401)
          setServerError("Bạn cần đăng nhập để tạo công thức.");
        else if (err.errors) setErrors(mapBackendErrors(err.errors));
        else setServerError(err.message);
      } else {
        setServerError("Có lỗi xảy ra. Vui lòng thử lại.");
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-6 p-4 sm:p-6">
      <div className="flex items-center justify-between">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-bold sm:text-3xl">Tạo công thức mới</h1>
          <p className="text-sm text-zinc-500">
            Công thức mới được lưu ở trạng thái nháp.
          </p>
        </div>
        <Link
          href="/"
          className="rounded-full border px-4 py-2 text-sm hover:bg-zinc-100 dark:hover:bg-zinc-800"
        >
          Hủy
        </Link>
      </div>

      {serverError && (
        <p className="rounded-lg border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700">
          {serverError}
        </p>
      )}

      {loadingCats ? (
        <RecipeFormSkeleton />
      ) : (
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <RecipeForm
            values={values}
            errors={errors}
            categories={categories}
            disabled={submitting}
            onChange={setValues}
          />
          <button
            type="submit"
            disabled={submitting}
            className="rounded-lg bg-zinc-900 px-4 py-2.5 text-sm font-medium text-white transition hover:bg-zinc-700 disabled:opacity-50 dark:bg-zinc-100 dark:text-zinc-900 dark:hover:bg-white"
          >
            {submitting ? "Đang tạo…" : "Tạo công thức"}
          </button>
        </form>
      )}
    </main>
  );
}

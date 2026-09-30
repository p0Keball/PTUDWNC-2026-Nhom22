"use client";

import { use, useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import RecipeForm, {
  mapBackendErrors,
  validateRecipeForm,
  type RecipeFormErrors,
  type RecipeFormValues,
} from "@/components/RecipeForm";
import {
  ApiError,
  getCategories,
  getRecipeBySlug,
  updateRecipe,
  type Category,
  type RecipeDetail,
} from "@/lib/api";

const DIFFICULTY_TO_NUMBER: Record<string, string> = {
  Easy: "1",
  Medium: "2",
  Hard: "3",
  Expert: "4",
};

function getAccessToken(): string | undefined {
  if (typeof window === "undefined") return undefined;
  return (
    window.localStorage.getItem("accessToken") ??
    window.localStorage.getItem("access_token") ??
    undefined
  );
}

function toFormValues(detail: RecipeDetail): RecipeFormValues {
  return {
    title: detail.title,
    description: detail.description,
    instructions: detail.instructions,
    categoryId: detail.categoryId,
    prepTimeMinutes: String(detail.prepTimeMinutes),
    cookTimeMinutes: String(detail.cookTimeMinutes),
    servings: String(detail.servings),
    difficulty: DIFFICULTY_TO_NUMBER[detail.difficulty] ?? "1",
  };
}

export default function EditRecipePage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = use(params);
  const router = useRouter();
  const [detail, setDetail] = useState<RecipeDetail | null>(null);
  const [values, setValues] = useState<RecipeFormValues | null>(null);
  const [errors, setErrors] = useState<RecipeFormErrors>({});
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    let cancelled = false;
    Promise.all([
      getRecipeBySlug(decodeURIComponent(slug)),
      getCategories().catch(() => [] as Category[]),
    ])
      .then(([recipe, cats]) => {
        if (cancelled) return;
        if (!recipe) {
          setNotFound(true);
        } else {
          setDetail(recipe);
          setValues(toFormValues(recipe));
        }
        setCategories(cats);
      })
      .catch(() => {
        if (!cancelled)
          setServerError("Không tải được công thức. Vui lòng thử lại.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [slug]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!values || !detail) return;
    const clientErrors = validateRecipeForm(values);
    setErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0) return;

    setSubmitting(true);
    setServerError(null);
    try {
      const updated = await updateRecipe(
        detail.id,
        {
          title: values.title.trim(),
          description: values.description,
          instructions: values.instructions,
          categoryId: values.categoryId,
          prepTimeMinutes: Number(values.prepTimeMinutes),
          cookTimeMinutes: Number(values.cookTimeMinutes),
          servings: Number(values.servings),
          difficulty: Number(values.difficulty),
          rowVersion: detail.rowVersion,
        },
        getAccessToken()
      );
      router.push(`/recipes/${encodeURIComponent(updated.slug)}`);
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 401)
          setServerError("Bạn cần đăng nhập để sửa công thức.");
        else if (err.status === 403)
          setServerError("Bạn không có quyền sửa công thức này.");
        else if (err.status === 409)
          setServerError(
            "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng tải lại trang."
          );
        else if (err.errors) setErrors(mapBackendErrors(err.errors));
        else setServerError(err.message);
      } else {
        setServerError("Có lỗi xảy ra. Vui lòng thử lại.");
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (loading)
    return (
      <main className="mx-auto w-full max-w-3xl flex-1 p-4 sm:p-6">
        <p className="text-sm text-zinc-500">Đang tải công thức…</p>
      </main>
    );

  if (notFound)
    return (
      <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-4 p-4 sm:p-6">
        <h1 className="text-2xl font-bold">Không tìm thấy công thức</h1>
        <Link href="/" className="text-sm underline">
          Về trang chủ
        </Link>
      </main>
    );

  return (
    <main className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-6 p-4 sm:p-6">
      <div className="flex items-center justify-between">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-bold sm:text-3xl">Sửa công thức</h1>
          <p className="text-sm text-zinc-500">{detail?.title}</p>
        </div>
        <Link
          href={`/recipes/${encodeURIComponent(decodeURIComponent(slug))}`}
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

      {values && (
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
            {submitting ? "Đang lưu…" : "Lưu thay đổi"}
          </button>
        </form>
      )}
    </main>
  );
}

"use client";

import { useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ArrowLeft, Save } from "lucide-react";
import { toast } from "sonner";

import { PageHeader } from "@/components/PageHeader";
import RecipeForm, {
  emptyRecipeForm,
  mapBackendErrors,
  validateRecipeForm,
  type RecipeFormErrors,
  type RecipeFormValues,
} from "@/components/RecipeForm";
import { RecipeFormSkeleton } from "@/components/RecipeSkeleton";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  createRecipe,
  getCategories,
  type Category,
} from "@/lib/api";
import { getErrorMessage, getFieldErrors } from "@/lib/errors";
import { useAuthStore } from "@/store/auth-store";

export default function CreateRecipePage() {
  const router = useRouter();
  const token = useAuthStore((state) => state.accessToken);

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

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
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
        token
      );
      toast.success("Đã tạo công thức nháp. Hãy thêm nguyên liệu và các bước.");
      router.push(`/dashboard/recipes/${encodeURIComponent(created.slug)}/edit`);
    } catch (err) {
      const fieldErrors = getFieldErrors(err);
      if (Object.keys(fieldErrors).length > 0) {
        setErrors(mapBackendErrors(fieldErrors));
      } else {
        setServerError(getErrorMessage(err, "Tạo công thức thất bại."));
      }
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Tạo công thức mới"
        description="Công thức mới được lưu ở trạng thái nháp. Sau khi tạo, bạn có thể thêm nguyên liệu, các bước và ảnh."
        actions={
          <Button asChild variant="outline">
            <Link href="/dashboard/recipes">
              <ArrowLeft />
              Quay lại
            </Link>
          </Button>
        }
      />

      {serverError && (
        <Alert variant="destructive">
          <AlertTitle>Không thể tạo công thức</AlertTitle>
          <AlertDescription>{serverError}</AlertDescription>
        </Alert>
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
          <div className="flex flex-wrap items-center gap-2">
            <Button type="submit" disabled={submitting}>
              <Save />
              {submitting ? "Đang tạo…" : "Tạo công thức"}
            </Button>
            <Button asChild variant="outline" disabled={submitting}>
              <Link href="/dashboard/recipes">Hủy</Link>
            </Button>
          </div>
        </form>
      )}
    </div>
  );
}

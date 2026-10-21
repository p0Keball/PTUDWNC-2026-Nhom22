"use client";

import { use, useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ArrowLeft, ExternalLink, RefreshCw, Save } from "lucide-react";
import { toast } from "sonner";

import { PageHeader } from "@/components/PageHeader";
import RecipeForm, {
  mapBackendErrors,
  validateRecipeForm,
  type RecipeFormErrors,
  type RecipeFormValues,
} from "@/components/RecipeForm";
import { RecipeFormSkeleton } from "@/components/RecipeSkeleton";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  ApiError,
  findMyRecipeById,
  getCategories,
  getMyRecipeById,
  getRecipeBySlug,
  updateRecipe,
  type Category,
  type RecipeDetail,
} from "@/lib/api";
import { getErrorMessage, getFieldErrors } from "@/lib/errors";
import { useAuthStore } from "@/store/auth-store";

const DIFFICULTY_TO_NUMBER: Record<string, string> = {
  Easy: "1",
  Medium: "2",
  Hard: "3",
  Expert: "4",
};

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

function isLookupMiss(err: unknown) {
  return (
    err instanceof ApiError && [401, 403, 404].includes(err.status)
  );
}

async function resolveRecipe(id: string, token: string | null) {
  // 1. Route SRS dùng [id]: lấy chi tiết có auth (draft/archived cũng được).
  try {
    const mine = await getMyRecipeById(id, token);
    if (mine) return mine;
  } catch (err) {
    if (!isLookupMiss(err)) throw err;
  }

  // 2. Tương thích URL cũ dùng slug: thử trang public trước.
  try {
    const direct = await getRecipeBySlug(id);
    if (direct) return direct;
  } catch (err) {
    if (!(err instanceof ApiError) || (err.status !== 403 && err.status !== 404)) {
      throw err;
    }
  }

  // 3. Slug cũ: tìm trong danh sách của mình rồi lấy chi tiết theo id.
  const summary = await findMyRecipeById(id, token ?? undefined);
  if (!summary) return null;
  return getMyRecipeById(summary.id, token);
}

export default function EditRecipePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id: rawId } = use(params);
  const id = decodeURIComponent(rawId);
  const router = useRouter();
  const token = useAuthStore((state) => state.accessToken);

  const [detail, setDetail] = useState<RecipeDetail | null>(null);
  const [values, setValues] = useState<RecipeFormValues | null>(null);
  const [errors, setErrors] = useState<RecipeFormErrors>({});
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [reloading, setReloading] = useState(false);
  const [serverError, setServerError] = useState<string | null>(null);
  const [conflict, setConflict] = useState(false);
  const [reloaded, setReloaded] = useState(false);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    let cancelled = false;

    Promise.all([
      resolveRecipe(id, token),
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
        setServerError(null);
      })
      .catch((err) => {
        if (!cancelled)
          setServerError(
            getErrorMessage(err, "Không tải được công thức. Vui lòng thử lại.")
          );
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [id, token]);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!values || !detail) return;

    const clientErrors = validateRecipeForm(values);
    setErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0) return;

    setSubmitting(true);
    setServerError(null);
    setConflict(false);
    setReloaded(false);

    try {
      await updateRecipe(
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
        token ?? undefined
      );
      toast.success("Đã lưu thay đổi.");
      // Bản nháp không xem được ở trang public (server component không gửi cookie),
      // nên quay về dashboard thay vì /recipes/{slug}.
      router.push("/dashboard/recipes");
    } catch (err) {
      if (err instanceof ApiError && err.status === 409) {
        setConflict(true);
      } else {
        const fieldErrors = getFieldErrors(err);
        if (Object.keys(fieldErrors).length > 0) {
          setErrors(mapBackendErrors(fieldErrors));
        } else {
          setServerError(getErrorMessage(err, "Cập nhật công thức thất bại."));
        }
      }
    } finally {
      setSubmitting(false);
    }
  }

  async function handleReload() {
    setReloading(true);
    setServerError(null);
    try {
      const fresh = await getMyRecipeById(detail?.id ?? id, token);
      if (!fresh) {
        setNotFound(true);
        return;
      }
      // Chỉ cập nhật rowVersion/detail, giữ nguyên dữ liệu người dùng đã nhập.
      setDetail(fresh);
      setConflict(false);
      setReloaded(true);
      toast.success("Đã tải bản mới nhất, dữ liệu bạn nhập được giữ nguyên.");
    } catch (err) {
      setServerError(getErrorMessage(err, "Không tải được bản mới."));
    } finally {
      setReloading(false);
    }
  }

  if (loading) {
    return (
      <div className="flex flex-col gap-4">
        <div className="h-8 w-48 animate-pulse rounded bg-muted" />
        <RecipeFormSkeleton />
      </div>
    );
  }

  if (notFound) {
    return (
      <div className="flex flex-col items-start gap-4">
        <h1 className="text-2xl font-bold">Không tìm thấy công thức</h1>
        <p className="text-sm text-muted-foreground">
          Công thức không tồn tại hoặc bạn không có quyền chỉnh sửa.
        </p>
        <Button asChild>
          <Link href="/dashboard/recipes">Quay lại dashboard</Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Sửa công thức"
        description={detail?.title}
        actions={
          <>
            <Button asChild variant="outline">
              <Link href="/dashboard/recipes">
                <ArrowLeft />
                Quay lại
              </Link>
            </Button>
            {detail && detail.status === "Published" && (
              <Button asChild variant="ghost">
                <Link href={`/recipes/${encodeURIComponent(detail.slug)}`}>
                  <ExternalLink />
                  Xem công thức
                </Link>
              </Button>
            )}
          </>
        }
      />

      {serverError && (
        <Alert variant="destructive">
          <AlertTitle>Không thể lưu thay đổi</AlertTitle>
          <AlertDescription>{serverError}</AlertDescription>
        </Alert>
      )}

      {conflict && (
        <Alert variant="warning">
          <AlertTitle>Dữ liệu đã bị thay đổi (409)</AlertTitle>
          <AlertDescription className="flex flex-col gap-2">
            <span>
              Công thức đã được cập nhật bởi người khác. Tải bản mới nhất — nội
              dung bạn đang nhập vẫn được giữ nguyên, sau đó lưu lại.
            </span>
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="w-fit"
              onClick={handleReload}
              disabled={reloading}
            >
              <RefreshCw />
              {reloading ? "Đang tải…" : "Tải bản mới & giữ dữ liệu"}
            </Button>
          </AlertDescription>
        </Alert>
      )}

      {reloaded && !conflict && (
        <Alert variant="success">
          <AlertTitle>Đã tải bản mới nhất</AlertTitle>
          <AlertDescription>
            Dữ liệu bạn nhập được giữ nguyên. Nhấn “Lưu thay đổi” để lưu lại.
          </AlertDescription>
        </Alert>
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
          <div className="flex flex-wrap items-center gap-2">
            <Button type="submit" disabled={submitting}>
              <Save />
              {submitting ? "Đang lưu…" : "Lưu thay đổi"}
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

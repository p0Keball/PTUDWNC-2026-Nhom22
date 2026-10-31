"use client";

import { use, useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  Archive,
  ArrowLeft,
  ExternalLink,
  RefreshCw,
  RotateCcw,
  Save,
  Send,
  Trash2,
  Undo2,
} from "lucide-react";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/ConfirmDialog";
import { PageHeader } from "@/components/PageHeader";
import RecipeContentEditor from "@/components/RecipeContentEditor";
import RecipeForm, {
  mapBackendErrors,
  validateRecipeForm,
  type RecipeFormErrors,
  type RecipeFormValues,
} from "@/components/RecipeForm";
import { RecipeFormSkeleton } from "@/components/RecipeSkeleton";
import { StatusBadge } from "@/components/StatusBadge";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  ApiError,
  archiveRecipe,
  deleteRecipe,
  findMyRecipeById,
  getCategories,
  getMyRecipeById,
  getRecipeBySlug,
  getRecipeValidation,
  publishRecipe,
  unarchiveRecipe,
  unpublishRecipe,
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

type StatusAction =
  | "publish"
  | "unpublish"
  | "archive"
  | "unarchive"
  | "delete";

const STATUS_ACTIONS: Record<
  StatusAction,
  {
    success: string;
    confirmTitle: string;
    confirmLabel: string;
    confirmDescription: string;
    destructive?: boolean;
  }
> = {
  publish: {
    success: "Đã xuất bản công thức.",
    confirmTitle: "Xuất bản công thức?",
    confirmLabel: "Xuất bản",
    confirmDescription:
      "Công thức sẽ hiển thị công khai ngay sau khi xuất bản thành công.",
  },
  unpublish: {
    success: "Đã gỡ xuất bản, công thức quay về Nháp.",
    confirmTitle: "Gỡ xuất bản công thức?",
    confirmLabel: "Gỡ xuất bản",
    confirmDescription:
      "Công thức sẽ ngừng hiển thị công khai và quay về trạng thái Nháp.",
  },
  archive: {
    success: "Đã lưu trữ công thức.",
    confirmTitle: "Lưu trữ công thức?",
    confirmLabel: "Lưu trữ",
    confirmDescription:
      "Công thức sẽ bị ẩn khỏi danh sách công khai nhưng vẫn có thể khôi phục về Nháp.",
  },
  unarchive: {
    success: "Đã khôi phục công thức về Nháp.",
    confirmTitle: "Khôi phục công thức?",
    confirmLabel: "Khôi phục",
    confirmDescription:
      "Công thức sẽ trở về trạng thái Nháp để tiếp tục chỉnh sửa và xuất bản.",
  },
  delete: {
    success: "Đã xóa công thức.",
    confirmTitle: "Xóa công thức?",
    confirmLabel: "Xóa",
    confirmDescription:
      "Công thức sẽ bị xóa mềm và không còn hiển thị trong danh sách của bạn.",
    destructive: true,
  },
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
  const token = useAuthStore((state) => state.accessToken);
  const router = useRouter();

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
  const [statusBusy, setStatusBusy] = useState(false);
  const [statusError, setStatusError] = useState<string | null>(null);
  const [pendingStatusAction, setPendingStatusAction] =
    useState<StatusAction | null>(null);

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
        token ?? undefined
      );
      // Cập nhật rowVersion mới để lần lưu sau không bị 409; giữ người dùng
      // ở lại trang để tiếp tục thêm nguyên liệu/bước/ảnh.
      setDetail(updated);
      setValues(toFormValues(updated));
      toast.success("Đã lưu thay đổi.");
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

  async function runStatusAction(action: StatusAction) {
    if (!detail) return;
    setStatusBusy(true);
    setStatusError(null);

    try {
      if (action === "delete") {
        await deleteRecipe(detail.id, token);
        toast.success(STATUS_ACTIONS.delete.success);
        router.push("/dashboard/recipes");
        router.refresh();
        return;
      }

      if (action === "publish") {
        const validation = await getRecipeValidation(detail.id);
        if (!validation.canPublish) {
          const message = `Chưa thể xuất bản: ${
            validation.issues.join(" ") || "Cần ít nhất 1 bước thực hiện."
          }`;
          setStatusError(message);
          toast.error(message);
          return;
        }
      }

      const call =
        action === "publish"
          ? publishRecipe
          : action === "unpublish"
            ? unpublishRecipe
            : action === "archive"
              ? archiveRecipe
              : unarchiveRecipe;

      const result = await call(detail.id, token);
      setDetail((prev) => (prev ? { ...prev, status: result.status } : prev));
      setPendingStatusAction(null);
      toast.success(STATUS_ACTIONS[action].success);
    } catch (err) {
      const message = getErrorMessage(
        err,
        "Đổi trạng thái công thức thất bại."
      );
      setStatusError(message);
      toast.error(message);
    } finally {
      setStatusBusy(false);
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

      {detail && (
        <Card className="gap-3 py-4">
          <CardContent className="flex flex-col gap-3 px-4">
            <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
              <div className="flex flex-col gap-1">
                <div className="flex flex-wrap items-center gap-2">
                  <span className="text-sm font-medium">Trạng thái</span>
                  <StatusBadge status={detail.status} />
                </div>
                <p className="text-xs text-muted-foreground">
                  {detail.status === "Draft" &&
                    "Công thức đang ở bản nháp. Kiểm tra nội dung rồi xuất bản khi đã sẵn sàng."}
                  {detail.status === "Published" &&
                    "Công thức đang hiển thị công khai. Bạn có thể gỡ xuất bản để quay về Nháp."}
                  {detail.status === "Archived" &&
                    "Công thức đang được lưu trữ. Khôi phục về Nháp để tiếp tục chỉnh sửa."}
                </p>
              </div>

              <div className="flex flex-wrap gap-2">
                {detail.status === "Draft" && (
                  <Button
                    type="button"
                    size="sm"
                    disabled={statusBusy}
                    onClick={() => setPendingStatusAction("publish")}
                  >
                    <Send />
                    Xuất bản
                  </Button>
                )}
                {detail.status === "Published" && (
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    disabled={statusBusy}
                    onClick={() => setPendingStatusAction("unpublish")}
                  >
                    <Undo2 />
                    Gỡ xuất bản
                  </Button>
                )}
                {detail.status === "Archived" && (
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    disabled={statusBusy}
                    onClick={() => setPendingStatusAction("unarchive")}
                  >
                    <RotateCcw />
                    Khôi phục
                  </Button>
                )}
                {detail.status !== "Archived" && (
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    disabled={statusBusy}
                    onClick={() => setPendingStatusAction("archive")}
                  >
                    <Archive />
                    Lưu trữ
                  </Button>
                )}
                <Button
                  type="button"
                  variant="destructive"
                  size="sm"
                  disabled={statusBusy}
                  onClick={() => setPendingStatusAction("delete")}
                >
                  <Trash2 />
                  Xóa
                </Button>
              </div>
            </div>

            {statusError && (
              <p role="alert" className="text-xs text-destructive">
                {statusError}
              </p>
            )}
          </CardContent>
        </Card>
      )}

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

      {detail && (
        <RecipeContentEditor
          key={detail.rowVersion}
          recipeId={detail.id}
          steps={detail.steps}
          ingredients={detail.ingredients}
          images={detail.images}
        />
      )}

      <ConfirmDialog
        open={Boolean(pendingStatusAction)}
        onOpenChange={(open) => {
          if (!open) setPendingStatusAction(null);
        }}
        title={pendingStatusAction ? STATUS_ACTIONS[pendingStatusAction].confirmTitle : ""}
        description={
          pendingStatusAction
            ? `${STATUS_ACTIONS[pendingStatusAction].confirmDescription}${
                detail ? ` Công thức: "${detail.title}".` : ""
              }`
            : undefined
        }
        confirmLabel={
          pendingStatusAction
            ? STATUS_ACTIONS[pendingStatusAction].confirmLabel
            : "Xác nhận"
        }
        destructive={
          pendingStatusAction
            ? STATUS_ACTIONS[pendingStatusAction].destructive
            : false
        }
        loading={statusBusy}
        onConfirm={() => {
          if (!pendingStatusAction) return;
          void runStatusAction(pendingStatusAction);
        }}
      />
    </div>
  );
}

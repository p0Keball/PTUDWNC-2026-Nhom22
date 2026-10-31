"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { Edit3, Plus } from "lucide-react";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/ConfirmDialog";
import { EmptyState } from "@/components/EmptyState";
import { PageHeader } from "@/components/PageHeader";
import Pagination from "@/components/Pagination";
import { RecipeListSkeleton } from "@/components/RecipeSkeleton";
import { StatusBadge } from "@/components/StatusBadge";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  archiveRecipe,
  deleteRecipe,
  getMyRecipes,
  getRecipeValidation,
  publishRecipe,
  unarchiveRecipe,
  unpublishRecipe,
  type RecipeSummary,
} from "@/lib/api";
import { getErrorMessage } from "@/lib/errors";
import { useAuthStore } from "@/store/auth-store";

type StatusTab = "all" | "draft" | "published" | "archived";

type ConfirmableAction = "archive" | "unpublish" | "unarchive";

interface PendingConfirm {
  kind: ConfirmableAction;
  recipe: RecipeSummary;
}

const CONFIRM_COPY: Record<
  ConfirmableAction,
  { title: string; confirmLabel: string; success: string; verb: string }
> = {
  archive: {
    title: "Lưu trữ công thức?",
    confirmLabel: "Lưu trữ",
    success: "Đã lưu trữ công thức.",
    verb: "sẽ bị ẩn khỏi danh sách công khai và chuyển sang trạng thái Lưu trữ",
  },
  unpublish: {
    title: "Gỡ xuất bản công thức?",
    confirmLabel: "Gỡ xuất bản",
    success: "Đã gỡ xuất bản.",
    verb: "sẽ ngừng hiển thị công khai và quay về trạng thái Nháp",
  },
  unarchive: {
    title: "Khôi phục công thức?",
    confirmLabel: "Khôi phục",
    success: "Đã khôi phục công thức về Nháp.",
    verb: "sẽ được khôi phục về trạng thái Nháp để tiếp tục chỉnh sửa và xuất bản",
  },
};

const TABS: { value: StatusTab; label: string }[] = [
  { value: "all", label: "Tất cả" },
  { value: "draft", label: "Nháp" },
  { value: "published", label: "Đã xuất bản" },
  { value: "archived", label: "Lưu trữ" },
];

export default function DashboardRecipesPage() {
  const token = useAuthStore((state) => state.accessToken);

  const [tab, setTab] = useState<StatusTab>("all");
  const [items, setItems] = useState<RecipeSummary[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [acting, setActing] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<RecipeSummary | null>(null);
  const [pendingConfirm, setPendingConfirm] = useState<PendingConfirm | null>(
    null
  );

  const pageSize = 12;

  const fetchPage = useCallback(
    async (nextTab: StatusTab, nextPage: number, showLoading = true) => {
      if (showLoading) setLoading(true);
      try {
        const data = await getMyRecipes(nextPage, pageSize, nextTab, token);
        setItems(data.items);
        setTotalCount(data.totalCount);
        setPage(data.page);
        setError(null);
      } catch (err) {
        setError(getErrorMessage(err, "Không tải được danh sách công thức."));
      } finally {
        setLoading(false);
      }
    },
    [token]
  );

  useEffect(() => {
    let cancelled = false;

    getMyRecipes(1, pageSize, tab, token)
      .then((data) => {
        if (cancelled) return;
        setItems(data.items);
        setTotalCount(data.totalCount);
        setPage(data.page);
        setError(null);
      })
      .catch((err) => {
        if (!cancelled)
          setError(getErrorMessage(err, "Không tải được danh sách công thức."));
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [tab, token]);

  function handleTabChange(value: string) {
    setLoading(true);
    setTab(value as StatusTab);
  }

  function handlePageChange(nextPage: number) {
    setLoading(true);
    void fetchPage(tab, nextPage, false);
  }

  async function runAction(
    recipe: RecipeSummary,
    action: () => Promise<unknown>,
    successMessage: string,
    precheck?: () => Promise<string | null>
  ) {
    setActing(recipe.id);
    setError(null);
    try {
      // Checklist trước khi Publish: chặn nếu chưa đủ điều kiện tối thiểu.
      if (precheck) {
        const issue = await precheck();
        if (issue) {
          setError(issue);
          return;
        }
      }
      await action();
      toast.success(successMessage);
      const nextPage = items.length === 1 && page > 1 ? page - 1 : page;
      await fetchPage(tab, nextPage, false);
    } catch (err) {
      const message = getErrorMessage(err, "Thao tác thất bại. Vui lòng thử lại.");
      setError(message);
      toast.error(message);
    } finally {
      setActing(null);
      setDeleteTarget(null);
      setPendingConfirm(null);
    }
  }

  const totalPages = Math.max(Math.ceil(totalCount / pageSize), 1);

  function renderActions(recipe: RecipeSummary) {
    const busy = acting === recipe.id;

    return (
      <div className="flex flex-wrap gap-2 pt-2">
        <Button asChild variant="outline" size="sm" disabled={busy}>
          <Link href={`/dashboard/recipes/${encodeURIComponent(recipe.id)}/edit`}>
            <Edit3 />
            Sửa
          </Link>
        </Button>
        {recipe.status === "Draft" && (
          <Button
            type="button"
            size="sm"
            disabled={busy}
            onClick={() =>
              runAction(
                recipe,
                () => publishRecipe(recipe.id, token),
                "Đã xuất bản công thức.",
                async () => {
                  const validation = await getRecipeValidation(recipe.id);
                  return validation.canPublish
                    ? null
                    : `Chưa thể xuất bản: ${
                        validation.issues.join(" ") ||
                        "Cần ít nhất 1 bước thực hiện."
                      }`;
                }
              )
            }
          >
            Xuất bản
          </Button>
        )}
        {recipe.status === "Published" && (
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={busy}
            onClick={() =>
              setPendingConfirm({ kind: "unpublish", recipe })
            }
          >
            Gỡ xuất bản
          </Button>
        )}
        {recipe.status === "Archived" && (
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={busy}
            onClick={() => setPendingConfirm({ kind: "unarchive", recipe })}
          >
            Khôi phục
          </Button>
        )}
        {recipe.status !== "Archived" && (
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={busy}
            onClick={() => setPendingConfirm({ kind: "archive", recipe })}
          >
            Lưu trữ
          </Button>
        )}
        <Button
          type="button"
          variant="destructive"
          size="sm"
          disabled={busy}
          onClick={() => setDeleteTarget(recipe)}
        >
          Xóa
        </Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Công thức của tôi"
        description={`${totalCount} công thức trong mục này.`}
        actions={
          <Button asChild>
            <Link href="/dashboard/recipes/new">
              <Plus />
              Tạo công thức
            </Link>
          </Button>
        }
      />

      <Tabs value={tab} onValueChange={handleTabChange}>
        <TabsList className="w-fit">
          {TABS.map((item) => (
            <TabsTrigger key={item.value} value={item.value}>
              {item.label}
            </TabsTrigger>
          ))}
        </TabsList>
      </Tabs>

      <p className="-mt-3 text-xs text-muted-foreground">
        Quy trình trạng thái: <span className="font-medium">Nháp</span> →{" "}
        <span className="font-medium">Đã xuất bản</span> →{" "}
        <span className="font-medium">Lưu trữ</span>. Công thức đã lưu trữ có
        thể khôi phục về Nháp, công thức đã xóa mềm sẽ không còn hiển thị.
      </p>

      {error && (
        <Alert variant="destructive">
          <AlertTitle>Không tải được dữ liệu</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {loading ? (
        <RecipeListSkeleton count={6} />
      ) : items.length === 0 ? (
        <EmptyState
          title="Chưa có công thức nào"
          description="Công thức bạn tạo sẽ xuất hiện ở đây để quản lý và xuất bản."
          action={
            <Button asChild>
              <Link href="/dashboard/recipes/new">Viết công thức đầu tiên</Link>
            </Button>
          }
        />
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {items.map((recipe) => (
            <Card
              key={recipe.id}
              className="gap-0 overflow-hidden p-0 transition hover:shadow-md"
            >
              <div className="aspect-[16/10] w-full overflow-hidden bg-muted">
                {recipe.primaryImageUrl ? (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img
                    src={recipe.primaryImageUrl}
                    alt={recipe.title}
                    loading="lazy"
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <div className="flex h-full w-full items-center justify-center text-sm text-muted-foreground">
                    Chưa có ảnh
                  </div>
                )}
              </div>
              <div className="flex flex-1 flex-col gap-2 p-4">
                <StatusBadge status={recipe.status} />
                <Link
                  href={`/recipes/${encodeURIComponent(recipe.slug)}`}
                  className="line-clamp-2 font-semibold leading-snug hover:underline"
                >
                  {recipe.title}
                </Link>
                <p className="line-clamp-2 text-sm text-muted-foreground">
                  {recipe.description}
                </p>
                {renderActions(recipe)}
              </div>
            </Card>
          ))}
        </div>
      )}

      <Pagination
        page={page}
        totalPages={totalPages}
        onPageChange={handlePageChange}
        disabled={loading || Boolean(acting)}
      />

      <ConfirmDialog
        open={Boolean(pendingConfirm)}
        onOpenChange={(open) => {
          if (!open) setPendingConfirm(null);
        }}
        title={pendingConfirm ? CONFIRM_COPY[pendingConfirm.kind].title : ""}
        description={
          pendingConfirm
            ? `Công thức "${pendingConfirm.recipe.title}" ${
                CONFIRM_COPY[pendingConfirm.kind].verb
              }.`
            : undefined
        }
        confirmLabel={
          pendingConfirm
            ? CONFIRM_COPY[pendingConfirm.kind].confirmLabel
            : "Xác nhận"
        }
        loading={Boolean(
          pendingConfirm && acting === pendingConfirm.recipe.id
        )}
        onConfirm={() => {
          if (!pendingConfirm) return;
          const { kind, recipe } = pendingConfirm;
          const action =
            kind === "archive"
              ? archiveRecipe
              : kind === "unpublish"
                ? unpublishRecipe
                : unarchiveRecipe;
          void runAction(
            recipe,
            () => action(recipe.id, token),
            CONFIRM_COPY[kind].success
          );
        }}
      />

      <ConfirmDialog
        open={Boolean(deleteTarget)}
        onOpenChange={(open) => {
          if (!open) setDeleteTarget(null);
        }}
        title="Xóa công thức?"
        description={
          deleteTarget
            ? `Công thức "${deleteTarget.title}" sẽ bị xóa mềm và không còn hiển thị. Bạn có chắc chắn?`
            : undefined
        }
        confirmLabel="Xóa"
        destructive
        loading={Boolean(deleteTarget && acting === deleteTarget.id)}
        onConfirm={() => {
          if (!deleteTarget) return;
          void runAction(
            deleteTarget,
            () => deleteRecipe(deleteTarget.id, token),
            "Đã xóa công thức."
          );
        }}
      />
    </div>
  );
}

"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import {
  ApiError,
  archiveRecipe,
  deleteRecipe,
  getMyRecipes,
  publishRecipe,
  unpublishRecipe,
  type RecipeSummary,
} from "@/lib/api";
import { RecipeListSkeleton } from "@/components/RecipeSkeleton";

type StatusTab = "all" | "draft" | "published" | "archived";

const TABS: { value: StatusTab; label: string }[] = [
  { value: "all", label: "Tất cả" },
  { value: "draft", label: "Nháp" },
  { value: "published", label: "Đã xuất bản" },
  { value: "archived", label: "Lưu trữ" },
];

const STATUS_LABEL: Record<string, string> = {
  Draft: "Nháp",
  Published: "Đã xuất bản",
  Archived: "Lưu trữ",
};

function getAccessToken(): string | undefined {
  if (typeof window === "undefined") return undefined;
  return (
    window.localStorage.getItem("accessToken") ??
    window.localStorage.getItem("access_token") ??
    undefined
  );
}

export default function DashboardRecipesPage() {
  const [tab, setTab] = useState<StatusTab>("all");
  const [items, setItems] = useState<RecipeSummary[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [acting, setActing] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);

  const pageSize = 12;

  const load = useCallback(async (nextTab: StatusTab, nextPage: number) => {
    setLoading(true);
    setError(null);
    try {
      const data = await getMyRecipes(nextPage, pageSize, nextTab, getAccessToken());
      setItems(data.items);
      setTotalCount(data.totalCount);
      setPage(data.page);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401)
        setError("Bạn cần đăng nhập để xem dashboard.");
      else setError("Không tải được danh sách. Vui lòng thử lại.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load(tab, 1);
  }, [tab, load]);

  const refresh = () => load(tab, page);

  const runAction = async (
    id: string,
    action: () => Promise<unknown>,
    okMessage: string
  ) => {
    setActing(id);
    setError(null);
    setNotice(null);
    try {
      await action();
      setNotice(okMessage);
      await refresh();
    } catch (err) {
      if (err instanceof ApiError) {
        if (err.status === 401) setError("Bạn cần đăng nhập để thực hiện.");
        else if (err.status === 403) setError("Bạn không có quyền với công thức này.");
        else if (err.status === 409) setError("Dữ liệu đã bị thay đổi, vui lòng tải lại.");
        else setError(err.message);
      } else setError("Có lỗi xảy ra. Vui lòng thử lại.");
    } finally {
      setActing(null);
    }
  };

  const handleDelete = (id: string, title: string) => {
    if (!window.confirm(`Xóa vĩnh viễn "${title}"?`)) return;
    runAction(id, () => deleteRecipe(id, getAccessToken()), "Đã xóa công thức.");
  };

  const totalPages = Math.max(Math.ceil(totalCount / pageSize), 1);

  const actionsFor = (r: RecipeSummary) => {
    const token = getAccessToken();
    const busy = acting === r.id;
    const btn =
      "rounded-full border px-3 py-1 text-xs hover:bg-zinc-100 disabled:opacity-50 dark:hover:bg-zinc-800";
    return (
      <div className="flex flex-wrap gap-2 pt-2">
        <Link
          href={`/recipes/${encodeURIComponent(r.slug)}/edit`}
          className="rounded-full border px-3 py-1 text-xs hover:bg-zinc-100 dark:hover:bg-zinc-800"
        >
          Sửa
        </Link>
        {r.status === "Draft" && (
          <button
            type="button"
            disabled={busy}
            onClick={() =>
              runAction(r.id, () => publishRecipe(r.id, token), "Đã xuất bản.")
            }
            className={btn}
          >
            Xuất bản
          </button>
        )}
        {r.status !== "Draft" && (
          <button
            type="button"
            disabled={busy}
            onClick={() =>
              runAction(r.id, () => unpublishRecipe(r.id, token), "Đã gỡ xuất bản.")
            }
            className={btn}
          >
            Gỡ xuất bản
          </button>
        )}
        {r.status !== "Archived" && (
          <button
            type="button"
            disabled={busy}
            onClick={() =>
              runAction(r.id, () => archiveRecipe(r.id, token), "Đã lưu trữ.")
            }
            className={btn}
          >
            Lưu trữ
          </button>
        )}
        <button
          type="button"
          disabled={busy}
          onClick={() => handleDelete(r.id, r.title)}
          className="rounded-full border border-red-300 px-3 py-1 text-xs text-red-600 hover:bg-red-50 disabled:opacity-50"
        >
          Xóa
        </button>
      </div>
    );
  };

  return (
    <main className="mx-auto flex w-full max-w-6xl flex-1 flex-col gap-6 p-4 sm:p-6">
      <div className="flex items-center justify-between gap-4">
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-bold sm:text-3xl">Công thức của tôi</h1>
          <p className="text-sm text-zinc-500">{totalCount} công thức</p>
        </div>
        <Link
          href="/recipes/create"
          className="shrink-0 rounded-full bg-zinc-900 px-4 py-2 text-sm font-medium text-white hover:bg-zinc-700 dark:bg-zinc-100 dark:text-zinc-900 dark:hover:bg-white"
        >
          Tạo mới
        </Link>
      </div>

      <div className="flex gap-2">
        {TABS.map((t) => (
          <button
            key={t.value}
            type="button"
            onClick={() => setTab(t.value)}
            className={`rounded-full px-4 py-1.5 text-sm transition ${
              tab === t.value
                ? "bg-zinc-900 text-white dark:bg-zinc-100 dark:text-zinc-900"
                : "border hover:bg-zinc-100 dark:hover:bg-zinc-800"
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {error && (
        <p className="rounded-lg border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700">
          {error}
        </p>
      )}
      {notice && (
        <p className="rounded-lg border border-green-300 bg-green-50 px-3 py-2 text-sm text-green-800">
          {notice}
        </p>
      )}

      {loading ? (
        <RecipeListSkeleton count={6} />
      ) : items.length === 0 ? (
        <p className="text-sm text-zinc-500">Chưa có công thức nào trong mục này.</p>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((r) => (
            <div
              key={r.id}
              className="flex flex-col overflow-hidden rounded-xl border border-zinc-200 bg-white dark:border-zinc-800 dark:bg-zinc-900"
            >
              <div className="aspect-[16/10] w-full overflow-hidden bg-zinc-100 dark:bg-zinc-800">
                {r.primaryImageUrl ? (
                  // eslint-disable-next-line @next/next/no-img-element
                  <img
                    src={r.primaryImageUrl}
                    alt={r.title}
                    loading="lazy"
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <div className="flex h-full w-full items-center justify-center text-sm text-zinc-400">
                    Chưa có ảnh
                  </div>
                )}
              </div>
              <div className="flex flex-1 flex-col gap-1 p-4">
                <span className="w-fit rounded-full bg-zinc-100 px-2 py-0.5 text-xs text-zinc-600 dark:bg-zinc-800 dark:text-zinc-300">
                  {STATUS_LABEL[r.status] ?? r.status}
                </span>
                <Link
                  href={`/recipes/${encodeURIComponent(r.slug)}`}
                  className="font-semibold leading-snug hover:underline"
                >
                  {r.title}
                </Link>
                <p className="line-clamp-2 text-sm text-zinc-500 dark:text-zinc-400">
                  {r.description}
                </p>
                {actionsFor(r)}
              </div>
            </div>
          ))}
        </div>
      )}

      <div className="flex items-center justify-between pt-2 text-sm">
        <span>
          Trang {page} / {totalPages}
        </span>
        <div className="flex gap-2">
          <button
            type="button"
            disabled={page <= 1 || loading}
            onClick={() => load(tab, page - 1)}
            className="rounded-full border px-4 py-2 hover:bg-zinc-100 disabled:opacity-50 dark:hover:bg-zinc-800"
          >
            Trước
          </button>
          <button
            type="button"
            disabled={page >= totalPages || loading}
            onClick={() => load(tab, page + 1)}
            className="rounded-full border px-4 py-2 hover:bg-zinc-100 disabled:opacity-50 dark:hover:bg-zinc-800"
          >
            Sau
          </button>
        </div>
      </div>
    </main>
  );
}

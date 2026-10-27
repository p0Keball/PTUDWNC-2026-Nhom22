"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import {
  ArrowRight,
  BookOpen,
  FileText,
  PenLine,
  Send,
  Archive,
} from "lucide-react";

import { PageHeader } from "@/components/PageHeader";
import { EmptyState } from "@/components/EmptyState";
import { ErrorState } from "@/components/ErrorState";
import { Spinner } from "@/components/Loading";
import { StatusBadge } from "@/components/StatusBadge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { getMyRecipes, type RecipeSummary } from "@/lib/api";
import { formatDate } from "@/lib/utils";
import { useAuthStore } from "@/store/auth-store";

interface Stats {
  all: number;
  draft: number;
  published: number;
  archived: number;
}

const EMPTY_STATS: Stats = { all: 0, draft: 0, published: 0, archived: 0 };

export default function DashboardPage() {
  const token = useAuthStore((state) => state.accessToken);
  const user = useAuthStore((state) => state.user);

  const [stats, setStats] = useState<Stats>(EMPTY_STATS);
  const [recent, setRecent] = useState<RecipeSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    Promise.all([
      getMyRecipes(1, 1, "all", token),
      getMyRecipes(1, 1, "draft", token),
      getMyRecipes(1, 1, "published", token),
      getMyRecipes(1, 1, "archived", token),
      getMyRecipes(1, 4, "all", token),
    ])
      .then(([all, draft, published, archived, latest]) => {
        if (cancelled) return;
        setStats({
          all: all.totalCount,
          draft: draft.totalCount,
          published: published.totalCount,
          archived: archived.totalCount,
        });
        setRecent(latest.items);
        setError(null);
      })
      .catch(() => {
        if (!cancelled)
          setError("Không tải được dashboard. Vui lòng thử lại sau.");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [token]);

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title={`Xin chào${user?.fullName ? `, ${user.fullName}` : ""}!`}
        description="Quản lý công thức, theo dõi trạng thái xuất bản và viết nội dung mới."
        actions={
          <>
            <Button asChild>
              <Link href="/dashboard/recipes/new">
                <PenLine />
                Viết công thức
              </Link>
            </Button>
            <Button asChild variant="outline">
              <Link href="/dashboard/recipes">
                Quản lý công thức
                <ArrowRight />
              </Link>
            </Button>
          </>
        }
      />

      {loading ? (
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Spinner />
          Đang tải dashboard…
        </div>
      ) : error ? (
        <ErrorState title="Không tải được dashboard" message={error} onRetry={() => window.location.reload()} />
      ) : (
        <>
          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            {[
              { label: "Tổng công thức", value: stats.all, icon: BookOpen },
              { label: "Bản nháp", value: stats.draft, icon: FileText },
              { label: "Đã xuất bản", value: stats.published, icon: Send },
              { label: "Lưu trữ", value: stats.archived, icon: Archive },
            ].map((item) => (
              <Card key={item.label}>
                <CardContent className="flex items-center gap-3 pt-6">
                  <div className="flex size-10 items-center justify-center rounded-lg bg-muted">
                    <item.icon className="size-5" />
                  </div>
                  <div>
                    <p className="text-2xl font-bold leading-none">{item.value}</p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      {item.label}
                    </p>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          <Card>
            <CardHeader className="flex-row items-center justify-between gap-2">
              <CardTitle className="text-base">Công thức gần đây</CardTitle>
              <Button asChild variant="ghost" size="sm">
                <Link href="/dashboard/recipes">
                  Xem tất cả
                  <ArrowRight />
                </Link>
              </Button>
            </CardHeader>
            <CardContent>
              {recent.length === 0 ? (
                <EmptyState
                  title="Chưa có công thức nào"
                  description="Bắt đầu chia sẻ công thức đầu tiên của bạn."
                  action={
                    <Button asChild>
                      <Link href="/dashboard/recipes/new">Viết công thức</Link>
                    </Button>
                  }
                />
              ) : (
                <ul className="flex flex-col divide-y">
                  {recent.map((recipe) => (
                    <li
                      key={recipe.id}
                      className="flex flex-col gap-1 py-3 first:pt-0 last:pb-0 sm:flex-row sm:items-center sm:justify-between"
                    >
                      <div className="flex min-w-0 flex-col gap-1">
                        <Link
                          href={`/recipes/${encodeURIComponent(recipe.slug)}`}
                          className="truncate font-medium hover:underline"
                        >
                          {recipe.title}
                        </Link>
                        <p className="text-xs text-muted-foreground">
                          Cập nhật: {formatDate(recipe.publishedAt)}
                        </p>
                      </div>
                      <div className="flex items-center gap-2">
                        <StatusBadge status={recipe.status} />
                        <Button asChild variant="outline" size="sm">
                          <Link
                            href={`/dashboard/recipes/${encodeURIComponent(recipe.slug)}/edit`}
                          >
                            Sửa
                          </Link>
                        </Button>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>
        </>
      )}
    </div>
  );
}

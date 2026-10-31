"use client";

import { useCallback, useEffect, useState, type FormEvent } from "react";
import {
  Search,
  ShieldCheck,
  UserCheck,
  UserRound,
  UserX,
  Users,
} from "lucide-react";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/ConfirmDialog";
import { EmptyState } from "@/components/EmptyState";
import { PageLoading } from "@/components/Loading";
import { PageHeader } from "@/components/PageHeader";
import Pagination from "@/components/Pagination";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import {
  getAdminUsers,
  setAdminUserRole,
  setAdminUserStatus,
  type AdminUserItem,
  type AdminUserListResponse,
} from "@/lib/api";
import { getErrorMessage } from "@/lib/errors";
import { useAuthStore } from "@/store/auth-store";

const PAGE_SIZE = 20;

const ROLE_LABELS: Record<string, string> = {
  Admin: "Quản trị viên",
  Author: "Tác giả",
};

const dateTimeFormatter = new Intl.DateTimeFormat("vi-VN", {
  dateStyle: "short",
  timeStyle: "short",
});

function formatDateTime(value: string | null | undefined) {
  if (!value) return "—";
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? "—" : dateTimeFormatter.format(date);
}

export default function AdminUsersPage() {
  const token = useAuthStore((state) => state.accessToken);
  const currentUserId = useAuthStore((state) => state.user?.id);

  const [data, setData] = useState<AdminUserListResponse | null>(null);
  const [page, setPage] = useState(1);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [role, setRole] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [sort, setSort] = useState("-createdAt");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const [pendingStatus, setPendingStatus] = useState<{
    user: AdminUserItem;
    isActive: boolean;
  } | null>(null);
  const [pendingRole, setPendingRole] = useState<{
    user: AdminUserItem;
    role: string;
  } | null>(null);

  const refresh = useCallback(
    async (targetPage: number, showLoading = false) => {
      if (showLoading) setLoading(true);
      try {
        const result = await getAdminUsers(
          {
            page: targetPage,
            pageSize: PAGE_SIZE,
            search,
            role,
            status: statusFilter,
            sort,
          },
          token
        );
        setData(result);
        setPage(result.page);
        setError(null);
      } catch (err) {
        setError(
          getErrorMessage(err, "Không tải được danh sách người dùng.")
        );
      } finally {
        setLoading(false);
      }
    },
    [role, search, sort, statusFilter, token]
  );

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      setLoading(true);
      try {
        const result = await getAdminUsers(
          {
            page,
            pageSize: PAGE_SIZE,
            search,
            role,
            status: statusFilter,
            sort,
          },
          token
        );
        if (cancelled) return;
        setData(result);
        setError(null);
      } catch (err) {
        if (!cancelled)
          setError(
            getErrorMessage(err, "Không tải được danh sách người dùng.")
          );
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [page, role, search, sort, statusFilter, token]);

  function applyFilter(updater: () => void) {
    updater();
    setPage(1);
  }

  function handleSearch(event: FormEvent) {
    event.preventDefault();
    applyFilter(() => setSearch(searchInput.trim()));
  }

  function resetFilters() {
    setSearchInput("");
    setSearch("");
    setRole("");
    setStatusFilter("");
    setSort("-createdAt");
    setPage(1);
  }

  async function handleStatusConfirm() {
    if (!pendingStatus) return;
    const { user, isActive } = pendingStatus;
    setBusyId(user.id);
    try {
      await setAdminUserStatus(user.id, isActive, token);
      toast.success(
        isActive
          ? `Đã kích hoạt tài khoản ${user.email}.`
          : `Đã vô hiệu hóa tài khoản ${user.email}.`
      );
      setPendingStatus(null);
      await refresh(page);
    } catch (err) {
      toast.error(
        getErrorMessage(err, "Cập nhật trạng thái người dùng thất bại.")
      );
    } finally {
      setBusyId(null);
    }
  }

  async function handleRoleConfirm() {
    if (!pendingRole) return;
    const { user, role: nextRole } = pendingRole;
    setBusyId(user.id);
    try {
      await setAdminUserRole(user.id, nextRole, token);
      toast.success(
        `Đã đổi role của ${user.email} thành ${
          ROLE_LABELS[nextRole] ?? nextRole
        }.`
      );
      setPendingRole(null);
      await refresh(page);
    } catch (err) {
      toast.error(getErrorMessage(err, "Đổi role người dùng thất bại."));
    } finally {
      setBusyId(null);
    }
  }

  const stats = data?.stats;
  const totalPages = Math.max(
    Math.ceil((data?.totalCount ?? 0) / PAGE_SIZE),
    1
  );

  const statCards = [
    {
      label: "Tổng người dùng",
      value: stats?.totalUsers ?? 0,
      icon: Users,
    },
    {
      label: "Đang hoạt động",
      value: stats?.activeUsers ?? 0,
      icon: UserCheck,
    },
    { label: "Quản trị viên", value: stats?.admins ?? 0, icon: ShieldCheck },
    { label: "Tác giả", value: stats?.authors ?? 0, icon: UserRound },
  ];

  return (
    <div className="flex flex-col gap-6">
      <PageHeader
        title="Quản lý người dùng"
        description="Xem danh sách Admin/Tác giả, kích hoạt hoặc vô hiệu hóa tài khoản và đổi role."
      />

      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        {statCards.map((item) => (
          <Card key={item.label} className="gap-0 py-4">
            <CardContent className="flex items-center gap-3 px-4">
              <span className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-secondary text-secondary-foreground">
                <item.icon className="size-4" />
              </span>
              <div className="min-w-0">
                <p className="text-xs text-muted-foreground">{item.label}</p>
                <p className="text-xl font-semibold tabular-nums">
                  {item.value}
                </p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card className="gap-0 py-4">
        <CardContent className="flex flex-col gap-3 px-4">
          <form
            onSubmit={handleSearch}
            className="flex flex-col gap-2 sm:flex-row sm:items-center"
          >
            <div className="relative flex-1">
              <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                value={searchInput}
                onChange={(event) => setSearchInput(event.target.value)}
                placeholder="Tìm theo email hoặc tên hiển thị…"
                aria-label="Tìm người dùng"
                className="pl-8"
              />
            </div>
            <Button type="submit" variant="secondary">
              Tìm kiếm
            </Button>
          </form>

          <div className="grid grid-cols-1 gap-2 sm:grid-cols-3">
            <label className="flex flex-col gap-1 text-xs text-muted-foreground">
              Role
              <Select
                value={role}
                onChange={(event) =>
                  applyFilter(() => setRole(event.target.value))
                }
                aria-label="Lọc theo role"
              >
                <option value="">Tất cả role</option>
                <option value="Admin">Quản trị viên</option>
                <option value="Author">Tác giả</option>
              </Select>
            </label>

            <label className="flex flex-col gap-1 text-xs text-muted-foreground">
              Trạng thái
              <Select
                value={statusFilter}
                onChange={(event) =>
                  applyFilter(() => setStatusFilter(event.target.value))
                }
                aria-label="Lọc theo trạng thái"
              >
                <option value="">Tất cả trạng thái</option>
                <option value="active">Đang hoạt động</option>
                <option value="inactive">Đã vô hiệu hóa</option>
              </Select>
            </label>

            <label className="flex flex-col gap-1 text-xs text-muted-foreground">
              Sắp xếp
              <Select
                value={sort}
                onChange={(event) =>
                  applyFilter(() => setSort(event.target.value))
                }
                aria-label="Sắp xếp danh sách"
              >
                <option value="-createdAt">Mới nhất trước</option>
                <option value="createdAt">Cũ nhất trước</option>
                <option value="name">Tên A → Z</option>
                <option value="-name">Tên Z → A</option>
                <option value="email">Email A → Z</option>
              </Select>
            </label>
          </div>

          <div className="flex items-center justify-between gap-2">
            <p className="text-xs text-muted-foreground">
              {data
                ? `${data.totalCount} người dùng phù hợp`
                : "Đang tải dữ liệu…"}
            </p>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={resetFilters}
              disabled={
                !search &&
                !role &&
                !statusFilter &&
                sort === "-createdAt" &&
                !searchInput
              }
            >
              Xóa bộ lọc
            </Button>
          </div>
        </CardContent>
      </Card>

      {error && (
        <Alert variant="destructive">
          <AlertTitle>Không tải được dữ liệu</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {loading && !data ? (
        <PageLoading label="Đang tải danh sách người dùng…" />
      ) : !data || data.items.length === 0 ? (
        <EmptyState
          title="Không có người dùng phù hợp"
          description="Thử thay đổi từ khóa hoặc bộ lọc để tìm người dùng khác."
          action={
            <Button type="button" variant="outline" onClick={resetFilters}>
              Xóa bộ lọc
            </Button>
          }
        />
      ) : (
        <div className="overflow-hidden rounded-xl border">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[880px] text-sm">
              <thead className="bg-muted/60 text-left text-xs uppercase tracking-wide text-muted-foreground">
                <tr>
                  <th scope="col" className="px-4 py-3 font-medium">
                    Người dùng
                  </th>
                  <th scope="col" className="px-4 py-3 font-medium">
                    Role
                  </th>
                  <th scope="col" className="px-4 py-3 font-medium">
                    Trạng thái
                  </th>
                  <th scope="col" className="px-4 py-3 font-medium">
                    Công thức
                  </th>
                  <th scope="col" className="px-4 py-3 font-medium">
                    Ngày tạo
                  </th>
                  <th scope="col" className="px-4 py-3 text-right font-medium">
                    Thao tác
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y">
                {data.items.map((user) => {
                  const isSelf = user.id === currentUserId;
                  const busy = busyId === user.id;
                  const initials =
                    user.displayName
                      .trim()
                      .split(/\s+/)
                      .slice(-2)
                      .map((part) => part[0]?.toUpperCase())
                      .join("") || "?";

                  return (
                    <tr key={user.id} className="align-middle">
                      <td className="px-4 py-3">
                        <div className="flex items-center gap-3">
                          <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-semibold text-primary">
                            {initials}
                          </span>
                          <div className="min-w-0">
                            <p className="flex items-center gap-2 truncate font-medium">
                              {user.displayName}
                              {isSelf && (
                                <Badge variant="outline" className="shrink-0">
                                  Bạn
                                </Badge>
                              )}
                            </p>
                            <p className="truncate text-xs text-muted-foreground">
                              {user.email}
                            </p>
                          </div>
                        </div>
                      </td>
                      <td className="px-4 py-3">
                        <Select
                          value={
                            user.role === "Admin" || user.role === "Author"
                              ? user.role
                              : "Author"
                          }
                          disabled={busy || isSelf}
                          aria-label={`Đổi role của ${user.displayName}`}
                          className="h-8 w-36"
                          onChange={(event) => {
                            const nextRole = event.target.value;
                            if (nextRole === user.role) return;
                            setPendingRole({ user, role: nextRole });
                          }}
                        >
                          <option value="Admin">Quản trị viên</option>
                          <option value="Author">Tác giả</option>
                        </Select>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex flex-col items-start gap-1">
                          <Badge
                            variant={user.isActive ? "success" : "warning"}
                          >
                            {user.isActive
                              ? "Đang hoạt động"
                              : "Đã vô hiệu hóa"}
                          </Badge>
                          {user.isLockedOut && (
                            <Badge variant="destructive">Đang bị khóa</Badge>
                          )}
                        </div>
                      </td>
                      <td className="px-4 py-3 tabular-nums text-muted-foreground">
                        {user.recipeCount}
                      </td>
                      <td className="px-4 py-3 whitespace-nowrap text-muted-foreground">
                        {formatDateTime(user.createdAt)}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex justify-end">
                          <Button
                            type="button"
                            variant={user.isActive ? "outline" : "default"}
                            size="sm"
                            disabled={busy || (isSelf && user.isActive)}
                            title={
                              isSelf && user.isActive
                                ? "Không thể tự vô hiệu hóa tài khoản đang đăng nhập"
                                : undefined
                            }
                            onClick={() =>
                              setPendingStatus({
                                user,
                                isActive: !user.isActive,
                              })
                            }
                          >
                            {user.isActive ? <UserX /> : <UserCheck />}
                            {user.isActive ? "Vô hiệu hóa" : "Kích hoạt"}
                          </Button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <Pagination
        page={page}
        totalPages={totalPages}
        onPageChange={(nextPage) => void refresh(nextPage, true)}
        disabled={loading || Boolean(busyId)}
      />

      <ConfirmDialog
        open={Boolean(pendingStatus)}
        onOpenChange={(open) => {
          if (!open) setPendingStatus(null);
        }}
        title={
          pendingStatus?.isActive
            ? "Kích hoạt tài khoản?"
            : "Vô hiệu hóa tài khoản?"
        }
        description={
          pendingStatus
            ? pendingStatus.isActive
              ? `${pendingStatus.user.email} sẽ có thể đăng nhập và sử dụng hệ thống trở lại.`
              : `${pendingStatus.user.email} sẽ bị đăng xuất khỏi mọi phiên và không thể đăng nhập cho đến khi được kích hoạt lại.`
            : undefined
        }
        confirmLabel={pendingStatus?.isActive ? "Kích hoạt" : "Vô hiệu hóa"}
        destructive={pendingStatus ? !pendingStatus.isActive : false}
        loading={Boolean(pendingStatus && busyId === pendingStatus.user.id)}
        onConfirm={() => void handleStatusConfirm()}
      />

      <ConfirmDialog
        open={Boolean(pendingRole)}
        onOpenChange={(open) => {
          if (!open) setPendingRole(null);
        }}
        title="Đổi role người dùng?"
        description={
          pendingRole
            ? `${pendingRole.user.email} sẽ được chuyển từ ${
                ROLE_LABELS[pendingRole.user.role] ?? pendingRole.user.role
              } sang ${ROLE_LABELS[pendingRole.role] ?? pendingRole.role}.`
            : undefined
        }
        confirmLabel="Đổi role"
        loading={Boolean(pendingRole && busyId === pendingRole.user.id)}
        onConfirm={() => void handleRoleConfirm()}
      />
    </div>
  );
}

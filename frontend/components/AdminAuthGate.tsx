"use client";

import { useEffect, useRef, useState } from "react";
import Link from "next/link";
import { LockKeyhole, ShieldAlert } from "lucide-react";

import { PageLoading } from "@/components/Loading";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import type { AuthUser } from "@/store/auth-store";
import { useAuthStore } from "@/store/auth-store";

function isAdmin(user: AuthUser | null) {
  if (!user) return false;
  return user.role === "Admin" || Boolean(user.roles?.includes("Admin"));
}

/**
 * Chặn khu vực /admin với người dùng không có role Admin.
 *
 * - Cookie-auth: khi localStorage chưa có role (hoặc role cũ) phải gọi /auth/me
 *   một lần để xác thực lại trước khi kết luận 403.
 * - Không render nội dung admin trong lúc đang xác thực để tránh lộ UI.
 */
export function AdminAuthGate({ children }: { children: React.ReactNode }) {
  const status = useAuthStore((state) => state.status);
  const hasHydrated = useAuthStore((state) => state.hasHydrated);
  const user = useAuthStore((state) => state.user);
  const fetchMe = useAuthStore((state) => state.fetchMe);
  const [fetchFinished, setFetchFinished] = useState(false);
  const checked = useRef(false);
  const admin = isAdmin(user);

  useEffect(() => {
    if (!hasHydrated || checked.current) return;
    checked.current = true;

    if (isAdmin(useAuthStore.getState().user)) return;

    let cancelled = false;
    void fetchMe().finally(() => {
      if (!cancelled) setFetchFinished(true);
    });

    return () => {
      cancelled = true;
    };
  }, [fetchMe, hasHydrated]);

  const verifying = !admin && !fetchFinished;

  if (!hasHydrated || status === "idle" || status === "loading" || verifying) {
    return <PageLoading label="Đang kiểm tra quyền quản trị…" />;
  }

  if (status !== "authenticated") {
    return (
      <Card className="mx-auto w-full max-w-xl">
        <CardHeader>
          <div className="mb-2 flex size-10 items-center justify-center rounded-full bg-muted">
            <LockKeyhole className="size-5" />
          </div>
          <CardTitle>Bạn cần đăng nhập</CardTitle>
          <CardDescription>
            Khu vực quản trị chỉ dành cho tài khoản Admin. Vui lòng đăng nhập để
            tiếp tục.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-2">
          <Button asChild>
            <Link href="/auth/login">Đăng nhập</Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/recipes">Xem công thức công khai</Link>
          </Button>
        </CardContent>
      </Card>
    );
  }

  if (!isAdmin(user)) {
    return (
      <Card className="mx-auto w-full max-w-xl">
        <CardHeader>
          <div className="mb-2 flex size-10 items-center justify-center rounded-full bg-destructive/10 text-destructive">
            <ShieldAlert className="size-5" />
          </div>
          <CardTitle>Không có quyền truy cập</CardTitle>
          <CardDescription>
            Tài khoản của bạn không có role Admin. Nếu bạn cho rằng đây là nhầm
            lẫn, hãy liên hệ quản trị viên hệ thống.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-2">
          <Button asChild>
            <Link href="/dashboard">Về dashboard</Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/">Về trang chủ</Link>
          </Button>
        </CardContent>
      </Card>
    );
  }

  return <>{children}</>;
}

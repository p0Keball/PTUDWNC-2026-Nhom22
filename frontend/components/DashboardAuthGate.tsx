"use client";

import { useEffect, useRef } from "react";
import Link from "next/link";
import { LockKeyhole } from "lucide-react";

import { PageLoading } from "@/components/Loading";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useAuthStore } from "@/store/auth-store";

export function DashboardAuthGate({
  children,
}: {
  children: React.ReactNode;
}) {
  const status = useAuthStore((state) => state.status);
  const hasHydrated = useAuthStore((state) => state.hasHydrated);
  const accessToken = useAuthStore((state) => state.accessToken);
  const fetchMe = useAuthStore((state) => state.fetchMe);
  const checkedCookie = useRef(false);

  useEffect(() => {
    if (!hasHydrated || checkedCookie.current) return;
    checkedCookie.current = true;

    // Hỗ trợ cookie-auth hiện tại: nếu chưa có token nhưng cookie còn hiệu lực,
    // /auth/me sẽ trả profile và store chuyển sang authenticated.
    if (!accessToken && status !== "authenticated") {
      void fetchMe();
    }
  }, [accessToken, fetchMe, hasHydrated, status]);

  if (!hasHydrated || status === "idle" || status === "loading") {
    return <PageLoading label="Đang kiểm tra phiên đăng nhập…" />;
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
            Khu vực dashboard dành cho tác giả. Vui lòng đăng nhập để quản lý
            công thức của bạn.
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

  return <>{children}</>;
}

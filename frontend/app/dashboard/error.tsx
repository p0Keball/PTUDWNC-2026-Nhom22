"use client";

import { useEffect } from "react";

import { ErrorState } from "@/components/ErrorState";

export default function DashboardError({
  error,
  retry,
}: {
  error: Error & { digest?: string };
  retry: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <ErrorState
      title="Dashboard gặp sự cố"
      message="Không thể tải khu vực quản lý. Vui lòng thử lại."
      onRetry={retry}
    />
  );
}

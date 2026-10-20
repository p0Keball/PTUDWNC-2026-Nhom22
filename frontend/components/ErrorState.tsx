"use client";

import Link from "next/link";
import { AlertTriangle, RotateCcw } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface ErrorStateProps {
  title?: string;
  message?: string;
  onRetry?: () => void;
  retryLabel?: string;
  retryHref?: string;
  retryHrefLabel?: string;
  homeHref?: string;
  className?: string;
}

export function ErrorState({
  title = "Có lỗi xảy ra",
  message = "Không thể tải dữ liệu. Vui lòng thử lại.",
  onRetry,
  retryLabel = "Thử lại",
  retryHref,
  retryHrefLabel = "Thử lại",
  homeHref,
  className,
}: ErrorStateProps) {
  return (
    <div className={cn("flex w-full flex-col items-center gap-4 py-10", className)}>
      <Alert variant="destructive" className="max-w-xl">
        <AlertTriangle />
        <AlertTitle>{title}</AlertTitle>
        <AlertDescription>{message}</AlertDescription>
      </Alert>
      <div className="flex flex-wrap items-center gap-2">
        {onRetry && (
          <Button type="button" onClick={onRetry}>
            <RotateCcw />
            {retryLabel}
          </Button>
        )}
        {retryHref && (
          <Button asChild>
            <Link href={retryHref}>
              <RotateCcw />
              {retryHrefLabel}
            </Link>
          </Button>
        )}
        {homeHref && (
          <Button asChild variant="outline">
            <Link href={homeHref}>Về trang chủ</Link>
          </Button>
        )}
      </div>
    </div>
  );
}

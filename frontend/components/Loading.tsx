import { Loader2 } from "lucide-react";

import { cn } from "@/lib/utils";

export function Spinner({ className }: { className?: string }) {
  return (
    <Loader2
      className={cn("size-4 animate-spin text-muted-foreground", className)}
      aria-hidden
    />
  );
}

export function PageLoading({
  label = "Đang tải dữ liệu…",
  className,
}: {
  label?: string;
  className?: string;
}) {
  return (
    <div
      className={cn(
        "flex min-h-40 w-full flex-col items-center justify-center gap-3 text-sm text-muted-foreground",
        className
      )}
      role="status"
      aria-live="polite"
    >
      <Spinner className="size-6" />
      <span>{label}</span>
    </div>
  );
}

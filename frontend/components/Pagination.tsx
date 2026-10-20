"use client";

import Link from "next/link";
import { ChevronLeft, ChevronRight, MoreHorizontal } from "lucide-react";

import { Button, buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface PaginationProps {
  page: number;
  totalPages: number;
  /** Chế độ server: truyền hàm tạo href cho từng trang. */
  getHref?: (page: number) => string;
  /** Chế độ client: callback đổi trang. */
  onPageChange?: (page: number) => void;
  /** Tương thích API cũ. */
  prevHref?: string | null;
  nextHref?: string | null;
  onPrevious?: () => void;
  onNext?: () => void;
  disabled?: boolean;
}

type PageItem = number | "ellipsis-start" | "ellipsis-end";

function getPageItems(current: number, total: number): PageItem[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, index) => index + 1);
  }
  if (current <= 4) {
    return [1, 2, 3, 4, 5, "ellipsis-end", total];
  }
  if (current >= total - 3) {
    return [
      1,
      "ellipsis-start",
      total - 4,
      total - 3,
      total - 2,
      total - 1,
      total,
    ];
  }
  return [
    1,
    "ellipsis-start",
    current - 1,
    current,
    current + 1,
    "ellipsis-end",
    total,
  ];
}

export default function Pagination({
  page,
  totalPages,
  getHref,
  onPageChange,
  prevHref,
  nextHref,
  onPrevious,
  onNext,
  disabled = false,
}: PaginationProps) {
  if (totalPages <= 1) return null;

  const hasPageLinks = Boolean(getHref || onPageChange);
  const previousDisabled = disabled || page <= 1;
  const nextDisabled = disabled || page >= totalPages;

  function renderPrevious() {
    if (getHref) {
      return previousDisabled ? (
        <span
          className={cn(
            buttonVariants({ variant: "outline", size: "sm" }),
            "pointer-events-none opacity-50"
          )}
        >
          <ChevronLeft />
          Trước
        </span>
      ) : (
        <Link
          href={getHref(page - 1)}
          className={buttonVariants({ variant: "outline", size: "sm" })}
        >
          <ChevronLeft />
          Trước
        </Link>
      );
    }
    if (prevHref) {
      return (
        <Link
          href={prevHref}
          className={buttonVariants({ variant: "outline", size: "sm" })}
        >
          <ChevronLeft />
          Trước
        </Link>
      );
    }
    return (
      <Button
        type="button"
        variant="outline"
        size="sm"
        disabled={previousDisabled}
        onClick={onPrevious ?? (() => onPageChange?.(page - 1))}
      >
        <ChevronLeft />
        Trước
      </Button>
    );
  }

  function renderNext() {
    if (getHref) {
      return nextDisabled ? (
        <span
          className={cn(
            buttonVariants({ variant: "outline", size: "sm" }),
            "pointer-events-none opacity-50"
          )}
        >
          Sau
          <ChevronRight />
        </span>
      ) : (
        <Link
          href={getHref(page + 1)}
          className={buttonVariants({ variant: "outline", size: "sm" })}
        >
          Sau
          <ChevronRight />
        </Link>
      );
    }
    if (nextHref) {
      return (
        <Link
          href={nextHref}
          className={buttonVariants({ variant: "outline", size: "sm" })}
        >
          Sau
          <ChevronRight />
        </Link>
      );
    }
    return (
      <Button
        type="button"
        variant="outline"
        size="sm"
        disabled={nextDisabled}
        onClick={onNext ?? (() => onPageChange?.(page + 1))}
      >
        Sau
        <ChevronRight />
      </Button>
    );
  }

  return (
    <div className="flex flex-col gap-3 pt-2 text-sm sm:flex-row sm:items-center sm:justify-between">
      <span className="text-muted-foreground">
        Trang <span className="font-medium text-foreground">{page}</span> /{" "}
        {totalPages}
      </span>

      <nav aria-label="Phân trang" className="flex items-center gap-1">
        {renderPrevious()}

        {hasPageLinks && (
          <div className="hidden items-center gap-1 sm:flex">
            {getPageItems(page, totalPages).map((item) => {
              if (typeof item !== "number") {
                return (
                  <span
                    key={item}
                    className="flex size-8 items-center justify-center text-muted-foreground"
                    aria-hidden
                  >
                    <MoreHorizontal className="size-4" />
                  </span>
                );
              }

              const isCurrent = item === page;
              if (getHref) {
                return (
                  <Link
                    key={item}
                    href={getHref(item)}
                    aria-current={isCurrent ? "page" : undefined}
                    className={cn(
                      buttonVariants({
                        variant: isCurrent ? "default" : "ghost",
                        size: "sm",
                      }),
                      "min-w-8 px-2"
                    )}
                  >
                    {item}
                  </Link>
                );
              }

              return (
                <Button
                  key={item}
                  type="button"
                  size="sm"
                  variant={isCurrent ? "default" : "ghost"}
                  aria-current={isCurrent ? "page" : undefined}
                  disabled={disabled}
                  onClick={() => onPageChange?.(item)}
                  className="min-w-8 px-2"
                >
                  {item}
                </Button>
              );
            })}
          </div>
        )}

        {renderNext()}
      </nav>
    </div>
  );
}

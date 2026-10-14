"use client";

import Link from "next/link";

interface PaginationProps {
  page: number;
  totalPages: number;
  prevHref?: string | null;
  nextHref?: string | null;
  onPrevious?: () => void;
  onNext?: () => void;
  disabled?: boolean;
}

const buttonClass =
  "rounded-full border px-4 py-2 hover:bg-zinc-100 disabled:opacity-50 dark:hover:bg-zinc-800";

export default function Pagination({
  page,
  totalPages,
  prevHref,
  nextHref,
  onPrevious,
  onNext,
  disabled = false,
}: PaginationProps) {
  return (
    <div className="flex items-center justify-between pt-2 text-sm">
      <span>
        Trang {page} / {totalPages}
      </span>
      <div className="flex gap-2">
        {prevHref ? (
          <Link href={prevHref} className={buttonClass}>
            Trước
          </Link>
        ) : (
          onPrevious && (
            <button
              type="button"
              disabled={disabled || page <= 1}
              onClick={onPrevious}
              className={buttonClass}
            >
              Trước
            </button>
          )
        )}
        {nextHref ? (
          <Link href={nextHref} className={buttonClass}>
            Sau
          </Link>
        ) : (
          onNext && (
            <button
              type="button"
              disabled={disabled || page >= totalPages}
              onClick={onNext}
              className={buttonClass}
            >
              Sau
            </button>
          )
        )}
      </div>
    </div>
  );
}

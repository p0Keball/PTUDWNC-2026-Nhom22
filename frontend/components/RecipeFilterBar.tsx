"use client";

import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { useTransition } from "react";
import { Loader2, RotateCcw, SlidersHorizontal } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Select } from "@/components/ui/select";
import type { Category } from "@/lib/api";

const DIFFICULTIES = [
  { value: "1", label: "Dễ" },
  { value: "2", label: "Trung bình" },
  { value: "3", label: "Khó" },
  { value: "4", label: "Chuyên gia" },
];

const COOK_TIMES = [
  { value: "15", label: "≤ 15 phút" },
  { value: "30", label: "≤ 30 phút" },
  { value: "45", label: "≤ 45 phút" },
  { value: "60", label: "≤ 60 phút" },
  { value: "120", label: "≤ 120 phút" },
];

const SORTS = [
  { value: "-createdAt", label: "Mới nhất" },
  { value: "createdAt", label: "Cũ nhất" },
  { value: "title", label: "Tên A → Z" },
  { value: "-title", label: "Tên Z → A" },
  { value: "cookTime", label: "Nấu nhanh nhất" },
  { value: "-cookTime", label: "Nấu lâu nhất" },
];

interface RecipeFilterBarProps {
  categories: Category[];
  initialFilters: {
    categoryId?: string;
    difficulty?: string;
    maxCookTime?: string;
    sort?: string;
  };
  totalCount: number;
}

export default function RecipeFilterBar({
  categories,
  initialFilters,
  totalCount,
}: RecipeFilterBarProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [isPending, startTransition] = useTransition();

  const categoryId = initialFilters.categoryId ?? "";
  const difficulty = initialFilters.difficulty ?? "";
  const maxCookTime = initialFilters.maxCookTime ?? "";
  const sort = initialFilters.sort ?? "-createdAt";

  const hasFilters = Boolean(
    categoryId || difficulty || maxCookTime || (sort && sort !== "-createdAt")
  );

  const handleFilterChange = (key: string, value: string) => {
    const params = new URLSearchParams(searchParams.toString());
    if (value && value !== "-createdAt") {
      params.set(key, value);
    } else if (key === "sort" && value === "-createdAt") {
      params.delete(key);
    } else {
      params.delete(key);
    }

    // Reset về trang 1 khi thay đổi bộ lọc
    params.delete("page");

    startTransition(() => {
      const qs = params.toString();
      router.push(qs ? `${pathname}?${qs}` : pathname, { scroll: false });
    });
  };

  const handleResetFilters = () => {
    startTransition(() => {
      router.push(pathname, { scroll: false });
    });
  };

  return (
    <div className="flex flex-col gap-3 rounded-xl border bg-card p-4 shadow-sm dark:border-zinc-800">
      <div className="flex items-center justify-between pb-1 text-sm font-medium">
        <div className="flex items-center gap-2 text-muted-foreground">
          <SlidersHorizontal className="size-4" />
          <span>Bộ lọc thông minh (tự động cập nhật)</span>
          {isPending && <Loader2 className="size-3.5 animate-spin text-primary" />}
        </div>
        {hasFilters && (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={handleResetFilters}
            disabled={isPending}
            className="h-8 gap-1.5 text-xs text-muted-foreground hover:text-foreground"
          >
            <RotateCcw className="size-3" />
            Xóa tất cả bộ lọc
          </Button>
        )}
      </div>

      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        {/* Lọc theo Danh mục */}
        <div className="flex flex-col gap-1.5">
          <label htmlFor="categoryId" className="text-xs font-medium text-muted-foreground">
            Danh mục
          </label>
          <Select
            id="categoryId"
            name="categoryId"
            value={categoryId}
            onChange={(e) => handleFilterChange("categoryId", e.target.value)}
            disabled={isPending}
          >
            <option value="">Tất cả danh mục</option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>
                {cat.name}
              </option>
            ))}
          </Select>
        </div>

        {/* Lọc theo Độ khó */}
        <div className="flex flex-col gap-1.5">
          <label htmlFor="difficulty" className="text-xs font-medium text-muted-foreground">
            Độ khó
          </label>
          <Select
            id="difficulty"
            name="difficulty"
            value={difficulty}
            onChange={(e) => handleFilterChange("difficulty", e.target.value)}
            disabled={isPending}
          >
            <option value="">Tất cả độ khó</option>
            {DIFFICULTIES.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </Select>
        </div>

        {/* Lọc theo Thời gian nấu */}
        <div className="flex flex-col gap-1.5">
          <label htmlFor="maxCookTime" className="text-xs font-medium text-muted-foreground">
            Thời gian nấu
          </label>
          <Select
            id="maxCookTime"
            name="maxCookTime"
            value={maxCookTime}
            onChange={(e) => handleFilterChange("maxCookTime", e.target.value)}
            disabled={isPending}
          >
            <option value="">Không giới hạn</option>
            {COOK_TIMES.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </Select>
        </div>

        {/* Sắp xếp */}
        <div className="flex flex-col gap-1.5">
          <label htmlFor="sort" className="text-xs font-medium text-muted-foreground">
            Sắp xếp
          </label>
          <Select
            id="sort"
            name="sort"
            value={sort}
            onChange={(e) => handleFilterChange("sort", e.target.value)}
            disabled={isPending}
          >
            {SORTS.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </Select>
        </div>
      </div>
    </div>
  );
}

"use client";

import { useCallback, useEffect, useState } from "react";
import StepForm from "@/components/StepForm";
import IngredientForm from "@/components/IngredientForm";
import ImageUploader from "@/components/ImageUploader";
import {
  ApiError,
  getRecipeValidation,
  type RecipeImage,
  type RecipeIngredient,
  type RecipeStep,
  type RecipeValidation,
} from "@/lib/api";

interface Props {
  recipeId: string;
  steps: RecipeStep[];
  ingredients: RecipeIngredient[];
  images: RecipeImage[];
}

function validationErrorMessage(err: unknown): string {
  return err instanceof ApiError && err.status === 404
    ? "Không tìm thấy công thức để kiểm tra."
    : "Không tải được checklist. Vui lòng thử lại.";
}

function CheckRow({
  ok,
  label,
  count,
}: {
  ok: boolean;
  label: string;
  count: number;
}) {
  return (
    <div
      className={`rounded-lg border px-3 py-2 text-sm ${
        ok
          ? "border-green-300 bg-green-50 text-green-800 dark:border-green-900 dark:bg-green-950 dark:text-green-300"
          : "border-amber-300 bg-amber-50 text-amber-800 dark:border-amber-900 dark:bg-amber-950 dark:text-amber-300"
      }`}
    >
      <span className="mr-1 font-semibold">{ok ? "✓" : "!"}</span>
      {label} ({count})
    </div>
  );
}

/**
 * TUẦN 4 (Trần Gia Huy) — gắn StepForm + IngredientForm + ImageUploader vào trang edit.
 * Component tự quản lý state ảnh và checklist validation, không đụng logic RecipeForm/rowVersion.
 */
export default function RecipeContentEditor({
  recipeId,
  steps,
  ingredients,
  images: initialImages,
}: Props) {
  const [images, setImages] = useState<RecipeImage[]>(initialImages);
  const [validation, setValidation] = useState<RecipeValidation | null>(null);
  const [loadingValidation, setLoadingValidation] = useState(true);
  const [validationError, setValidationError] = useState<string | null>(null);

  const refreshValidation = useCallback(async () => {
    setLoadingValidation(true);
    setValidationError(null);
    try {
      setValidation(await getRecipeValidation(recipeId));
    } catch (err) {
      setValidationError(validationErrorMessage(err));
    } finally {
      setLoadingValidation(false);
    }
  }, [recipeId]);

  // Load lần đầu: setState nằm sau await nên không vi phạm rule set-state-in-effect.
  useEffect(() => {
    let cancelled = false;
    void (async () => {
      try {
        const next = await getRecipeValidation(recipeId);
        if (!cancelled) {
          setValidation(next);
          setValidationError(null);
        }
      } catch (err) {
        if (!cancelled) setValidationError(validationErrorMessage(err));
      } finally {
        if (!cancelled) setLoadingValidation(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [recipeId]);

  const handleImagesChange = useCallback(
    (next: RecipeImage[]) => {
      setImages(next);
      void refreshValidation();
    },
    [refreshValidation]
  );

  return (
    <section className="flex flex-col gap-6 rounded-2xl border border-zinc-200 p-4 dark:border-zinc-800 sm:p-6">
      <div className="flex flex-col gap-1">
        <h2 className="text-lg font-semibold">Nội dung công thức</h2>
        <p className="text-sm text-zinc-500">
          Thêm nguyên liệu, các bước và ảnh. Mỗi thay đổi được lưu ngay vào công thức.
        </p>
      </div>

      <StepForm
        recipeId={recipeId}
        defaultSteps={steps}
        onPersisted={refreshValidation}
      />

      <IngredientForm
        recipeId={recipeId}
        defaultIngredients={ingredients}
        onPersisted={refreshValidation}
      />

      <ImageUploader
        recipeId={recipeId}
        images={images}
        onChange={handleImagesChange}
      />

      <div className="flex flex-col gap-3 rounded-xl border border-zinc-200 bg-zinc-50 p-4 dark:border-zinc-800 dark:bg-zinc-900">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="flex flex-col gap-1">
            <h3 className="font-semibold">Kiểm tra trước khi xuất bản</h3>
            <p className="text-xs text-zinc-500">
              Checklist lấy trực tiếp từ backend. Nút xuất bản nằm ở trang “Công thức của tôi”.
            </p>
          </div>
          <button
            type="button"
            onClick={() => void refreshValidation()}
            disabled={loadingValidation}
            className="rounded-full border px-4 py-1.5 text-sm hover:bg-zinc-100 disabled:opacity-50 dark:hover:bg-zinc-800"
          >
            {loadingValidation ? "Đang kiểm tra…" : "Kiểm tra lại"}
          </button>
        </div>

        {validationError && (
          <p className="text-sm text-red-600 dark:text-red-400">{validationError}</p>
        )}

        {validation && (
          <>
            <div className="grid grid-cols-1 gap-2 sm:grid-cols-3">
              <CheckRow
                ok={validation.hasSteps}
                label="Có bước thực hiện"
                count={validation.stepsCount}
              />
              <CheckRow
                ok={validation.hasIngredients}
                label="Có nguyên liệu"
                count={validation.ingredientsCount}
              />
              <CheckRow
                ok={validation.hasCoverImage}
                label="Có ảnh"
                count={validation.imagesCount}
              />
            </div>

            {validation.issues.length > 0 ? (
              <ul className="list-disc space-y-1 pl-5 text-sm text-amber-700 dark:text-amber-400">
                {validation.issues.map((issue) => (
                  <li key={issue}>{issue}</li>
                ))}
              </ul>
            ) : (
              <p className="text-sm text-green-700 dark:text-green-400">
                Đã đủ điều kiện cơ bản để xuất bản.
              </p>
            )}
          </>
        )}
      </div>
    </section>
  );
}

"use client";

import { useRef, useState } from "react";
import { imageApi, uploadRecipeImage, type RecipeImage } from "@/lib/api";

const ALLOWED_TYPES = ["image/jpeg", "image/png", "image/webp", "image/avif"];
const MAX_SIZE = 5 * 1024 * 1024;

interface Props {
  recipeId: string;
  images: RecipeImage[];
  onChange: (images: RecipeImage[]) => void;
}

export default function ImageUploader({ recipeId, images, onChange }: Props) {
  const [progress, setProgress] = useState<number | null>(null);
  const [uploadingName, setUploadingName] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [preview, setPreview] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const abortRef = useRef<AbortController | null>(null);

  function validateClient(file: File): string | null {
    if (!ALLOWED_TYPES.includes(file.type))
      return "Chỉ chấp nhận ảnh JPEG, PNG, WebP, AVIF.";
    if (file.size > MAX_SIZE) return "File tối đa 5MB.";
    return null;
  }

  async function handleFiles(files: FileList | null) {
    if (!files || files.length === 0) return;
    const file = files[0];
    setError(null);

    const clientError = validateClient(file);
    if (clientError) {
      setError(clientError);
      return;
    }

    const objectUrl = URL.createObjectURL(file);
    setPreview(objectUrl);
    setUploadingName(file.name);
    setProgress(0);
    const controller = new AbortController();
    abortRef.current = controller;

    try {
      const created = await uploadRecipeImage(
        recipeId,
        file,
        (p) => setProgress(p),
        file.name,
        controller.signal
      );
      onChange([...images, created]);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Upload thất bại.");
    } finally {
      URL.revokeObjectURL(objectUrl);
      setPreview(null);
      setProgress(null);
      setUploadingName(null);
      abortRef.current = null;
      if (inputRef.current) inputRef.current.value = "";
    }
  }

  async function handleSetPrimary(id: string) {
    setError(null);
    try {
      const updated = await imageApi.setPrimary(recipeId, id);
      onChange(images.map((img) => ({ ...img, isPrimary: img.id === updated.id })));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Đặt ảnh chính thất bại.");
    }
  }

  async function handleDelete(id: string) {
    setError(null);
    try {
      await imageApi.remove(recipeId, id);
      const rest = images.filter((img) => img.id !== id);
      onChange(
        rest.map((img, idx) => ({
          ...img,
          isPrimary: rest.some((r) => r.isPrimary) ? img.isPrimary : idx === 0,
        }))
      );
    } catch (e) {
      setError(e instanceof Error ? e.message : "Xóa ảnh thất bại.");
    }
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={() => inputRef.current?.click()}
          disabled={progress !== null}
          className="rounded-full bg-zinc-900 px-4 py-2 text-sm text-white disabled:opacity-50 dark:bg-zinc-100 dark:text-zinc-900"
        >
          {progress !== null ? "Đang upload..." : "Chọn ảnh"}
        </button>
        {progress !== null && (
          <button
            type="button"
            onClick={() => abortRef.current?.abort()}
            className="rounded-full border px-4 py-2 text-sm"
          >
            Hủy
          </button>
        )}
        <input
          ref={inputRef}
          type="file"
          accept="image/jpeg,image/png,image/webp,image/avif"
          className="hidden"
          onChange={(e) => handleFiles(e.target.files)}
        />
        <span className="text-xs text-zinc-500">JPEG/PNG/WebP, tối đa 5MB.</span>
      </div>

      {preview && (
        // eslint-disable-next-line @next/next/no-img-element
        <img src={preview} alt="preview" className="aspect-[16/9] w-full rounded-xl object-cover" />
      )}
      {progress !== null && (
        <div className="flex flex-col gap-1">
          <div className="h-2 w-full overflow-hidden rounded-full bg-zinc-200 dark:bg-zinc-800">
            <div
              className="h-full rounded-full bg-zinc-900 transition-all dark:bg-zinc-100"
              style={{ width: `${progress}%` }}
            />
          </div>
          <span className="text-xs text-zinc-500">
            {uploadingName} — {progress}%
          </span>
        </div>
      )}

      {error && <p className="text-sm text-red-600">{error}</p>}

      {images.length > 0 && (
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
          {images.map((img) => (
            <div key={img.id} className="relative overflow-hidden rounded-xl border dark:border-zinc-800">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img src={img.originalUrl} alt={img.altText ?? "recipe image"} className="aspect-square w-full object-cover" loading="lazy" />
              {img.isPrimary && (
                <span className="absolute left-2 top-2 rounded-full bg-zinc-900 px-2 py-1 text-[11px] text-white">
                  Ảnh chính
                </span>
              )}
              <div className="flex gap-1 p-2">
                {!img.isPrimary && (
                  <button
                    type="button"
                    onClick={() => handleSetPrimary(img.id)}
                    className="flex-1 rounded-lg border px-2 py-1 text-xs hover:bg-zinc-100 dark:hover:bg-zinc-800"
                  >
                    Đặt chính
                  </button>
                )}
                <button
                  type="button"
                  onClick={() => handleDelete(img.id)}
                  className="flex-1 rounded-lg border px-2 py-1 text-xs text-red-600 hover:bg-red-50"
                >
                  Xóa
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

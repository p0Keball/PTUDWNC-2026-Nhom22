"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import {
  getStorageHealth,
  imageApi,
  uploadRecipeImage,
  type RecipeImage,
} from "@/lib/api";
import { compressImage, formatBytes } from "@/lib/compress";
import { thumbSrc, FALLBACK_IMG } from "@/components/RecipeGallery";

const ALLOWED_TYPES = ["image/jpeg", "image/png", "image/webp", "image/avif"];
const MAX_SIZE = 5 * 1024 * 1024;
const MAX_FILES_PER_DROP = 5;

interface Props {
  recipeId: string;
  images: RecipeImage[];
  onChange: (images: RecipeImage[]) => void;
}

interface UploadState {
  name: string;
  progress: number;
  status: string;
}

export default function ImageUploader({ recipeId, images, onChange }: Props) {
  const [uploads, setUploads] = useState<Record<string, UploadState>>({});
  const [error, setError] = useState<string | null>(null);
  const [dragOver, setDragOver] = useState(false);
  const [storageDown, setStorageDown] = useState(false);
  const [brokenIds, setBrokenIds] = useState<Set<string>>(new Set());
  const [lightboxId, setLightboxId] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const controllers = useRef<Map<string, AbortController>>(new Map());

  // TUẦN 4 — Storage disconnect: poll /storage/health mỗi 30s để disable upload + banner.
  useEffect(() => {
    let cancelled = false;
    async function check() {
      try {
        const h = await getStorageHealth();
        if (!cancelled) setStorageDown(h.status !== "healthy");
      } catch {
        if (!cancelled) setStorageDown(true);
      }
    }
    check();
    const t = setInterval(check, 30_000);
    return () => {
      cancelled = true;
      clearInterval(t);
    };
  }, []);

  function validateClient(file: File): string | null {
    if (!ALLOWED_TYPES.includes(file.type))
      return `“${file.name}”: chỉ chấp nhận JPEG, PNG, WebP, AVIF.`;
    if (file.size > MAX_SIZE)
      return `“${file.name}”: file tối đa 5MB (đang ${formatBytes(file.size)}).`;
    return null;
  }

  const uploadOne = useCallback(
    async (raw: File, current: RecipeImage[]) => {
      const key = `${raw.name}-${Date.now()}-${Math.random().toString(36).slice(2)}`;
      const controller = new AbortController();
      controllers.current.set(key, controller);
      setUploads((u) => ({ ...u, [key]: { name: raw.name, progress: 0, status: "Đang nén…" } }));
      try {
        // TUẦN 4 — Upload Optimization: nén client-side trước khi presign.
        const { file, wasCompressed, originalSize, compressedSize } = await compressImage(raw);
        setUploads((u) =>
          u[key]
            ? {
                ...u,
                [key]: {
                  ...u[key],
                  status: wasCompressed
                    ? `Đã nén ${formatBytes(originalSize)} → ${formatBytes(compressedSize)}. Đang upload…`
                    : "Đang upload…",
                },
              }
            : u
        );
        const created = await uploadRecipeImage(
          recipeId,
          file,
          (p) =>
            setUploads((u) =>
              u[key] ? { ...u, [key]: { ...u[key], progress: p, status: "Đang upload…" } } : u
            ),
          raw.name,
          controller.signal
        );
        current.push(created);
      } finally {
        controllers.current.delete(key);
        setUploads((u) => {
          const next = { ...u };
          delete next[key];
          return next;
        });
      }
    },
    [recipeId]
  );

  const handleFiles = useCallback(
    async (list: FileList | File[] | null) => {
      if (!list || list.length === 0) return;
      if (storageDown) {
        setError("MinIO đang mất kết nối — tạm thời không upload được. Vui lòng thử lại sau.");
        return;
      }
      setError(null);
      const files = Array.from(list).slice(0, MAX_FILES_PER_DROP);
      for (const f of files) {
        const err = validateClient(f);
        if (err) {
          setError(err);
          continue;
        }
      }
      const valid = files.filter((f) => !validateClient(f));
      if (valid.length === 0) return;
      // Upload song song, mỗi file progress riêng; 1 file lỗi không ảnh hưởng file khác.
      const current = [...images];
      const results = await Promise.allSettled(valid.map((f) => uploadOne(f, current)));
      onChange([...current]);
      const failed = results.filter((r) => r.status === "rejected") as PromiseRejectedResult[];
      if (failed.length > 0) {
        const msg = failed[0].reason instanceof Error ? failed[0].reason.message : "Upload thất bại.";
        setError(
          failed.length === valid.length
            ? msg
            : `${msg} (${valid.length - failed.length}/${valid.length} ảnh đã upload).`
        );
        // Upload lỗi hàng loạt → nghi storage disconnect, check lại ngay.
        if (/MinIO|503|mạng|STORAGE/i.test(msg)) {
          getStorageHealth()
            .then((h) => setStorageDown(h.status !== "healthy"))
            .catch(() => setStorageDown(true));
        }
      }
      if (inputRef.current) inputRef.current.value = "";
    },
    [images, onChange, storageDown, uploadOne]
  );

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

  const uploading = Object.entries(uploads);

  return (
    <div className="flex flex-col gap-3">
      {storageDown && (
        <p className="rounded-xl border border-amber-300 bg-amber-50 px-3 py-2 text-sm text-amber-800">
          MinIO đang mất kết nối — ảnh hiện tại vẫn hiển thị bản cache, upload mới tạm dừng.
        </p>
      )}

      {/* TUẦN 4 — Drag & Drop zone (multi-file, tối đa 5/lần). */}
      <div
        onDragOver={(e) => {
          e.preventDefault();
          setDragOver(true);
        }}
        onDragLeave={() => setDragOver(false)}
        onDrop={(e) => {
          e.preventDefault();
          setDragOver(false);
          handleFiles(e.dataTransfer.files);
        }}
        className={`flex flex-col items-center gap-2 rounded-2xl border-2 border-dashed px-4 py-6 text-center transition-colors ${
          dragOver
            ? "border-zinc-900 bg-zinc-100 dark:border-zinc-100 dark:bg-zinc-800"
            : "border-zinc-300 dark:border-zinc-700"
        }`}
      >
        <p className="text-sm font-medium">Kéo & thả ảnh vào đây (tối đa {MAX_FILES_PER_DROP} ảnh/lần)</p>
        <p className="text-xs text-zinc-500">
          hoặc bấm nút bên dưới. JPEG/PNG/WebP/AVIF, tối đa 5MB/ảnh. Ảnh tự nén trước khi upload.
        </p>
        <div className="flex flex-wrap items-center justify-center gap-2">
          <button
            type="button"
            onClick={() => inputRef.current?.click()}
            disabled={uploading.length > 0 || storageDown}
            title={storageDown ? "Storage đang lỗi, tạm dừng upload" : undefined}
            className="rounded-full bg-zinc-900 px-4 py-2 text-sm text-white disabled:opacity-50 dark:bg-zinc-100 dark:text-zinc-900"
          >
            {uploading.length > 0 ? `Đang upload (${uploading.length})…` : "Chọn ảnh"}
          </button>
          {uploading.length > 0 && (
            <button
              type="button"
              onClick={() => controllers.current.forEach((c) => c.abort())}
              className="rounded-full border px-4 py-2 text-sm"
            >
              Hủy tất cả
            </button>
          )}
        </div>
        <input
          ref={inputRef}
          type="file"
          multiple
          accept="image/jpeg,image/png,image/webp,image/avif"
          className="hidden"
          onChange={(e) => handleFiles(e.target.files)}
        />
      </div>

      {uploading.map(([key, u]) => (
        <div key={key} className="flex flex-col gap-1">
          <div className="h-2 w-full overflow-hidden rounded-full bg-zinc-200 dark:bg-zinc-800">
            <div
              className="h-full rounded-full bg-zinc-900 transition-all dark:bg-zinc-100"
              style={{ width: `${u.progress}%` }}
            />
          </div>
          <span className="text-xs text-zinc-500">
            {u.name} — {u.progress}% — {u.status}
          </span>
        </div>
      ))}

      {error && <p className="text-sm text-red-600">{error}</p>}

      {images.length > 0 && (
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
          {images.map((img) => (
            <div key={img.id} className="relative overflow-hidden rounded-xl border dark:border-zinc-800">
              <button type="button" onClick={() => setLightboxId(img.id)} title="Nhấn để phóng to">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={brokenIds.has(img.id) ? FALLBACK_IMG : thumbSrc(img)}
                  onError={() =>
                    setBrokenIds((s) => {
                      const next = new Set(s);
                      next.add(img.id);
                      return next;
                    })
                  }
                  alt={img.altText ?? "recipe image"}
                  className="aspect-square w-full object-cover"
                  loading="lazy"
                />
              </button>
              {img.isPrimary && (
                <span className="absolute left-2 top-2 rounded-full bg-zinc-900 px-2 py-1 text-[11px] text-white">
                  Ảnh chính
                </span>
              )}
              {img.thumbnailStatus === "pending" && (
                <span className="absolute right-2 top-2 rounded-full bg-black/60 px-2 py-1 text-[11px] text-white">
                  Đang xử lý…
                </span>
              )}
              {brokenIds.has(img.id) && (
                <span className="absolute bottom-14 left-2 rounded-full bg-amber-500 px-2 py-1 text-[11px] text-white">
                  Storage lỗi — hiện ảnh dự phòng
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

      {lightboxId && (
        (() => {
          const img = images.find((i) => i.id === lightboxId);
          if (!img) return null;
          return (
            <div
              className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 p-4"
              onClick={() => setLightboxId(null)}
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={brokenIds.has(img.id) ? FALLBACK_IMG : img.originalUrl}
                onError={(e) => {
                  (e.target as HTMLImageElement).src = FALLBACK_IMG;
                }}
                alt={img.altText ?? "recipe image"}
                className="max-h-full max-w-full rounded-xl object-contain"
              />
            </div>
          );
        })()
      )}
    </div>
  );
}

"use client";

import { useState } from "react";
import type { RecipeImage } from "@/lib/api";

/** Fallback khi MinIO lỗi / URL gãy (graceful fallback TUẦN 4). */
export const FALLBACK_IMG =
  "data:image/svg+xml;charset=utf-8," +
  encodeURIComponent(
    `<svg xmlns="http://www.w3.org/2000/svg" width="600" height="600"><rect width="100%" height="100%" fill="#e4e4e7"/><text x="50%" y="50%" dominant-baseline="middle" text-anchor="middle" font-size="28" fill="#71717a">Ảnh tạm lỗi</text></svg>`
  );

interface Props {
  images: RecipeImage[];
}

/** Prefer biến thể đã xử lý: thumb cho strip, medium cho preview, gốc cho lightbox. */
export function thumbSrc(img: RecipeImage): string {
  return img.thumbnailUrl ?? img.mediumUrl ?? img.originalUrl;
}

export function previewSrc(img: RecipeImage): string {
  return img.mediumUrl ?? img.originalUrl;
}

export default function RecipeGallery({ images }: Props) {
  const sorted = [...images].sort((a, b) => a.orderIndex - b.orderIndex);
  const [activeId, setActiveId] = useState<string | null>(
    sorted.find((i) => i.isPrimary)?.id ?? sorted[0]?.id ?? null
  );
  const [lightbox, setLightbox] = useState(false);

  const active = sorted.find((i) => i.id === activeId) ?? sorted[0] ?? null;
  if (!active) return null;

  return (
    <div className="flex flex-col gap-2">
      <button
        type="button"
        onClick={() => setLightbox(true)}
        className="relative block w-full overflow-hidden rounded-xl"
        title="Nhấn để phóng to"
      >
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={previewSrc(active)}
          onError={(e) => {
            if ((e.target as HTMLImageElement).src !== FALLBACK_IMG)
              (e.target as HTMLImageElement).src = FALLBACK_IMG;
          }}
          alt={active.altText ?? "recipe image"}
          className="aspect-[16/9] w-full object-cover"
        />
        {active.thumbnailStatus === "pending" && (
          <span className="absolute right-2 top-2 rounded-full bg-black/60 px-2 py-1 text-[11px] text-white">
            Đang xử lý ảnh…
          </span>
        )}
      </button>

      {sorted.length > 1 && (
        <div className="grid grid-cols-5 gap-2">
          {sorted.map((img) => (
            <button
              key={img.id}
              type="button"
              onClick={() => setActiveId(img.id)}
              className={`overflow-hidden rounded-lg border-2 ${
                img.id === active.id
                  ? "border-zinc-900 dark:border-zinc-100"
                  : "border-transparent opacity-70 hover:opacity-100"
              }`}
            >
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={thumbSrc(img)}
                onError={(e) => {
                  if ((e.target as HTMLImageElement).src !== FALLBACK_IMG)
                    (e.target as HTMLImageElement).src = FALLBACK_IMG;
                }}
                alt={img.altText ?? "thumbnail"}
                loading="lazy"
                className="aspect-square w-full object-cover"
              />
            </button>
          ))}
        </div>
      )}

      {lightbox && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 p-4"
          onClick={() => setLightbox(false)}
        >
          {/* eslint-disable-next-line @next/next/no-img-element */}
          <img
            src={active.originalUrl}
            onError={(e) => {
              if ((e.target as HTMLImageElement).src !== FALLBACK_IMG)
                (e.target as HTMLImageElement).src = FALLBACK_IMG;
            }}
            alt={active.altText ?? "recipe image"}
            className="max-h-full max-w-full rounded-xl object-contain"
          />
        </div>
      )}
    </div>
  );
}

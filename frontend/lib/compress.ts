/**
 * TUẦN 4 (Trần Gia Huy) — Upload Optimization: client-side image compression.
 * Không thêm dependency: dùng canvas + createImageBitmap.
 * Ảnh > 1920px resize về 1920, encode JPEG q=0.82 (trừ PNG nhỏ giữ nguyên
 * để không vỡ transparency). Trả về File mới sẵn sàng presign.
 */

export interface CompressResult {
  file: File;
  originalSize: number;
  compressedSize: number;
  wasCompressed: boolean;
}

const MAX_DIMENSION = 1920;
const JPEG_QUALITY = 0.82;
const SKIP_BELOW = 300 * 1024; // file < 300KB: bỏ qua nén

export async function compressImage(file: File): Promise<CompressResult> {
  const originalSize = file.size;
  const noCompress: CompressResult = {
    file,
    originalSize,
    compressedSize: originalSize,
    wasCompressed: false,
  };

  // AVIF: canvas nhiều trình duyệt không encode được → giữ nguyên cho BE xử lý.
  if (file.type === "image/avif") return noCompress;
  if (originalSize < SKIP_BELOW) return noCompress;
  if (typeof window === "undefined" || typeof document === "undefined")
    return noCompress;

  try {
    const bitmap = await createImageBitmap(file);
    const { width, height } = bitmap;
    const scale = Math.min(1, MAX_DIMENSION / Math.max(width, height));
    const targetW = Math.max(1, Math.round(width * scale));
    const targetH = Math.max(1, Math.round(height * scale));

    // Không cần resize lẫn encode khi ảnh đã nhỏ và đúng chiều.
    if (scale === 1 && file.type === "image/jpeg") {
      bitmap.close();
      return noCompress;
    }

    const canvas = document.createElement("canvas");
    canvas.width = targetW;
    canvas.height = targetH;
    const ctx = canvas.getContext("2d");
    if (!ctx) {
      bitmap.close();
      return noCompress;
    }
    ctx.drawImage(bitmap, 0, 0, targetW, targetH);
    bitmap.close();

    // PNG có transparency: giữ PNG nếu canvas có alpha; ngược lại chuyển JPEG cho nhẹ.
    const outType = file.type === "image/png" ? "image/png" : "image/jpeg";
    const blob: Blob | null = await new Promise((resolve) =>
      canvas.toBlob(resolve, outType, JPEG_QUALITY)
    );
    if (!blob || blob.size >= originalSize) return noCompress;

    const ext = outType === "image/png" ? "png" : "jpg";
    const base = file.name.replace(/\.[^.]+$/, "") || "image";
    const compressed = new File([blob], `${base}.compressed.${ext}`, {
      type: blob.type || outType,
      lastModified: Date.now(),
    });
    return {
      file: compressed,
      originalSize,
      compressedSize: compressed.size,
      wasCompressed: true,
    };
  } catch {
    return noCompress;
  }
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(2)} MB`;
}

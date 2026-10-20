import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

/** Ghép className có điều kiện và xử lý xung đột utility của Tailwind. */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

const DIFFICULTY_LABELS: Record<string, string> = {
  Easy: "Dễ",
  Medium: "Trung bình",
  Hard: "Khó",
  Expert: "Chuyên gia",
  "1": "Dễ",
  "2": "Trung bình",
  "3": "Khó",
  "4": "Chuyên gia",
};

export function difficultyLabel(value: string | number | null | undefined) {
  if (value == null) return "—";
  return DIFFICULTY_LABELS[String(value)] ?? String(value);
}

const STATUS_LABELS: Record<string, string> = {
  Draft: "Nháp",
  Published: "Đã xuất bản",
  Archived: "Lưu trữ",
  draft: "Nháp",
  published: "Đã xuất bản",
  archived: "Lưu trữ",
};

export function statusLabel(value: string | null | undefined) {
  if (!value) return "—";
  return STATUS_LABELS[value] ?? value;
}

export function formatDate(value: string | null | undefined) {
  if (!value) return "—";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";
  return new Intl.DateTimeFormat("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(date);
}

export function formatMinutes(minutes: number | null | undefined) {
  if (minutes == null || minutes < 0) return "—";
  if (minutes < 60) return `${minutes} phút`;
  const hours = Math.floor(minutes / 60);
  const rest = minutes % 60;
  return rest === 0 ? `${hours} giờ` : `${hours} giờ ${rest} phút`;
}

export function totalTime(prep: number, cook: number) {
  return formatMinutes(prep + cook);
}

/** Tạo query string, bỏ qua giá trị rỗng/undefined. */
export function buildQuery(
  params: Record<string, string | number | boolean | null | undefined>
) {
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === "") continue;
    search.set(key, String(value));
  }
  const query = search.toString();
  return query ? `?${query}` : "";
}

export function absoluteUrl(path = "/") {
  const base =
    process.env.NEXT_PUBLIC_SITE_URL?.replace(/\/$/, "") ??
    "http://localhost:3000";
  return `${base}${path.startsWith("/") ? path : `/${path}`}`;
}

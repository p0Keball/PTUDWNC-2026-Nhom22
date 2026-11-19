import type { ProblemDetails } from "@/types/problem";

/**
 * Lấy Base URL của Backend API:
 * - Khi chạy SSR trên Node.js (trong Docker container): ưu tiên INTERNAL_API_URL (ví dụ http://api:8080).
 * - Khi chạy ở Client-side (trình duyệt): dùng NEXT_PUBLIC_API_URL (ví dụ http://localhost:8080).
 */
export function getApiBaseUrl(): string {
  if (typeof window === "undefined") {
    if (process.env.INTERNAL_API_URL) {
      return process.env.INTERNAL_API_URL.replace(/\/$/, "");
    }
    const publicUrl = process.env.NEXT_PUBLIC_API_URL ?? "";
    if (publicUrl.includes("localhost:8080") && process.env.HOSTNAME === "0.0.0.0") {
      return "http://api:8080";
    }
    return (publicUrl || "http://localhost:8080").replace(/\/$/, "");
  }
  return (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080").replace(/\/$/, "");
}

export const API_BASE_URL = (
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080"
).replace(/\/$/, "");

export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;
  readonly detail?: string | null;
  readonly errors?: Record<string, string[]>;
  readonly traceId?: string;
  readonly correlationId?: string | null;

  constructor(init: {
    status: number;
    message: string;
    code?: string;
    detail?: string | null;
    errors?: Record<string, string[]>;
    traceId?: string;
    correlationId?: string | null;
  }) {
    super(init.message);
    this.name = "ApiError";
    this.status = init.status;
    this.code = init.code;
    this.detail = init.detail;
    this.errors = init.errors;
    this.traceId = init.traceId;
    this.correlationId = init.correlationId;
  }
}

function isJsonResponse(response: Response) {
  return response.headers
    .get("content-type")
    ?.toLowerCase()
    .includes("application/json");
}

async function readProblem(response: Response): Promise<ProblemDetails | null> {
  if (!isJsonResponse(response)) return null;
  try {
    const data = (await response.json()) as ProblemDetails;
    return data && typeof data === "object" ? data : null;
  } catch {
    return null;
  }
}

/** Chuyển response lỗi RFC 7807 thành ApiError thống nhất toàn app. */
export async function toApiError(
  response: Response,
  fallbackMessage = `Yêu cầu thất bại (${response.status}).`
): Promise<ApiError> {
  const problem = await readProblem(response);
  return new ApiError({
    status: response.status,
    message: problem?.title || problem?.detail || fallbackMessage,
    code: problem?.code ?? undefined,
    detail: problem?.detail ?? null,
    errors: problem?.errors,
    traceId: problem?.traceId,
    correlationId: problem?.correlationId,
  });
}

export interface RequestOptions extends Omit<RequestInit, "body" | "headers"> {
  body?: unknown;
  token?: string | null;
  headers?: HeadersInit;
  fallbackMessage?: string;
  /** Next.js fetch cache options. */
  next?: { revalidate?: number | false; tags?: string[] };
}

/**
 * Wrapper fetch dùng chung:
 * - Tự gắn Content-Type JSON (trừ FormData).
 * - Gắn Bearer token khi có.
 * - Gửi cookie (credentials: include) để tương thích cookie auth hiện tại.
 * - Parse RFC 7807 problem+json thành ApiError.
 * - Hỗ trợ 204 No Content.
 */
export async function request<T>(
  path: string,
  options: RequestOptions = {}
): Promise<T> {
  const {
    body,
    token,
    headers,
    fallbackMessage,
    credentials,
    ...init
  } = options;

  const finalHeaders = new Headers(headers);
  const isFormData = body instanceof FormData;

  if (body !== undefined && !isFormData && !finalHeaders.has("Content-Type")) {
    finalHeaders.set("Content-Type", "application/json");
  }
  if (token) finalHeaders.set("Authorization", `Bearer ${token}`);

  const baseUrl = getApiBaseUrl();
  const url = /^https?:\/\//i.test(path) ? path : `${baseUrl}${path}`;

  const response = await fetch(url, {
    ...init,
    headers: finalHeaders,
    credentials: credentials ?? "include",
    body:
      body === undefined
        ? undefined
        : isFormData
          ? (body as FormData)
          : JSON.stringify(body),
  });

  if (!response.ok) {
    throw await toApiError(response, fallbackMessage);
  }

  if (response.status === 204) return undefined as T;

  const text = await response.text();
  if (!text) return undefined as T;

  if (isJsonResponse(response)) {
    return JSON.parse(text) as T;
  }
  return text as T;
}

export const http = {
  get: <T>(path: string, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "GET" }),
  post: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "POST", body }),
  put: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "PUT", body }),
  patch: <T>(path: string, body?: unknown, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "PATCH", body }),
  delete: <T>(path: string, options?: RequestOptions) =>
    request<T>(path, { ...options, method: "DELETE" }),
};

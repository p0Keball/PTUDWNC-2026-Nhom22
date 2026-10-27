import type { ProblemDetails } from "@/types/problem";

export const API_BASE_URL = (
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000"
).replace(/\/$/, "");

export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;
  readonly detail?: string | null;
  readonly errors?: Record<string, string[]>;
  readonly traceId?: string;

  constructor(init: {
    status: number;
    message: string;
    code?: string;
    detail?: string | null;
    errors?: Record<string, string[]>;
    traceId?: string;
  }) {
    super(init.message);
    this.name = "ApiError";
    this.status = init.status;
    this.code = init.code;
    this.detail = init.detail;
    this.errors = init.errors;
    this.traceId = init.traceId;
  }
}

function isJsonResponse(response: Response) {
  return response.headers.get("content-type")?.toLowerCase().includes("json") ?? false;
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

export async function toApiError(
  response: Response,
  fallbackMessage = `Yêu cầu thất bại (${response.status}).`
): Promise<ApiError> {
  const problem = await readProblem(response);
  return new ApiError({
    status: response.status,
    message: problem?.title ?? problem?.detail ?? fallbackMessage,
    code: problem?.code ?? undefined,
    detail: problem?.detail,
    errors: problem?.errors,
    traceId: problem?.traceId ?? undefined,
  });
}

export interface RequestOptions extends Omit<RequestInit, "body" | "headers"> {
  body?: unknown;
  token?: string | null;
  headers?: HeadersInit;
  fallbackMessage?: string;
  next?: { revalidate?: number | false; tags?: string[] };
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { body, token, headers, fallbackMessage, credentials, ...init } = options;
  const finalHeaders = new Headers(headers);
  const isFormData = body instanceof FormData;

  if (body !== undefined && !isFormData && !finalHeaders.has("Content-Type"))
    finalHeaders.set("Content-Type", "application/json");
  if (token) finalHeaders.set("Authorization", "Bearer " + token);

  const response = await fetch(
    /^https?:\/\//i.test(path) ? path : `${API_BASE_URL}${path}`,
    {
      ...init,
      headers: finalHeaders,
      credentials: credentials ?? "include",
      body:
        body === undefined
          ? undefined
          : isFormData
            ? body
            : JSON.stringify(body),
    }
  );

  if (!response.ok) throw await toApiError(response, fallbackMessage);
  if (response.status === 204) return undefined as T;
  const text = await response.text();
  return text && isJsonResponse(response) ? (JSON.parse(text) as T) : (text as T);
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

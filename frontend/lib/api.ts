import { appConfig } from "@/lib/config";

export interface RecipeSummary {
  id: string;
  title: string;
  slug: string;
  description: string;
  primaryImageUrl: string | null;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: string;
  status: string;
  publishedAt: string | null;
}

export interface RecipeListResponse {
  items: RecipeSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface RecipeDetail {
  id: string;
  title: string;
  slug: string;
  description: string;
  instructions: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: string;
  status: string;
  categoryId: string;
  authorId: string;
  publishedAt: string | null;
  rowVersion: string;
  nutrition: {
    calories: number | null;
    protein: number | null;
    carbohydrates: number | null;
    fat: number | null;
    fiber: number | null;
    sodium: number | null;
  } | null;
  steps: {
    id: string;
    stepNumber: number;
    title: string;
    description: string;
    timerMinutes: number | null;
    imageUrl: string | null;
  }[];
  ingredients: {
    id: string;
    name: string;
    quantity: number | null;
    unit: string | null;
    notes: string | null;
    orderIndex: number;
  }[];
  images: {
    id: string;
    originalUrl: string;
    mediumUrl: string | null;
    thumbnailUrl: string | null;
    altText: string | null;
    isPrimary: boolean;
    orderIndex: number;
    thumbnailStatus: string;
  }[];
}

export interface RecipeNutritionInput {
  calories?: number | null;
  protein?: number | null;
  carbohydrates?: number | null;
  fat?: number | null;
  fiber?: number | null;
  sodium?: number | null;
}

export interface CreateRecipeInput {
  title: string;
  description: string;
  instructions: string;
  categoryId: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  servings: number;
  difficulty: number;
}

export interface UpdateRecipeInput extends CreateRecipeInput {
  rowVersion: string;
  nutrition?: RecipeNutritionInput | null;
}

export class ApiError extends Error {
  status: number;
  code?: string;
  errors?: Record<string, string[]>;

  constructor(status: number, message: string, code?: string, errors?: Record<string, string[]>) {
    super(message);
    this.status = status;
    this.code = code;
    this.errors = errors;
  }
}

export interface AuthUser {
  id: string;
  email: string;
  displayName: string;
  role: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: AuthUser;
}

export interface UserProfile extends AuthUser {
  avatarUrl: string | null;
  bio: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface UpdateProfileInput {
  displayName?: string;
  avatarUrl?: string;
  bio?: string;
}

export interface RecipeListParams {
  categoryId?: string;
  difficulty?: number;
  maxCookTime?: number;
  sort?: string;
}

export async function changePassword(
  currentPassword: string,
  newPassword: string,
  token: string
): Promise<void> {
  const res = await fetch(`${API_BASE}/api/v1/auth/change-password`, {
    method: "POST",
    headers: authHeaders(token),
    body: JSON.stringify({ currentPassword, newPassword }),
    credentials: "include",
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Đổi mật khẩu thất bại.");
}

export async function forgotPassword(email: string): Promise<void> {
  const res = await fetch(`${API_BASE}/api/v1/auth/forgot-password`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify({ email }),
    credentials: "include",
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Không thể gửi yêu cầu đặt lại mật khẩu.");
}

export async function resetPassword(
  email: string,
  token: string,
  newPassword: string
): Promise<void> {
  const res = await fetch(`${API_BASE}/api/v1/auth/reset-password`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify({ email, token, newPassword }),
    credentials: "include",
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Đặt lại mật khẩu thất bại.");
}

function authHeaders(token?: string | null): Record<string, string> {
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = "Bearer " + token;
  return headers;
}

async function throwApiError(res: Response, fallback: string): Promise<never> {
  let code: string | undefined;
  let errors: Record<string, string[]> | undefined;
  let title = fallback;
  try {
    const data = await res.json();
    if (typeof data?.title === "string") title = data.title;
    if (typeof data?.code === "string") code = data.code;
    if (data?.errors && typeof data.errors === "object") errors = data.errors;
  } catch {
    // giu fallback khi body khong phai JSON
  }
  throw new ApiError(res.status, title, code, errors);
}

const API_BASE = appConfig.apiUrl;

export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  orderIndex: number;
  recipeCount: number;
}

export async function getCategories(): Promise<Category[]> {
  const res = await fetch(`${API_BASE}/api/v1/categories`, {
    next: { revalidate: 3600 },
  });
  if (!res.ok) throw new Error(`Failed to fetch categories: ${res.status}`);
  return res.json();
}

export async function login(email: string, password: string): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE}/api/v1/auth/login`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify({ email, password }),
    credentials: "include",
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "ÄÄƒng nháº­p tháº¥t báº¡i.");
  return res.json();
}

export async function loginWithGoogle(
  code: string,
  codeVerifier: string,
  redirectUri: string
): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE}/api/v1/auth/google`, {
    method: "POST",
    headers: authHeaders(),
    body: JSON.stringify({ code, codeVerifier, redirectUri }),
    credentials: "include",
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "ÄÄƒng nháº­p Google tháº¥t báº¡i.");
  return res.json();
}

export async function getMyProfile(token: string): Promise<UserProfile> {
  const res = await fetch(`${API_BASE}/api/v1/auth/me`, {
    headers: authHeaders(token),
    credentials: "include",
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "KhÃ´ng táº£i Ä‘Æ°á»£c há»“ sÆ¡.");
  return res.json();
}

export async function updateMyProfile(
  input: UpdateProfileInput,
  token: string
): Promise<UserProfile> {
  const res = await fetch(`${API_BASE}/api/v1/auth/me`, {
    method: "PATCH",
    headers: authHeaders(token),
    body: JSON.stringify(input),
    credentials: "include",
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Cáº­p nháº­t há»“ sÆ¡ tháº¥t báº¡i.");
  return res.json();
}

export async function getRecipes(
  page = 1,
  pageSize = 12,
  filters: RecipeListParams = {},
  options: { noStore?: boolean } = {}
): Promise<RecipeListResponse> {
  const query = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });
  if (filters.categoryId) query.set("categoryId", filters.categoryId);
  if (filters.difficulty) query.set("difficulty", String(filters.difficulty));
  if (filters.maxCookTime) query.set("maxCookTime", String(filters.maxCookTime));
  if (filters.sort) query.set("sort", filters.sort);
  const res = await fetch(
    `${API_BASE}/api/v1/recipes?${query}`,
    options.noStore ? { cache: "no-store" } : { next: { revalidate: 60 } }
  );
  if (!res.ok) throw new Error(`Failed to fetch recipes: ${res.status}`);
  return res.json();
}

export async function getRecipeBySlug(slug: string): Promise<RecipeDetail | null> {
  const res = await fetch(
    `${API_BASE}/api/v1/recipes/${encodeURIComponent(slug)}`,
    { next: { revalidate: 60 } }
  );
  if (res.status === 404) return null;
  if (!res.ok) throw new Error(`Failed to fetch recipe: ${res.status}`);
  return res.json();
}

export async function createRecipe(
  input: CreateRecipeInput,
  token?: string | null
): Promise<RecipeDetail> {
  const res = await fetch(`${API_BASE}/api/v1/recipes`, {
    method: "POST",
    headers: authHeaders(token),
    body: JSON.stringify(input),
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Táº¡o cÃ´ng thá»©c tháº¥t báº¡i.");
  return res.json();
}

export async function updateRecipe(
  id: string,
  input: UpdateRecipeInput,
  token?: string | null
): Promise<RecipeDetail> {
  const res = await fetch(`${API_BASE}/api/v1/recipes/${encodeURIComponent(id)}`, {
    method: "PUT",
    headers: authHeaders(token),
    body: JSON.stringify(input),
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Cáº­p nháº­t cÃ´ng thá»©c tháº¥t báº¡i.");
  return res.json();
}


export type RecipeStep = RecipeDetail["steps"][number];
export type RecipeIngredient = RecipeDetail["ingredients"][number];
export type RecipeImage = RecipeDetail["images"][number];

async function handleChildResponse(res: Response, action: string) {
  if (res.status === 204) return null;
  const data = await res.json().catch(() => null);
  if (!res.ok) {
    const detail =
      data?.title ??
      (data?.errors ? JSON.stringify(data.errors) : `Request failed: ${res.status}`);
    throw new Error(`${action} tháº¥t báº¡i: ${detail}`);
  }
  return data;
}

export const stepApi = {
  create(recipeId: string, body: { title: string; description: string; timerMinutes?: number | null; imageUrl?: string | null }): Promise<RecipeStep> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/steps`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => handleChildResponse(r, "ThÃªm bÆ°á»›c"));
  },
  update(recipeId: string, stepId: string, body: { title: string; description: string; timerMinutes?: number | null; imageUrl?: string | null }): Promise<RecipeStep> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/steps/${stepId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => handleChildResponse(r, "Cáº­p nháº­t bÆ°á»›c"));
  },
  remove(recipeId: string, stepId: string): Promise<void> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/steps/${stepId}`, {
      method: "DELETE",
    }).then((r) => handleChildResponse(r, "XÃ³a bÆ°á»›c").then(() => undefined));
  },
  reorder(recipeId: string, items: { id: string; order: number }[]): Promise<RecipeStep[]> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/steps/reorder`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(items),
    }).then((r) => handleChildResponse(r, "Sáº¯p xáº¿p bÆ°á»›c"));
  },
};

export const ingredientApi = {
  create(recipeId: string, body: { name: string; quantity?: number | null; unit?: string | null; notes?: string | null }): Promise<RecipeIngredient> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/ingredients`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => handleChildResponse(r, "ThÃªm nguyÃªn liá»‡u"));
  },
  update(recipeId: string, ingId: string, body: { name: string; quantity?: number | null; unit?: string | null; notes?: string | null }): Promise<RecipeIngredient> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/ingredients/${ingId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => handleChildResponse(r, "Cáº­p nháº­t nguyÃªn liá»‡u"));
  },
  remove(recipeId: string, ingId: string): Promise<void> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/ingredients/${ingId}`, {
      method: "DELETE",
    }).then((r) => handleChildResponse(r, "XÃ³a nguyÃªn liá»‡u").then(() => undefined));
  },
  reorder(recipeId: string, items: { id: string; order: number }[]): Promise<RecipeIngredient[]> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/ingredients/reorder`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(items),
    }).then((r) => handleChildResponse(r, "Sáº¯p xáº¿p nguyÃªn liá»‡u"));
  },
};

export interface PresignResult {
  uploadUrl: string;
  objectKey: string;
  publicUrl: string;
  expiresAt: string;
}

export const imageApi = {
  async presign(recipeId: string, file: File): Promise<PresignResult> {
    const res = await fetch(`${API_BASE}/api/v1/recipes/${recipeId}/images/presign`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ fileName: file.name, contentType: file.type, fileSize: file.size }),
    });
    return handleChildResponse(res, "Xin presigned URL");
  },
  async confirm(recipeId: string, objectKey: string, altText?: string | null): Promise<RecipeImage> {
    const res = await fetch(`${API_BASE}/api/v1/recipes/${recipeId}/images/confirm`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ objectKey, altText: altText ?? null }),
    });
    return handleChildResponse(res, "XÃ¡c nháº­n áº£nh");
  },
  async setPrimary(recipeId: string, imgId: string): Promise<RecipeImage> {
    const res = await fetch(`${API_BASE}/api/v1/recipes/${recipeId}/images/${imgId}/primary`, {
      method: "PATCH",
    });
    return handleChildResponse(res, "Äáº·t áº£nh chÃ­nh");
  },
  async remove(recipeId: string, imgId: string): Promise<void> {
    const res = await fetch(`${API_BASE}/api/v1/recipes/${recipeId}/images/${imgId}`, {
      method: "DELETE",
    });
    await handleChildResponse(res, "XÃ³a áº£nh");
  },
};

/**
 * Upload trá»±c tiáº¿p lÃªn MinIO qua presigned PUT URL báº±ng XHR Ä‘á»ƒ cÃ³ progress %.
 * TrÃ¬nh duyá»‡t fetch chÆ°a há»— trá»£ upload progress nÃªn dÃ¹ng XHR á»Ÿ Ä‘Ã¢y.
 */
export function putFileToPresignedUrl(
  uploadUrl: string,
  file: File,
  onProgress: (percent: number) => void,
  signal?: AbortSignal
): Promise<void> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open("PUT", uploadUrl, true);
    xhr.setRequestHeader("Content-Type", file.type);

    if (signal) {
      signal.addEventListener("abort", () => xhr.abort(), { once: true });
    }
    xhr.upload.onprogress = (e) => {
      if (e.lengthComputable) onProgress(Math.round((e.loaded / e.total) * 100));
    };
    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) resolve();
      else reject(new Error(`Upload lÃªn MinIO tháº¥t báº¡i: ${xhr.status}`));
    };
    xhr.onerror = () => reject(new Error("Lá»—i máº¡ng khi upload áº£nh."));
    xhr.onabort = () => reject(new Error("ÄÃ£ há»§y upload."));
    xhr.send(file);
  });
}

export async function uploadRecipeImage(
  recipeId: string,
  file: File,
  onProgress: (percent: number) => void,
  altText?: string,
  signal?: AbortSignal
): Promise<RecipeImage> {
  const presigned = await imageApi.presign(recipeId, file);
  await putFileToPresignedUrl(presigned.uploadUrl, file, onProgress, signal);
  return imageApi.confirm(recipeId, presigned.objectKey, altText);
}

export interface CategoryDetail {
  category: Category;
  recipes: {
    items: RecipeSummary[];
    totalCount: number;
    page: number;
    pageSize: number;
  };
}

export async function getCategoryBySlug(
  slug: string,
  page = 1,
  pageSize = 12
): Promise<CategoryDetail | null> {
  const res = await fetch(
    `${API_BASE}/api/v1/categories/${encodeURIComponent(slug)}?page=${page}&pageSize=${pageSize}`,
    { next: { revalidate: 600 } }
  );
  if (res.status === 404) return null;
  if (!res.ok) throw new Error(`Failed to fetch category: ${res.status}`);
  return res.json();
}

export interface RecipeValidation {
  recipeId: string;
  hasSteps: boolean;
  hasIngredients: boolean;
  hasCoverImage: boolean;
  stepsCount: number;
  ingredientsCount: number;
  imagesCount: number;
  canPublish: boolean;
  issues: string[];
}

export async function getRecipeValidation(
  recipeId: string,
  token?: string | null
): Promise<RecipeValidation> {
  const res = await fetch(`${API_BASE}/api/v1/recipes/${encodeURIComponent(recipeId)}/validation`, {
    headers: authHeaders(token ?? undefined),
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Không kiểm tra được công thức.");
  return res.json();
}

export async function getMyRecipeById(
  recipeId: string,
  token?: string | null
): Promise<RecipeDetail> {
  const res = await fetch(`${API_BASE}/api/v1/recipes/mine/${encodeURIComponent(recipeId)}`, {
    headers: authHeaders(token ?? undefined),
    credentials: "include",
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Không tải được công thức.");
  return res.json();
}

export const findMyRecipeById = getMyRecipeById;

export async function getStorageHealth(): Promise<{ status: string; checkedAt: string }> {
  const res = await fetch(`${API_BASE}/api/v1/storage/health`, { cache: "no-store" });
  if (!res.ok) throw new Error("Storage unavailable.");
  return res.json();
}

export interface SearchParams {
  q: string;
  page?: number;
  pageSize?: number;
  categoryId?: string;
  difficulty?: string;
  maxCookTime?: number;
  sort?: string;
}

export interface SearchResponse {
  items: RecipeSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export async function searchRecipes(
  params: SearchParams
): Promise<SearchResponse> {
  const query = new URLSearchParams();
  query.set("q", params.q);
  query.set("page", String(params.page ?? 1));
  query.set("pageSize", String(params.pageSize ?? 12));
  if (params.categoryId) query.set("categoryId", params.categoryId);
  if (params.difficulty) query.set("difficulty", params.difficulty);
  if (params.maxCookTime !== undefined)
    query.set("maxCookTime", String(params.maxCookTime));
  if (params.sort) query.set("sort", params.sort);
  const res = await fetch(`${API_BASE}/api/v1/recipes/search?${query}`, {
    cache: "no-store",
  });
  if (!res.ok) throw new Error(`Failed to search recipes: ${res.status}`);
  return res.json();
}

export async function getMyRecipes(
  page = 1,
  pageSize = 12,
  status?: string,
  token?: string | null
): Promise<RecipeListResponse> {
  const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (status && status !== "all") params.set("status", status);
  const res = await fetch(`${API_BASE}/api/v1/recipes/mine?${params}`, {
    headers: authHeaders(token),
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Táº£i danh sÃ¡ch cÃ´ng thá»©c tháº¥t báº¡i.");
  return res.json();
}

export interface RecipeStatusResult {
  id: string;
  status: string;
}

async function changeRecipeStatus(
  id: string,
  action: "publish" | "unpublish" | "archive",
  token?: string | null
): Promise<RecipeStatusResult> {
  const res = await fetch(
    `${API_BASE}/api/v1/recipes/${encodeURIComponent(id)}/${action}`,
    { method: "PATCH", headers: authHeaders(token), cache: "no-store" }
  );
  if (!res.ok) await throwApiError(res, "Äá»•i tráº¡ng thÃ¡i cÃ´ng thá»©c tháº¥t báº¡i.");
  return res.json();
}

export const publishRecipe = (id: string, token?: string | null) =>
  changeRecipeStatus(id, "publish", token);
export const unpublishRecipe = (id: string, token?: string | null) =>
  changeRecipeStatus(id, "unpublish", token);
export const archiveRecipe = (id: string, token?: string | null) =>
  changeRecipeStatus(id, "archive", token);

export async function deleteRecipe(id: string, token?: string | null): Promise<void> {
  const res = await fetch(`${API_BASE}/api/v1/recipes/${encodeURIComponent(id)}`, {
    method: "DELETE",
    headers: authHeaders(token),
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "XÃ³a cÃ´ng thá»©c tháº¥t báº¡i.");
}

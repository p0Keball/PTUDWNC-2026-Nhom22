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

const API_BASE =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: string;
    email: string;
    fullName: string;
    role: string;
  };
}

export async function loginWithGoogleCode(
  code: string,
  codeVerifier: string
): Promise<AuthResponse> {
  const response = await fetch(`${API_BASE}/api/v1/auth/google`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ code, codeVerifier }),
  });
  const data = await response.json().catch(() => null);
  if (!response.ok) {
    throw new Error(data?.detail ?? data?.title ?? "Đăng nhập Google thất bại.");
  }
  return data;
}

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  bio: string | null;
  role: string;
  isActive: boolean;
  createdAt: string;
}

async function profileRequest<T>(input: RequestInfo, init?: RequestInit): Promise<T> {
  const response = await fetch(input, {
    ...init,
    credentials: "include",
  });
  const data = await response.json().catch(() => null);
  if (!response.ok) {
    throw new Error(data?.detail ?? data?.title ?? "Không thể thực hiện yêu cầu hồ sơ.");
  }
  return data;
}

export const profileApi = {
  get: () => profileRequest<UserProfile>(`${API_BASE}/api/v1/auth/me`),
  update: (payload: {
    displayName?: string | null;
    avatarUrl?: string | null;
    bio?: string | null;
  }) => profileRequest<UserProfile>(`${API_BASE}/api/v1/auth/me`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  }),
};

export async function getRecipes(
  page = 1,
  pageSize = 12
): Promise<RecipeListResponse> {
  const res = await fetch(
    `${API_BASE}/api/v1/recipes?page=${page}&pageSize=${pageSize}`,
    { next: { revalidate: 60 } }
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
    throw new Error(`${action} thất bại: ${detail}`);
  }
  return data;
}

export const stepApi = {
  create(recipeId: string, body: { title: string; description: string; timerMinutes?: number | null; imageUrl?: string | null }): Promise<RecipeStep> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/steps`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => handleChildResponse(r, "Thêm bước"));
  },
  update(recipeId: string, stepId: string, body: { title: string; description: string; timerMinutes?: number | null; imageUrl?: string | null }): Promise<RecipeStep> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/steps/${stepId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => handleChildResponse(r, "Cập nhật bước"));
  },
  remove(recipeId: string, stepId: string): Promise<void> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/steps/${stepId}`, {
      method: "DELETE",
    }).then((r) => handleChildResponse(r, "Xóa bước").then(() => undefined));
  },
  reorder(recipeId: string, items: { id: string; order: number }[]): Promise<RecipeStep[]> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/steps/reorder`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(items),
    }).then((r) => handleChildResponse(r, "Sắp xếp bước"));
  },
};

export const ingredientApi = {
  create(recipeId: string, body: { name: string; quantity?: number | null; unit?: string | null; notes?: string | null }): Promise<RecipeIngredient> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/ingredients`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => handleChildResponse(r, "Thêm nguyên liệu"));
  },
  update(recipeId: string, ingId: string, body: { name: string; quantity?: number | null; unit?: string | null; notes?: string | null }): Promise<RecipeIngredient> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/ingredients/${ingId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }).then((r) => handleChildResponse(r, "Cập nhật nguyên liệu"));
  },
  remove(recipeId: string, ingId: string): Promise<void> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/ingredients/${ingId}`, {
      method: "DELETE",
    }).then((r) => handleChildResponse(r, "Xóa nguyên liệu").then(() => undefined));
  },
  reorder(recipeId: string, items: { id: string; order: number }[]): Promise<RecipeIngredient[]> {
    return fetch(`${API_BASE}/api/v1/recipes/${recipeId}/ingredients/reorder`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(items),
    }).then((r) => handleChildResponse(r, "Sắp xếp nguyên liệu"));
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
    return handleChildResponse(res, "Xác nhận ảnh");
  },
  async setPrimary(recipeId: string, imgId: string): Promise<RecipeImage> {
    const res = await fetch(`${API_BASE}/api/v1/recipes/${recipeId}/images/${imgId}/primary`, {
      method: "PATCH",
    });
    return handleChildResponse(res, "Đặt ảnh chính");
  },
  async remove(recipeId: string, imgId: string): Promise<void> {
    const res = await fetch(`${API_BASE}/api/v1/recipes/${recipeId}/images/${imgId}`, {
      method: "DELETE",
    });
    await handleChildResponse(res, "Xóa ảnh");
  },
};

/**
 * Upload trực tiếp lên MinIO qua presigned PUT URL bằng XHR để có progress %.
 * Trình duyệt fetch chưa hỗ trợ upload progress nên dùng XHR ở đây.
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
      else reject(new Error(`Upload lên MinIO thất bại: ${xhr.status}`));
    };
    xhr.onerror = () => reject(new Error("Lỗi mạng khi upload ảnh."));
    xhr.onabort = () => reject(new Error("Đã hủy upload."));
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

import { ApiError, http } from "@/lib/http";
import { buildQuery } from "@/lib/utils";

export { ApiError };

/* -------------------------------------------------------------------------- */
/* Types                                                                      */
/* -------------------------------------------------------------------------- */

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

export interface RecipeListParams {
  categoryId?: string;
  /** 1 = Dễ, 2 = Trung bình, 3 = Khó, 4 = Chuyên gia */
  difficulty?: number;
  maxCookTime?: number;
  /** createdAt | -createdAt | title | -title */
  sort?: string;
}

export interface RecipeListOptions {
  /** Bỏ qua Data Cache của Next (trang danh sách SSR động). */
  noStore?: boolean;
  /** Thời gian ISR cho trang chủ (giây). */
  revalidate?: number;
}

export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  orderIndex: number;
  recipeCount: number;
}

export interface CategoryDetail {
  category: Category;
  recipes: RecipeListResponse;
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

export interface RecipeStatusResult {
  id: string;
  status: string;
}

export interface PresignResult {
  uploadUrl: string;
  objectKey: string;
  publicUrl: string;
  expiresAt: string;
}

export type RecipeStep = RecipeDetail["steps"][number];
export type RecipeIngredient = RecipeDetail["ingredients"][number];
export type RecipeImage = RecipeDetail["images"][number];

/* -------------------------------------------------------------------------- */
/* Recipes                                                                    */
/* -------------------------------------------------------------------------- */

export async function getRecipes(
  page = 1,
  pageSize = 12,
  filters: RecipeListParams = {},
  options: RecipeListOptions = {}
): Promise<RecipeListResponse> {
  const query = buildQuery({
    page,
    pageSize,
    categoryId: filters.categoryId,
    difficulty: filters.difficulty,
    maxCookTime: filters.maxCookTime,
    sort: filters.sort,
  });
  const hasFilters = Object.values(filters).some(
    (value) => value !== undefined && value !== null && value !== ""
  );
  const noStore = options.noStore ?? hasFilters;
  const revalidate = options.revalidate ?? 60;

  return http.get<RecipeListResponse>(`/api/v1/recipes${query}`, {
    // Trang danh sách là SSR động; trang chủ dùng ISR theo SRS (3600s).
    cache: noStore ? "no-store" : undefined,
    next: noStore ? undefined : { revalidate, tags: ["recipes"] },
    fallbackMessage: "Không tải được danh sách công thức.",
  });
}

export async function getRecipeBySlug(
  slug: string
): Promise<RecipeDetail | null> {
  try {
    return await http.get<RecipeDetail>(
      `/api/v1/recipes/${encodeURIComponent(slug)}`,
      {
        next: { revalidate: 300, tags: ["recipes", `recipe-${slug}`] },
        fallbackMessage: "Không tải được công thức.",
      }
    );
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
}

export async function createRecipe(
  input: CreateRecipeInput,
  token?: string | null
): Promise<RecipeDetail> {
  return http.post<RecipeDetail>("/api/v1/recipes", input, {
    token,
    cache: "no-store",
    fallbackMessage: "Tạo công thức thất bại.",
  });
}

export async function updateRecipe(
  id: string,
  input: UpdateRecipeInput,
  token?: string | null
): Promise<RecipeDetail> {
  return http.put<RecipeDetail>(
    `/api/v1/recipes/${encodeURIComponent(id)}`,
    input,
    {
      token,
      cache: "no-store",
      fallbackMessage: "Cập nhật công thức thất bại.",
    }
  );
}

export async function getMyRecipes(
  page = 1,
  pageSize = 12,
  status?: string,
  token?: string | null
): Promise<RecipeListResponse> {
  const query = buildQuery({
    page,
    pageSize,
    status: status && status !== "all" ? status : undefined,
  });
  return http.get<RecipeListResponse>(`/api/v1/recipes/mine${query}`, {
    token,
    cache: "no-store",
    fallbackMessage: "Tải danh sách công thức thất bại.",
  });
}

/**
 * Lấy chi tiết công thức của chính tác giả (kể cả bản nháp/lưu trữ).
 * Dùng cho route /dashboard/recipes/[id]/edit.
 */
export async function getMyRecipeById(
  id: string,
  token?: string | null
): Promise<RecipeDetail | null> {
  try {
    return await http.get<RecipeDetail>(
      `/api/v1/recipes/mine/${encodeURIComponent(id)}`,
      {
        token,
        cache: "no-store",
        fallbackMessage: "Không tải được công thức.",
      }
    );
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
}

/**
 * Tìm công thức trong danh sách của chính tác giả.
 * Giữ lại để tương thích URL cũ chỉ có slug.
 */
export async function findMyRecipeById(
  id: string,
  token?: string | null
): Promise<RecipeSummary | null> {
  const data = await getMyRecipes(1, 50, "all", token);
  return data.items.find((item) => item.id === id) ?? null;
}

async function changeRecipeStatus(
  id: string,
  action: "publish" | "unpublish" | "archive",
  token?: string | null
): Promise<RecipeStatusResult> {
  return http.patch<RecipeStatusResult>(
    `/api/v1/recipes/${encodeURIComponent(id)}/${action}`,
    undefined,
    {
      token,
      cache: "no-store",
      fallbackMessage: "Đổi trạng thái công thức thất bại.",
    }
  );
}

export const publishRecipe = (id: string, token?: string | null) =>
  changeRecipeStatus(id, "publish", token);
export const unpublishRecipe = (id: string, token?: string | null) =>
  changeRecipeStatus(id, "unpublish", token);
export const archiveRecipe = (id: string, token?: string | null) =>
  changeRecipeStatus(id, "archive", token);

export async function deleteRecipe(
  id: string,
  token?: string | null
): Promise<void> {
  await http.delete<void>(`/api/v1/recipes/${encodeURIComponent(id)}`, {
    token,
    cache: "no-store",
    fallbackMessage: "Xóa công thức thất bại.",
  });
}

/* -------------------------------------------------------------------------- */
/* Recipe child data (steps / ingredients / images)                           */
/* -------------------------------------------------------------------------- */

export const stepApi = {
  create(
    recipeId: string,
    body: {
      title: string;
      description: string;
      timerMinutes?: number | null;
      imageUrl?: string | null;
    }
  ): Promise<RecipeStep> {
    return http.post<RecipeStep>(
      `/api/v1/recipes/${recipeId}/steps`,
      body,
      { fallbackMessage: "Thêm bước thất bại." }
    );
  },
  update(
    recipeId: string,
    stepId: string,
    body: {
      title: string;
      description: string;
      timerMinutes?: number | null;
      imageUrl?: string | null;
    }
  ): Promise<RecipeStep> {
    return http.put<RecipeStep>(
      `/api/v1/recipes/${recipeId}/steps/${stepId}`,
      body,
      { fallbackMessage: "Cập nhật bước thất bại." }
    );
  },
  async remove(recipeId: string, stepId: string): Promise<void> {
    await http.delete<void>(
      `/api/v1/recipes/${recipeId}/steps/${stepId}`,
      { fallbackMessage: "Xóa bước thất bại." }
    );
  },
  reorder(
    recipeId: string,
    items: { id: string; order: number }[]
  ): Promise<RecipeStep[]> {
    return http.patch<RecipeStep[]>(
      `/api/v1/recipes/${recipeId}/steps/reorder`,
      items,
      { fallbackMessage: "Sắp xếp bước thất bại." }
    );
  },
};

export const ingredientApi = {
  create(
    recipeId: string,
    body: {
      name: string;
      quantity?: number | null;
      unit?: string | null;
      notes?: string | null;
    }
  ): Promise<RecipeIngredient> {
    return http.post<RecipeIngredient>(
      `/api/v1/recipes/${recipeId}/ingredients`,
      body,
      { fallbackMessage: "Thêm nguyên liệu thất bại." }
    );
  },
  update(
    recipeId: string,
    ingId: string,
    body: {
      name: string;
      quantity?: number | null;
      unit?: string | null;
      notes?: string | null;
    }
  ): Promise<RecipeIngredient> {
    return http.put<RecipeIngredient>(
      `/api/v1/recipes/${recipeId}/ingredients/${ingId}`,
      body,
      { fallbackMessage: "Cập nhật nguyên liệu thất bại." }
    );
  },
  async remove(recipeId: string, ingId: string): Promise<void> {
    await http.delete<void>(
      `/api/v1/recipes/${recipeId}/ingredients/${ingId}`,
      { fallbackMessage: "Xóa nguyên liệu thất bại." }
    );
  },
  reorder(
    recipeId: string,
    items: { id: string; order: number }[]
  ): Promise<RecipeIngredient[]> {
    return http.patch<RecipeIngredient[]>(
      `/api/v1/recipes/${recipeId}/ingredients/reorder`,
      items,
      { fallbackMessage: "Sắp xếp nguyên liệu thất bại." }
    );
  },
};

export const imageApi = {
  presign(recipeId: string, file: File): Promise<PresignResult> {
    return http.post<PresignResult>(
      `/api/v1/recipes/${recipeId}/images/presign`,
      { fileName: file.name, contentType: file.type, fileSize: file.size },
      { fallbackMessage: "Xin presigned URL thất bại." }
    );
  },
  confirm(
    recipeId: string,
    objectKey: string,
    altText?: string | null
  ): Promise<RecipeImage> {
    return http.post<RecipeImage>(
      `/api/v1/recipes/${recipeId}/images/confirm`,
      { objectKey, altText: altText ?? null },
      { fallbackMessage: "Xác nhận ảnh thất bại." }
    );
  },
  setPrimary(recipeId: string, imgId: string): Promise<RecipeImage> {
    return http.patch<RecipeImage>(
      `/api/v1/recipes/${recipeId}/images/${imgId}/primary`,
      undefined,
      { fallbackMessage: "Đặt ảnh chính thất bại." }
    );
  },
  async remove(recipeId: string, imgId: string): Promise<void> {
    await http.delete<void>(
      `/api/v1/recipes/${recipeId}/images/${imgId}`,
      { fallbackMessage: "Xóa ảnh thất bại." }
    );
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
      if (e.lengthComputable)
        onProgress(Math.round((e.loaded / e.total) * 100));
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

/* -------------------------------------------------------------------------- */
/* TUẦN 4 (Trần Gia Huy): MinIO Reliability + Orphan + Validation             */
/* -------------------------------------------------------------------------- */

export interface StorageHealth {
  status: "healthy" | "unhealthy";
  checkedAt: string;
}

export async function getStorageHealth(
  signal?: AbortSignal
): Promise<StorageHealth> {
  try {
    return await http.get<StorageHealth>("/api/v1/storage/health", {
      cache: "no-store",
      signal,
      fallbackMessage: "Không kiểm tra được trạng thái storage.",
    });
  } catch (error) {
    // 503 vẫn mang nghĩa storage không sẵn sàng — UI cần banner thay vì lỗi.
    if (error instanceof ApiError && error.status === 503) {
      return { status: "unhealthy", checkedAt: new Date().toISOString() };
    }
    throw error;
  }
}

export interface OrphanReport {
  totalObjects: number;
  referencedObjects: number;
  orphanCount: number;
  orphanBytes: number;
  orphanKeys: string[];
  checkedAt: string;
  dryRun: boolean;
}

export async function getOrphanPreview(): Promise<OrphanReport> {
  return http.get<OrphanReport>("/api/v1/admin/storage/orphans", {
    cache: "no-store",
    fallbackMessage: "Quét file mồ côi thất bại.",
  });
}

export async function cleanupOrphans(): Promise<OrphanReport> {
  return http.post<OrphanReport>(
    "/api/v1/admin/storage/orphans/cleanup",
    undefined,
    {
      cache: "no-store",
      fallbackMessage: "Dọn file mồ côi thất bại.",
    }
  );
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
  recipeId: string
): Promise<RecipeValidation> {
  return http.get<RecipeValidation>(
    `/api/v1/recipes/${encodeURIComponent(recipeId)}/validation`,
    {
      cache: "no-store",
      fallbackMessage: "Tải checklist validation thất bại.",
    }
  );
}

/* -------------------------------------------------------------------------- */
/* Categories & search                                                        */
/* -------------------------------------------------------------------------- */

export async function getCategories(): Promise<Category[]> {
  return http.get<Category[]>("/api/v1/categories", {
    next: { revalidate: 3600, tags: ["categories"] },
    fallbackMessage: "Không tải được danh mục.",
  });
}

export async function getCategoryBySlug(
  slug: string,
  page = 1,
  pageSize = 12
): Promise<CategoryDetail | null> {
  try {
    return await http.get<CategoryDetail>(
      `/api/v1/categories/${encodeURIComponent(slug)}${buildQuery({ page, pageSize })}`,
      {
        next: { revalidate: 600 },
        fallbackMessage: "Không tải được danh mục.",
      }
    );
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) return null;
    throw error;
  }
}

export async function searchRecipes(
  params: SearchParams
): Promise<SearchResponse> {
  return http.get<SearchResponse>(
    `/api/v1/recipes/search${buildQuery({
      q: params.q,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 12,
      categoryId: params.categoryId,
      difficulty: params.difficulty,
      maxCookTime: params.maxCookTime,
      sort: params.sort,
    })}`,
    {
      cache: "no-store",
      fallbackMessage: "Tìm kiếm công thức thất bại.",
    }
  );
}

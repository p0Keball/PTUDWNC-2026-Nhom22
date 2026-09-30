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

function authHeaders(token?: string): Record<string, string> {
  const headers: Record<string, string> = { "Content-Type": "application/json" };
  if (token) headers["Authorization"] = `Bearer ${token}`;
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

const API_BASE =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

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

export async function createRecipe(
  input: CreateRecipeInput,
  token?: string
): Promise<RecipeDetail> {
  const res = await fetch(`${API_BASE}/api/v1/recipes`, {
    method: "POST",
    headers: authHeaders(token),
    body: JSON.stringify(input),
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Tạo công thức thất bại.");
  return res.json();
}

export async function updateRecipe(
  id: string,
  input: UpdateRecipeInput,
  token?: string
): Promise<RecipeDetail> {
  const res = await fetch(`${API_BASE}/api/v1/recipes/${encodeURIComponent(id)}`, {
    method: "PUT",
    headers: authHeaders(token),
    body: JSON.stringify(input),
    cache: "no-store",
  });
  if (!res.ok) await throwApiError(res, "Cập nhật công thức thất bại.");
  return res.json();
}

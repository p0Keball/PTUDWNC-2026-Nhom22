import type { RecipeDetail, RecipeListResponse, RecipeSummary } from "../types/recipe";
import type {
  Category,
  CategoryDetail,
  CategoryListResponse,
} from "../types/category";
import type { SearchParams, SearchResponse } from "../types/search";

export type {
  RecipeDetail,
  RecipeListResponse,
  RecipeSummary,
  Category,
  CategoryDetail,
  CategoryListResponse,
  SearchParams,
  SearchResponse,
};

const API_BASE =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

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

export async function getCategories(): Promise<CategoryListResponse> {
  const res = await fetch(`${API_BASE}/api/v1/categories`, {
    next: { revalidate: 3600 },
  });
  if (!res.ok) throw new Error(`Failed to fetch categories: ${res.status}`);
  return res.json();
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

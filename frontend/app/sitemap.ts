import type { MetadataRoute } from "next";
import { API_BASE_URL } from "@/lib/http";

/**
 * NFR-SEO-003: sitemap.xml động cho toàn bộ trang tĩnh + Published recipes
 * + danh mục. Next.js cache route này; dữ liệu được fetch từ Backend API.
 */
const SITE_URL = (
  process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000"
).replace(/\/+$/, "");

interface RecipeSitemapItem {
  slug: string;
  publishedAt: string | null;
}

interface CategorySitemapItem {
  id: string;
  slug: string;
}

interface PagedResponse<T> {
  items: T[];
  totalPages?: number;
  page?: number;
}

async function fetchJson<T>(path: string): Promise<T | null> {
  try {
    const response = await fetch(`${API_BASE_URL}${path}`, {
      next: { revalidate: 3600, tags: ["recipes"] },
    });
    if (!response.ok) return null;
    return (await response.json()) as T;
  } catch {
    // Sitemap không được làm sập build khi Backend chưa chạy.
    return null;
  }
}

async function fetchAllPublishedRecipes(): Promise<RecipeSitemapItem[]> {
  const recipes: RecipeSitemapItem[] = [];
  let page = 1;
  let totalPages = 1;

  do {
    const data = await fetchJson<PagedResponse<RecipeSitemapItem>>(
      `/api/v1/recipes?page=${page}&pageSize=50&sort=-publishedAt`
    );
    if (!data) break;

    recipes.push(...data.items);
    totalPages = data.totalPages ?? 1;
    page += 1;
  } while (page <= totalPages && page <= 100);

  return recipes;
}

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const [recipes, categories] = await Promise.all([
    fetchAllPublishedRecipes(),
    fetchJson<CategorySitemapItem[]>("/api/v1/categories"),
  ]);

  const staticEntries: MetadataRoute.Sitemap = [
    {
      url: `${SITE_URL}/`,
      lastModified: new Date(),
      changeFrequency: "daily",
      priority: 1,
    },
    {
      url: `${SITE_URL}/recipes`,
      lastModified: new Date(),
      changeFrequency: "daily",
      priority: 0.9,
    },
    {
      url: `${SITE_URL}/categories`,
      lastModified: new Date(),
      changeFrequency: "weekly",
      priority: 0.8,
    },
  ];

  const categoryEntries: MetadataRoute.Sitemap = (categories ?? []).map(
    (category) => ({
      url: `${SITE_URL}/categories/${encodeURIComponent(category.slug)}`,
      lastModified: new Date(),
      changeFrequency: "weekly",
      priority: 0.7,
    })
  );

  const recipeEntries: MetadataRoute.Sitemap = recipes.map((recipe) => ({
    url: `${SITE_URL}/recipes/${encodeURIComponent(recipe.slug)}`,
    lastModified: recipe.publishedAt ? new Date(recipe.publishedAt) : new Date(),
    changeFrequency: "weekly",
    priority: 0.8,
  }));

  return [...staticEntries, ...categoryEntries, ...recipeEntries];
}

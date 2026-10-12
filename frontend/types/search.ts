import type { RecipeSummary } from "./recipe";

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

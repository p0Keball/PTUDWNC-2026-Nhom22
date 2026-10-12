export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  orderIndex: number;
  recipeCount: number;
}

export type CategoryListResponse = Category[];

export interface CategoryDetail {
  category: Category;
  recipes: {
    items: import("./recipe").RecipeSummary[];
    totalCount: number;
    page: number;
    pageSize: number;
  };
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
  imageUrl?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
  imageUrl?: string;
}

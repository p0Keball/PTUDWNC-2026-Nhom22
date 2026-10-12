export interface RecipeStep {
  id: string;
  stepNumber: number;
  title: string;
  description: string;
  timerMinutes: number | null;
  imageUrl: string | null;
}

export interface RecipeIngredient {
  id: string;
  name: string;
  quantity: number | null;
  unit: string | null;
  notes: string | null;
  orderIndex: number;
}

export interface RecipeImage {
  id: string;
  originalUrl: string;
  mediumUrl: string | null;
  thumbnailUrl: string | null;
  altText: string | null;
  isPrimary: boolean;
  orderIndex: number;
  thumbnailStatus: string;
}

export interface RecipeNutrition {
  calories: number | null;
  protein: number | null;
  carbohydrates: number | null;
  fat: number | null;
  fiber: number | null;
  sodium: number | null;
}

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
  nutrition: RecipeNutrition | null;
  steps: RecipeStep[];
  ingredients: RecipeIngredient[];
  images: RecipeImage[];
}

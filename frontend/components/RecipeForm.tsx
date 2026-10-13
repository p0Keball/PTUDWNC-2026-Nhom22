import type { Category } from "@/lib/api";

export interface RecipeFormValues {
  title: string;
  description: string;
  instructions: string;
  categoryId: string;
  prepTimeMinutes: string;
  cookTimeMinutes: string;
  servings: string;
  difficulty: string;
}

export const emptyRecipeForm: RecipeFormValues = {
  title: "",
  description: "",
  instructions: "",
  categoryId: "",
  prepTimeMinutes: "",
  cookTimeMinutes: "",
  servings: "",
  difficulty: "1",
};

export type RecipeFormErrors = Partial<Record<keyof RecipeFormValues, string>>;

const BACKEND_KEY_MAP: Record<string, keyof RecipeFormValues> = {
  title: "title",
  description: "description",
  instructions: "instructions",
  categoryId: "categoryId",
  preptimeMinutes: "prepTimeMinutes",
  cooktimeMinutes: "cookTimeMinutes",
  servings: "servings",
  difficulty: "difficulty",
};

export function mapBackendErrors(errors: Record<string, string[]>): RecipeFormErrors {
  const mapped: RecipeFormErrors = {};
  for (const [key, messages] of Object.entries(errors)) {
    const field =
      BACKEND_KEY_MAP[key] ?? BACKEND_KEY_MAP[key.toLowerCase()];
    if (field && messages.length > 0) mapped[field] = messages[0];
  }
  return mapped;
}

export function validateRecipeForm(v: RecipeFormValues): RecipeFormErrors {
  const errors: RecipeFormErrors = {};
  const title = v.title.trim();
  if (!title) errors.title = "Tiêu đề không được để trống.";
  else if (title.length < 5 || title.length > 200)
    errors.title = "Tiêu đề phải từ 5 đến 200 ký tự.";
  if (!v.description.trim()) errors.description = "Mô tả không được để trống.";
  if (!v.instructions.trim())
    errors.instructions = "Hướng dẫn không được để trống.";
  if (!v.categoryId) errors.categoryId = "Vui lòng chọn danh mục.";
  const prep = Number(v.prepTimeMinutes);
  if (!v.prepTimeMinutes.trim() || !Number.isInteger(prep) || prep <= 0)
    errors.prepTimeMinutes = "Thời gian chuẩn bị phải là số nguyên > 0.";
  const cook = Number(v.cookTimeMinutes);
  if (
    v.cookTimeMinutes.trim() === "" ||
    !Number.isInteger(cook) ||
    cook < 0
  )
    errors.cookTimeMinutes = "Thời gian nấu phải là số nguyên >= 0.";
  const servings = Number(v.servings);
  if (!v.servings.trim() || !Number.isInteger(servings) || servings <= 0)
    errors.servings = "Khẩu phần phải là số nguyên > 0.";
  const difficulty = Number(v.difficulty);
  if (![1, 2, 3, 4].includes(difficulty))
    errors.difficulty = "Độ khó phải từ 1 đến 4.";
  return errors;
}

function fieldClass(hasError: boolean) {
  return `w-full rounded-lg border px-3 py-2 text-sm outline-none transition focus:ring-2 ${
    hasError
      ? "border-red-400 focus:ring-red-200"
      : "border-zinc-300 focus:border-zinc-500 focus:ring-zinc-200 dark:border-zinc-700 dark:bg-zinc-900"
  }`;
}

interface RecipeFormProps {
  values: RecipeFormValues;
  errors: RecipeFormErrors;
  categories: Category[];
  disabled?: boolean;
  onChange: (values: RecipeFormValues) => void;
}

export default function RecipeForm({
  values,
  errors,
  categories,
  disabled = false,
  onChange,
}: RecipeFormProps) {
  const set = (patch: Partial<RecipeFormValues>) =>
    onChange({ ...values, ...patch });

  const renderError = (field: keyof RecipeFormValues) =>
    errors[field] ? (
      <p className="mt-1 text-xs text-red-600">{errors[field]}</p>
    ) : null;

  return (
    <div className="flex flex-col gap-4">
      <div>
        <label htmlFor="recipe-title" className="mb-1 block text-sm font-medium">
          Tiêu đề *
        </label>
        <input
          id="recipe-title"
          value={values.title}
          disabled={disabled}
          onChange={(e) => set({ title: e.target.value })}
          placeholder="Ví dụ: Phở bò Hà Nội"
          className={fieldClass(!!errors.title)}
        />
        {renderError("title")}
      </div>

      <div>
        <label
          htmlFor="recipe-category"
          className="mb-1 block text-sm font-medium"
        >
          Danh mục *
        </label>
        <select
          id="recipe-category"
          value={values.categoryId}
          disabled={disabled}
          onChange={(e) => set({ categoryId: e.target.value })}
          className={fieldClass(!!errors.categoryId)}
        >
          <option value="">— Chọn danh mục —</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>
        {renderError("categoryId")}
      </div>

      <div>
        <label
          htmlFor="recipe-description"
          className="mb-1 block text-sm font-medium"
        >
          Mô tả *
        </label>
        <textarea
          id="recipe-description"
          value={values.description}
          disabled={disabled}
          onChange={(e) => set({ description: e.target.value })}
          rows={3}
          placeholder="Mô tả ngắn về món ăn"
          className={fieldClass(!!errors.description)}
        />
        {renderError("description")}
      </div>

      <div>
        <label
          htmlFor="recipe-instructions"
          className="mb-1 block text-sm font-medium"
        >
          Hướng dẫn chung *
        </label>
        <textarea
          id="recipe-instructions"
          value={values.instructions}
          disabled={disabled}
          onChange={(e) => set({ instructions: e.target.value })}
          rows={5}
          placeholder="Các bước sơ chế, nấu và trình bày"
          className={fieldClass(!!errors.instructions)}
        />
        {renderError("instructions")}
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div>
          <label
            htmlFor="recipe-prep"
            className="mb-1 block text-sm font-medium"
          >
            Sơ chế (phút) *
          </label>
          <input
            id="recipe-prep"
            type="number"
            min={1}
            value={values.prepTimeMinutes}
            disabled={disabled}
            onChange={(e) => set({ prepTimeMinutes: e.target.value })}
            className={fieldClass(!!errors.prepTimeMinutes)}
          />
          {renderError("prepTimeMinutes")}
        </div>
        <div>
          <label
            htmlFor="recipe-cook"
            className="mb-1 block text-sm font-medium"
          >
            Nấu (phút) *
          </label>
          <input
            id="recipe-cook"
            type="number"
            min={0}
            value={values.cookTimeMinutes}
            disabled={disabled}
            onChange={(e) => set({ cookTimeMinutes: e.target.value })}
            className={fieldClass(!!errors.cookTimeMinutes)}
          />
          {renderError("cookTimeMinutes")}
        </div>
        <div>
          <label
            htmlFor="recipe-servings"
            className="mb-1 block text-sm font-medium"
          >
            Khẩu phần *
          </label>
          <input
            id="recipe-servings"
            type="number"
            min={1}
            value={values.servings}
            disabled={disabled}
            onChange={(e) => set({ servings: e.target.value })}
            className={fieldClass(!!errors.servings)}
          />
          {renderError("servings")}
        </div>
        <div>
          <label
            htmlFor="recipe-difficulty"
            className="mb-1 block text-sm font-medium"
          >
            Độ khó *
          </label>
          <select
            id="recipe-difficulty"
            value={values.difficulty}
            disabled={disabled}
            onChange={(e) => set({ difficulty: e.target.value })}
            className={fieldClass(!!errors.difficulty)}
          >
            <option value="1">1 — Dễ</option>
            <option value="2">2 — Trung bình</option>
            <option value="3">3 — Khó</option>
            <option value="4">4 — Chuyên gia</option>
          </select>
          {renderError("difficulty")}
        </div>
      </div>
    </div>
  );
}

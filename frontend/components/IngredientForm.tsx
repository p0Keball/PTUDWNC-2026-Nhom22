"use client";

import { useEffect } from "react";
import { useFieldArray, useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { ingredientApi, type RecipeIngredient } from "@/lib/api";

const itemSchema = z.object({
  key: z.string(),
  id: z.string().nullable(),
  name: z.string().trim().min(2, "Tên tối thiểu 2 ký tự.").max(200, "Tên tối đa 200 ký tự."),
  quantity: z
    .union([z.number(), z.nan(), z.null()])
    .transform((v) => (typeof v === "number" && Number.isNaN(v) ? null : v))
    .nullable()
    .refine((v) => v === null || v > 0, "Số lượng phải > 0."),
  unit: z.string().max(50).nullable().optional(),
  notes: z.string().max(500).nullable().optional(),
});

const formSchema = z.object({ items: z.array(itemSchema) });

type FormValues = z.infer<typeof formSchema>;

interface Props {
  recipeId?: string | null;
  defaultIngredients?: RecipeIngredient[];
  onChange?: (items: FormValues["items"]) => void;
}

function toKey(): string {
  return Math.random().toString(36).slice(2);
}

export default function IngredientForm({ recipeId = null, defaultIngredients = [], onChange }: Props) {
  const {
    control,
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      items: defaultIngredients.map((i) => ({
        key: i.id,
        id: i.id,
        name: i.name,
        quantity: i.quantity,
        unit: i.unit ?? "",
        notes: i.notes ?? "",
      })),
    },
  });

  const { fields, append, remove, update } = useFieldArray({ control, name: "items" });
  const items = watch("items");

  useEffect(() => {
    onChange?.(items);
  }, [items, onChange]);

  useEffect(() => {
    reset({
      items: defaultIngredients.map((i) => ({
        key: i.id,
        id: i.id,
        name: i.name,
        quantity: i.quantity,
        unit: i.unit ?? "",
        notes: i.notes ?? "",
      })),
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [defaultIngredients.length]);

  async function persistRow(index: number, value: FormValues["items"][number]): Promise<void> {
    if (!recipeId) return;
    const body = {
      name: value.name.trim(),
      quantity: value.quantity,
      unit: value.unit?.trim() || null,
      notes: value.notes?.trim() || null,
    };
    if (value.id) {
      const updated = await ingredientApi.update(recipeId, value.id, body);
      update(index, { ...value, name: updated.name, quantity: updated.quantity, unit: updated.unit, notes: updated.notes });
    } else {
      const created = await ingredientApi.create(recipeId, body);
      update(index, { ...value, key: created.id, id: created.id });
    }
  }

  async function handleRemove(index: number) {
    const row = items[index];
    if (recipeId && row?.id) {
      await ingredientApi.remove(recipeId, row.id);
    }
    remove(index);
  }

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit(() => {})}>
      <div className="flex items-center justify-between">
        <h3 className="font-semibold">Nguyên liệu ({fields.length})</h3>
        <button
          type="button"
          onClick={() => append({ key: toKey(), id: null, name: "", quantity: null, unit: "", notes: "" })}
          className="rounded-full border px-4 py-1.5 text-sm hover:bg-zinc-100 dark:hover:bg-zinc-800"
        >
          + Thêm
        </button>
      </div>

      {fields.length === 0 && (
        <p className="text-sm text-zinc-500">Chưa có nguyên liệu nào. Nhấn “Thêm”.</p>
      )}

      {fields.map((field, index) => (
        <div key={field.id} className="flex flex-col gap-2 rounded-xl border p-3 dark:border-zinc-800">
          <div className="grid grid-cols-1 gap-2 sm:grid-cols-[1fr_110px_90px]">
            <div>
              <input
                {...register(`items.${index}.name`)}
                placeholder="Tên nguyên liệu *"
                className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-900"
              />
              {errors.items?.[index]?.name && (
                <p className="mt-1 text-xs text-red-600">{errors.items[index]?.name?.message}</p>
              )}
            </div>
            <input
              type="number"
              step="0.001"
              {...register(`items.${index}.quantity`, { valueAsNumber: true })}
              placeholder="SL"
              className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-900"
            />
            <input
              {...register(`items.${index}.unit`)}
              placeholder="Đơn vị"
              className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-900"
            />
          </div>
          <input
            {...register(`items.${index}.notes`)}
            placeholder="Ghi chú"
            className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-900"
          />
          {errors.items?.[index]?.quantity && (
            <p className="text-xs text-red-600">{errors.items[index]?.quantity?.message}</p>
          )}
          <div className="flex gap-2">
            {recipeId && (
              <button
                type="button"
                onClick={() =>
                  handleSubmit(async (values) => {
                    await persistRow(index, values.items[index]);
                  })()
                }
                className="rounded-lg bg-zinc-900 px-3 py-1.5 text-xs text-white dark:bg-zinc-100 dark:text-zinc-900"
              >
                Lưu
              </button>
            )}
            <button
              type="button"
              onClick={() => handleRemove(index)}
              className="rounded-lg border px-3 py-1.5 text-xs text-red-600"
            >
              Xóa
            </button>
          </div>
        </div>
      ))}
    </form>
  );
}

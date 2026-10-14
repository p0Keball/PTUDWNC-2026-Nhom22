"use client";

import { useEffect, useState } from "react";
import { useFieldArray, useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from "@dnd-kit/core";
import {
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
  arrayMove,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
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
  unit: z.string().max(50, "Đơn vị tối đa 50 ký tự.").nullable().optional(),
  notes: z.string().max(500, "Ghi chú tối đa 500 ký tự.").nullable().optional(),
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

function SortableRow({
  rowKey,
  index,
  children,
}: {
  rowKey: string;
  index: number;
  children: React.ReactNode;
}) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: rowKey,
  });
  return (
    <div
      ref={setNodeRef}
      style={{ transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.6 : 1 }}
      className="flex flex-col gap-2 rounded-xl border p-3 dark:border-zinc-800"
    >
      <div className="flex items-center gap-2">
        <button
          type="button"
          {...attributes}
          {...listeners}
          title="Kéo để sắp xếp"
          className="cursor-grab rounded-lg border px-2 py-1 text-xs text-zinc-500 active:cursor-grabbing"
        >
          ⠿ Kéo
        </button>
        <span className="text-xs font-semibold text-zinc-500">#{index + 1}</span>
      </div>
      {children}
    </div>
  );
}

export default function IngredientForm({ recipeId = null, defaultIngredients = [], onChange }: Props) {
  const [dragError, setDragError] = useState<string | null>(null);
  const {
    control,
    register,
    handleSubmit,
    reset,
    watch,
    setValue,
    getValues,
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

  const { fields, append, remove, update, move } = useFieldArray({ control, name: "items" });
  const items = watch("items");

  // TUẦN 4 — Form Validation: phát hiện trùng tên (normalize trim+lower).
  const duplicateNames = (() => {
    const seen = new Map<string, number>();
    const dups = new Set<string>();
    for (const it of items ?? []) {
      const k = (it.name ?? "").trim().toLowerCase();
      if (!k) continue;
      if (seen.has(k)) dups.add(k);
      else seen.set(k, 1);
    }
    return dups;
  })();

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

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 4 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates })
  );

  // TUẦN 4 — Drag & Drop reorder nguyên liệu (BE đã có PATCH /ingredients/reorder).
  async function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;
    if (!over || active.id === over.id) return;
    const oldIndex = fields.findIndex((f) => f.key === String(active.id));
    const nextIndex = fields.findIndex((f) => f.key === String(over.id));
    if (oldIndex < 0 || nextIndex < 0) return;

    move(oldIndex, nextIndex);
    setDragError(null);

    if (recipeId) {
      try {
        const current = getValues("items");
        const reordered = arrayMove(current, oldIndex, nextIndex);
        const persisted = reordered.filter((r) => r.id !== null);
        if (persisted.length > 0) {
          await ingredientApi.reorder(
            recipeId,
            reordered.map((r, idx) => ({ id: r.id as string, order: idx })).filter((r) => r.id)
          );
        }
        reordered.forEach((row, idx) => setValue(`items.${idx}`, row, { shouldDirty: true }));
      } catch (e) {
        setDragError(e instanceof Error ? e.message : "Reorder thất bại, vui lòng thử lại.");
        move(nextIndex, oldIndex);
      }
    }
  }

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

      {dragError && <p className="text-sm text-red-600">{dragError}</p>}
      {fields.length === 0 && (
        <p className="rounded-xl border border-amber-300 bg-amber-50 px-3 py-2 text-sm text-amber-800">
          Chưa có nguyên liệu nào — công thức cần ít nhất 1 nguyên liệu. Nhấn “Thêm”.
        </p>
      )}
      {duplicateNames.size > 0 && (
        <p className="rounded-xl border border-amber-300 bg-amber-50 px-3 py-2 text-sm text-amber-800">
          Có tên nguyên liệu bị trùng — BE sẽ trả 422 khi lưu. Vui lòng gộp hoặc đổi tên.
        </p>
      )}

      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
        <SortableContext items={fields.map((f) => f.key)} strategy={verticalListSortingStrategy}>
          {fields.map((field, index) => (
            <SortableRow key={field.id} rowKey={field.key} index={index}>
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
                  {duplicateNames.has((items?.[index]?.name ?? "").trim().toLowerCase()) &&
                    (items?.[index]?.name ?? "").trim() !== "" && (
                      <p className="mt-1 text-xs text-amber-700">Tên này bị trùng với dòng khác.</p>
                    )}
                </div>
                <div>
                  <input
                    type="number"
                    step="0.001"
                    {...register(`items.${index}.quantity`, { valueAsNumber: true })}
                    placeholder="SL"
                    className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-900"
                  />
                  {errors.items?.[index]?.quantity && (
                    <p className="mt-1 text-xs text-red-600">{errors.items[index]?.quantity?.message}</p>
                  )}
                </div>
                <div>
                  <input
                    {...register(`items.${index}.unit`)}
                    placeholder="Đơn vị"
                    className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-900"
                  />
                  {errors.items?.[index]?.unit && (
                    <p className="mt-1 text-xs text-red-600">{errors.items[index]?.unit?.message}</p>
                  )}
                </div>
              </div>
              <div>
                <input
                  {...register(`items.${index}.notes`)}
                  placeholder="Ghi chú"
                  className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-900"
                />
                {errors.items?.[index]?.notes && (
                  <p className="mt-1 text-xs text-red-600">{errors.items[index]?.notes?.message}</p>
                )}
              </div>
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
            </SortableRow>
          ))}
        </SortableContext>
      </DndContext>
    </form>
  );
}

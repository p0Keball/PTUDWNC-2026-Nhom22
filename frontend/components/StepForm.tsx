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
import { stepApi, type RecipeStep } from "@/lib/api";

const itemSchema = z.object({
  key: z.string(),
  id: z.string().nullable(),
  title: z.string().trim().min(3, "Tiêu đề tối thiểu 3 ký tự.").max(200, "Tiêu đề tối đa 200 ký tự."),
  description: z.string().max(5000),
  timerMinutes: z
    .union([z.number(), z.nan(), z.null()])
    .transform((v) => (typeof v === "number" && Number.isNaN(v) ? null : v))
    .nullable()
    .refine((v) => v === null || v >= 0, "Timer phải >= 0."),
  imageUrl: z.string().max(500).nullable().optional(),
});

const formSchema = z.object({ items: z.array(itemSchema) });
type FormValues = z.infer<typeof formSchema>;

interface Props {
  recipeId?: string | null;
  defaultSteps?: RecipeStep[];
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
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({ id: rowKey });
  return (
    <div
      ref={setNodeRef}
      style={{ transform: CSS.Transform.toString(transform), transition, opacity: isDragging ? 0.6 : 1 }}
      className="rounded-xl border bg-white p-3 dark:border-zinc-800 dark:bg-zinc-900"
    >
      <div className="mb-2 flex items-center gap-2">
        <button
          type="button"
          {...attributes}
          {...listeners}
          title="Kéo để sắp xếp"
          className="cursor-grab rounded-lg border px-2 py-1 text-xs text-zinc-500 active:cursor-grabbing"
        >
          ⠿ Kéo
        </button>
        <span className="text-xs font-semibold text-zinc-500">Bước {index + 1}</span>
      </div>
      {children}
    </div>
  );
}

export default function StepForm({ recipeId = null, defaultSteps = [], onChange }: Props) {
  const [dragError, setDragError] = useState<string | null>(null);
  const {
    control,
    register,
    handleSubmit,
    watch,
    setValue,
    getValues,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      items: [...defaultSteps]
        .sort((a, b) => a.stepNumber - b.stepNumber)
        .map((s) => ({
          key: s.id,
          id: s.id,
          title: s.title,
          description: s.description,
          timerMinutes: s.timerMinutes,
          imageUrl: s.imageUrl ?? "",
        })),
    },
  });

  const { fields, append, remove, update, move } = useFieldArray({ control, name: "items" });
  const items = watch("items");

  useEffect(() => {
    onChange?.(items);
  }, [items, onChange]);

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 4 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates })
  );

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
          await stepApi.reorder(
            recipeId,
            reordered.map((r, idx) => ({ id: r.id as string, order: idx + 1 })).filter((r) => r.id)
          );
        }
        reordered.forEach((row, idx) => setValue(`items.${idx}`, row, { shouldDirty: true }));
      } catch (e) {
        setDragError(e instanceof Error ? e.message : "Reorder thất bại, vui lòng thử lại.");
        move(nextIndex, oldIndex);
      }
    }
  }

  async function persistRow(index: number, value: FormValues["items"][number]) {
    if (!recipeId) return;
    const body = {
      title: value.title.trim(),
      description: value.description ?? "",
      timerMinutes: value.timerMinutes,
      imageUrl: value.imageUrl?.trim() || null,
    };
    if (value.id) {
      const updated = await stepApi.update(recipeId, value.id, body);
      update(index, { ...value, title: updated.title, description: updated.description, timerMinutes: updated.timerMinutes, imageUrl: updated.imageUrl });
    } else {
      const created = await stepApi.create(recipeId, body);
      update(index, { ...value, key: created.id, id: created.id });
    }
  }

  async function handleRemove(index: number) {
    const row = items[index];
    if (recipeId && row?.id) {
      await stepApi.remove(recipeId, row.id);
    }
    remove(index);
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex items-center justify-between">
        <h3 className="font-semibold">Các bước ({fields.length})</h3>
        <button
          type="button"
          onClick={() => append({ key: toKey(), id: null, title: "", description: "", timerMinutes: null, imageUrl: "" })}
          className="rounded-full border px-4 py-1.5 text-sm hover:bg-zinc-100 dark:hover:bg-zinc-800"
        >
          + Thêm bước
        </button>
      </div>

      {dragError && <p className="text-sm text-red-600">{dragError}</p>}
      {fields.length === 0 && <p className="text-sm text-zinc-500">Chưa có bước nào.</p>}

      <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
        <SortableContext items={fields.map((f) => f.key)} strategy={verticalListSortingStrategy}>
          <div className="flex flex-col gap-2">
            {fields.map((field, index) => (
              <SortableRow key={field.id} rowKey={field.key} index={index}>
                <div className="flex flex-col gap-2">
                  <input
                    {...register(`items.${index}.title`)}
                    placeholder="Tiêu đề bước *"
                    className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-950"
                  />
                  {errors.items?.[index]?.title && (
                    <p className="text-xs text-red-600">{errors.items[index]?.title?.message}</p>
                  )}
                  <textarea
                    {...register(`items.${index}.description`)}
                    placeholder="Mô tả chi tiết"
                    rows={2}
                    className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-950"
                  />
                  <div className="grid grid-cols-2 gap-2">
                    <input
                      type="number"
                      {...register(`items.${index}.timerMinutes`, { valueAsNumber: true })}
                      placeholder="Timer (phút)"
                      className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-950"
                    />
                    <input
                      {...register(`items.${index}.imageUrl`)}
                      placeholder="Image URL (tùy chọn)"
                      className="w-full rounded-lg border px-3 py-2 text-sm dark:border-zinc-700 dark:bg-zinc-950"
                    />
                  </div>
                  <div className="flex gap-2">
                    {recipeId && (
                      <button
                        type="button"
                        onClick={() => handleSubmit(async (values) => persistRow(index, values.items[index]))()}
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
              </SortableRow>
            ))}
          </div>
        </SortableContext>
      </DndContext>

      {!recipeId && (
        <p className="text-xs text-zinc-500">
          Chế độ soạn thảo cục bộ: các bước sẽ được gửi cùng lúc khi tạo công thức.
        </p>
      )}
    </div>
  );
}

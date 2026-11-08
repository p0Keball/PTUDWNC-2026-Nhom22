"use client";

import { useCallback, useEffect, useState, type FormEvent } from "react";
import { FolderOpen, Pencil, Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";

import { AdminAuthGate } from "@/components/AdminAuthGate";
import { ConfirmDialog } from "@/components/ConfirmDialog";
import { EmptyState } from "@/components/EmptyState";
import { PageLoading } from "@/components/Loading";
import { PageHeader } from "@/components/PageHeader";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
  createCategory,
  deleteCategory,
  getCategories,
  updateCategory,
  type Category,
} from "@/lib/api";
import { getErrorMessage } from "@/lib/errors";
import { useAuthStore } from "@/store/auth-store";

interface CategoryFormState {
  name: string;
  description: string;
  imageUrl: string;
}

const EMPTY_FORM: CategoryFormState = {
  name: "",
  description: "",
  imageUrl: "",
};

export default function DashboardCategoriesPage() {
  return (
    <AdminAuthGate>
      <CategoriesManager />
    </AdminAuthGate>
  );
}

function CategoriesManager() {
  const token = useAuthStore((state) => state.accessToken);

  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Category | null>(null);
  const [form, setForm] = useState<CategoryFormState>(EMPTY_FORM);
  const [saving, setSaving] = useState(false);

  const [pendingDelete, setPendingDelete] = useState<Category | null>(null);
  const [deleting, setDeleting] = useState(false);

  const loadCategories = useCallback(async (showLoading = false) => {
    if (showLoading) setLoading(true);
    try {
      const result = await getCategories({ noStore: true });
      setCategories(result);
      setError(null);
    } catch (err) {
      setError(getErrorMessage(err, "Không tải được danh mục."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      try {
        const result = await getCategories({ noStore: true });
        if (cancelled) return;
        setCategories(result);
        setError(null);
      } catch (err) {
        if (!cancelled) {
          setError(getErrorMessage(err, "Không tải được danh mục."));
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  function openCreateDialog() {
    setEditing(null);
    setForm(EMPTY_FORM);
    setDialogOpen(true);
  }

  function openEditDialog(category: Category) {
    setEditing(category);
    setForm({
      name: category.name,
      description: category.description ?? "",
      imageUrl: category.imageUrl ?? "",
    });
    setDialogOpen(true);
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const name = form.name.trim();
    if (name.length < 2 || name.length > 50) {
      toast.error("Tên danh mục phải từ 2 đến 50 ký tự.");
      return;
    }

    setSaving(true);
    try {
      const input = {
        name,
        description: form.description.trim() || null,
        imageUrl: form.imageUrl.trim() || null,
      };

      if (editing) {
        await updateCategory(editing.id, input, token);
        toast.success("Đã cập nhật danh mục.");
      } else {
        await createCategory(input, token);
        toast.success("Đã tạo danh mục mới.");
      }

      setDialogOpen(false);
      await loadCategories();
    } catch (err) {
      toast.error(getErrorMessage(err, "Lưu danh mục thất bại."));
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete() {
    if (!pendingDelete) return;
    setDeleting(true);
    try {
      await deleteCategory(pendingDelete.id, token);
      toast.success("Đã xóa danh mục.");
      setPendingDelete(null);
      await loadCategories();
    } catch (err) {
      toast.error(getErrorMessage(err, "Xóa danh mục thất bại."));
    } finally {
      setDeleting(false);
    }
  }

  return (
    <>
      <PageHeader
        title="Quản lý danh mục"
        description="Tạo, chỉnh sửa và xóa danh mục công thức. Danh mục còn chứa công thức sẽ không thể xóa."
        actions={
          <Button type="button" onClick={openCreateDialog}>
            <Plus />
            Thêm danh mục
          </Button>
        }
      />

      {error && (
        <Alert variant="destructive">
          <AlertTitle>Không tải được danh mục</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {loading ? (
        <PageLoading label="Đang tải danh mục…" />
      ) : categories.length === 0 ? (
        <EmptyState
          icon={<FolderOpen className="size-6" />}
          title="Chưa có danh mục nào"
          description="Thêm danh mục đầu tiên để tác giả có thể phân loại công thức."
          action={
            <Button type="button" onClick={openCreateDialog}>
              <Plus />
              Thêm danh mục
            </Button>
          }
        />
      ) : (
        <div className="flex flex-col gap-3">
          {categories.map((category) => (
            <Card key={category.id} className="py-0">
              <CardContent className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between">
                <div className="flex min-w-0 items-start gap-3">
                  <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground">
                    <FolderOpen className="size-5" />
                  </div>
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <h2 className="truncate font-semibold">{category.name}</h2>
                      <Badge variant="secondary" className="tabular-nums">
                        {category.recipeCount} công thức
                      </Badge>
                    </div>
                    <p className="mt-0.5 truncate text-sm text-muted-foreground">
                      /{category.slug}
                      {category.description ? ` — ${category.description}` : ""}
                    </p>
                  </div>
                </div>

                <div className="flex shrink-0 items-center gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => openEditDialog(category)}
                  >
                    <Pencil />
                    Sửa
                  </Button>
                  <Button
                    type="button"
                    variant="destructive"
                    size="sm"
                    onClick={() => setPendingDelete(category)}
                  >
                    <Trash2 />
                    Xóa
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <DialogHeader>
              <DialogTitle>
                {editing ? "Chỉnh sửa danh mục" : "Thêm danh mục mới"}
              </DialogTitle>
              <DialogDescription>
                Slug được hệ thống tự sinh từ tên danh mục.
              </DialogDescription>
            </DialogHeader>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="category-name" className="text-sm font-medium">
                Tên danh mục
              </label>
              <Input
                id="category-name"
                value={form.name}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    name: event.target.value,
                  }))
                }
                minLength={2}
                maxLength={50}
                required
                autoFocus
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label
                htmlFor="category-description"
                className="text-sm font-medium"
              >
                Mô tả
              </label>
              <Textarea
                id="category-description"
                value={form.description}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    description: event.target.value,
                  }))
                }
                rows={3}
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label
                htmlFor="category-image"
                className="text-sm font-medium"
              >
                URL ảnh đại diện
              </label>
              <Input
                id="category-image"
                type="url"
                value={form.imageUrl}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    imageUrl: event.target.value,
                  }))
                }
                placeholder="https://…"
              />
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => setDialogOpen(false)}
                disabled={saving}
              >
                Hủy
              </Button>
              <Button type="submit" disabled={saving}>
                {saving ? "Đang lưu…" : editing ? "Lưu thay đổi" : "Tạo danh mục"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <ConfirmDialog
        open={pendingDelete !== null}
        onOpenChange={(open) => {
          if (!open) setPendingDelete(null);
        }}
        title="Xóa danh mục?"
        description={
          pendingDelete
            ? `Danh mục “${pendingDelete.name}” sẽ bị xóa vĩnh viễn. Chỉ có thể xóa khi danh mục không còn công thức nào.`
            : undefined
        }
        confirmLabel="Xóa danh mục"
        destructive
        loading={deleting}
        onConfirm={handleDelete}
      />
    </>
  );
}

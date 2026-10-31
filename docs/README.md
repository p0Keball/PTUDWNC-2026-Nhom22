# Tài liệu kỹ thuật — Phần Trần Gia Huy (Tuần 2 → Tuần 5)

Thư mục này là bàn giao (handover) cho nhóm về mảng **Child Data & File Storage**:
RecipeStep, RecipeIngredient, RecipeImage, MinIO, thumbnail job, orphan cleanup,
upload optimization và bộ test tích hợp.

| Tài liệu | Nội dung |
|---|---|
| [child-data.md](child-data.md) | API + validation + quy tắc renumber của Step / Ingredient / Image; component FE |
| [file-storage.md](file-storage.md) | Kiến trúc MinIO, `IFileStorageService`, presigned upload, thumbnail, cleanup, orphan scan, graceful fallback |
| [testing.md](testing.md) | Cách chạy test, phạm vi coverage, test MinIO thật bằng Docker |
| [handover-file-storage.md](handover-file-storage.md) | Checklist bàn giao File Storage Service (chạy, cấu hình, verify, lỗi thường gặp) |
| [handover-ui-components.md](handover-ui-components.md) | Cách dùng lại `ImageUploader`, `RecipeGallery`, `IngredientForm`, `StepForm`, `compress.ts` |

## Bản đồ mã nguồn

```
src/FoodBlog.Domain/Entities/
  RecipeStep.cs, RecipeIngredient.cs, RecipeImage.cs

src/FoodBlog.Application/
  Common/ImageValidation.cs        # MIME + magic bytes + 5MB + object key
  Common/ImageVariants.cs          # quy ước key medium/thumb
  Interfaces/IFileStorageService.cs
  Interfaces/IImageJobQueue.cs
  Interfaces/IImageResizer.cs
  Options/MinioOptions.cs
  Options/OrphanCleanupOptions.cs

src/FoodBlog.Infrastructure/Services/
  MinioFileStorageService.cs       # MinIO SDK, presign, delete variants, health, list
  ImageSharpResizer.cs             # resize 800x600 + 300x300
  ImageJobWorker.cs                # queue + worker thumbnail/cleanup (retry 3 lần)
  OrphanCleanupService.cs          # scan + cleanup file mồ côi

src/FoodBlog.API/Endpoints/
  RecipeChildEndpoints.cs          # steps / ingredients / images / presign / confirm
  RecipeChildValidators.cs         # FluentValidation cho child data
  StorageEndpoints.cs              # /storage/health, /admin/storage/orphans, /recipes/{id}/validation

frontend/components/
  ImageUploader.tsx, RecipeGallery.tsx, IngredientForm.tsx, StepForm.tsx
frontend/lib/compress.ts

tests/FoodBlog.Tests/
  Infrastructure/                  # ApiTestBase, FoodBlogApiFactory (InMemory DB), fake storage/queue, test data
  ChildData/                       # integration test HTTP cho steps/ingredients/images
  Storage/                         # unit + integration test file storage/thumbnail/orphan
```

## Bổ sung Tuần 5 — Quản trị & nhật ký

| Tài liệu | Nội dung |
|---|---|
| [admin-and-logging.md](admin-and-logging.md) | Trạng thái công thức, upload ảnh có fallback, quản trị người dùng (Admin/Author), audit log + Serilog |

Mã nguồn liên quan:

```
src/FoodBlog.API/Endpoints/AdminEndpoints.cs       # /admin/users, /admin/audit-logs
src/FoodBlog.API/Middlewares/CorrelationIdMiddleware.cs
src/FoodBlog.Application/Common/Interfaces/IAuditLogger.cs
src/FoodBlog.Application/Common/Behaviors/LoggingBehavior.cs
src/FoodBlog.Infrastructure/Services/AuditLogger.cs
src/FoodBlog.Domain/Entities/AuditLog.cs
src/FoodBlog.Application/Features/Recipes/Commands/UnarchiveRecipeCommand.cs

frontend/app/admin/users/page.tsx
frontend/app/admin/logs/page.tsx
frontend/components/AdminAuthGate.tsx
frontend/components/AdminNav.tsx
```

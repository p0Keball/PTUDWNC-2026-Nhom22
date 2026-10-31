# Testing — Child Data & File Storage

## 1. Chạy test

```bash
# restore + build + test toàn bộ
dotnet test tests/FoodBlog.Tests/FoodBlog.Tests.csproj

# chỉ test không cần hạ tầng (bỏ test MinIO thật)
dotnet test tests/FoodBlog.Tests/FoodBlog.Tests.csproj --filter "Category!=RequiresMinIO"

# chỉ test MinIO thật (cần docker compose up -d minio)
dotnet test tests/FoodBlog.Tests/FoodBlog.Tests.csproj --filter "Category=RequiresMinIO"

# kèm coverage (coverlet)
dotnet test tests/FoodBlog.Tests/FoodBlog.Tests.csproj --collect:"XPlat Code Coverage"
```

Kết quả hiện tại: **67 test pass** trên máy không cần PostgreSQL/MinIO.

## 2. Test project

`tests/FoodBlog.Tests/FoodBlog.Tests.csproj` — xUnit + `WebApplicationFactory<Program>`.

Hạ tầng được thay thế khi `Environment = "Testing"`:

| Thành phần thật | Trong test |
|---|---|
| PostgreSQL + EF Npgsql | EF Core **InMemory** (`FoodBlogApiFactory`) |
| MinIO | `InMemoryFileStorage` (cùng `IFileStorageService` contract) |
| `ImageJobQueue` + worker nền | `RecordingImageJobQueue` (chỉ ghi nhận job) |
| Migrate + seed | bỏ qua (`Program.cs` check `IsEnvironment("Testing")`) |

Nhờ đó integration test chạy được trên máy sạch, không phụ thuộc Docker.

## 3. Phạm vi coverage

### 3.1 Child Data — Tuần 5

| Yêu cầu | Test |
|---|---|
| Integration Test Steps | `ChildData/StepEndpointsTests` (create, update, delete renumber, reorder, validate) |
| Integration Test Ingredients | `ChildData/IngredientEndpointsTests` (CRUD, trùng tên, renumber, reorder, validate) |
| Integration Test Image Upload | `ChildData/ImageEndpointsTests` (multipart, MIME, magic bytes, 5MB, primary, delete variants, presign, confirm) |

Các test gọi **HTTP thật** qua `HttpClient` của `WebApplicationFactory`, đi qua middleware,
model binding, FluentValidation và EF Core — không mock handler.

### 3.2 File Storage — Tuần 5

| Yêu cầu | Test |
|---|---|
| Test MinIO | `Storage/MinioIntegrationTests` (upload/read/presign/delete với MinIO thật, tự skip nếu không có MinIO) |
| Test Thumbnail Job | `Storage/ImageJobWorkerTests` (worker thật + ImageSharp thật + storage giả; assert 800×600 và 300×300, URL DB, ảnh bị xóa trước job) |
| Validate ảnh | `Storage/ImageValidationTests` (MIME, extension, magic bytes JPEG/PNG/WebP/AVIF, 5MB, object key) |
| Quy ước biến thể | `Storage/ImageVariantsTests` |
| Resize | `Storage/ImageSharpResizerTests` (kích thước, JPEG magic bytes, input lỗi) |
| MinIO service | `Storage/MinioFileStorageServiceTests` (parse key, chặn path traversal, public URL, graceful fallback) |
| Orphaned files | `Storage/OrphanCleanupTests` (dry-run, xóa orphan, giữ file còn tham chiếu và file mới) |

### 3.3 Bảng đối chiếu FR

| FR | Nội dung | Nơi kiểm tra |
|---|---|---|
| FR-RCP-009 | Add/Update/Delete Ingredient | `IngredientEndpointsTests` |
| FR-RCP-010 | Add/Update/Delete Step + renumber | `StepEndpointsTests` |
| FR-RCP-008 | Set primary / delete image | `ImageEndpointsTests` |
| FR-FILE-001 | Upload ảnh, MIME, magic bytes, 5MB | `ImageValidationTests`, `ImageEndpointsTests` |
| FR-FILE-002 | Retry/idempotent delete | `MinioFileStorageServiceTests` |
| FR-JOB-002 | Thumbnail 300×300 & 800×600 | `ImageSharpResizerTests`, `ImageJobWorkerTests` |
| Tuần 4 — Orphaned files | Detect + cleanup | `OrphanCleanupTests` |

## 4. Test MinIO thật

Yêu cầu:

```bash
docker compose up -d minio
# MinIO API: http://localhost:9000, console: http://localhost:9001
```

Biến môi trường tùy chọn (mặc định khớp `docker-compose.yml`):

```bash
FOODBLOG_MINIO_ENDPOINT=localhost:9000
FOODBLOG_MINIO_ACCESSKEY=minioadmin
FOODBLOG_MINIO_SECRETKEY=minioadmin123
```

Test tạo bucket riêng `foodblog-tests-{guid}` để không ảnh hưởng dữ liệu thật, sau đó:

1. `EnsureBucketExistsAsync`
2. `UploadStreamAsync` ảnh JPEG
3. `GetMetadataAsync` → size > 0, content-type `image/jpeg`
4. `ReadLeadingBytesAsync` → `FF D8 FF E0`
5. `GetPresignedPutUrlAsync` → URL có `X-Amz-Signature`
6. `DeleteImageVariantsAsync` → `GetMetadataAsync` trả `null`

Nếu MinIO không chạy, test ghi log và bỏ qua (không làm đỏ suite).

## 5. Lưu ý khi viết test mới

- Dùng `ApiTestBase` cho integration test HTTP: mỗi test có factory + DB InMemory riêng.
- Seed dữ liệu qua `TestData.SeedRecipeAsync`, đọc lại qua `TestData.QueryAsync`
  (mở scope mới để tránh change tracker).
- Không assert thứ tự không xác định: luôn `OrderBy` trước khi so sánh.
- Với test cần MinIO thật, gắn `[Trait("Category", "RequiresMinIO")]`.
- Không commit file trong `TestResults/` (đã gitignore).

## 6. Frontend

```bash
cd frontend
npm run lint
npm run build
```

Các component Tuần 2–4 được kiểm tra gián tiếp qua API contract; khi có thời gian có thể
bổ sung Playwright cho luồng upload/gallery.

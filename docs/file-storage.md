# File Storage Service (MinIO)

Phạm vi: **FR-FILE-001/002**, **FR-JOB-002**, **FR-RCP-008**, cùng các hạng mục Tuần 4:
MinIO reliability, orphaned files, upload optimization.

## 1. Kiến trúc

```
Browser ──(A) multipart ─────────────► API ──► MinioFileStorageService ──► MinIO
   │                                     │
   └─(B) presign ─► API ─► presigned PUT │
          │                              │
          └── PUT trực tiếp ────────────►│
                                         └─► ImageJobQueue ─► ImageJobWorker
                                                                 ├─ resize 800×600 + 300×300 (ImageSharp)
                                                                 └─ upload medium/thumb + cập nhật DB
```

- **MinIO** là S3-compatible object storage, bucket mặc định `culinary-blog`.
- **Backend không proxy file** khi dùng presigned URL: FE upload trực tiếp, giảm tải API.
- **Thumbnail** chạy nền, không chặn response tạo ảnh. Ảnh gốc vẫn hiển thị ngay.
- **Cleanup** ảnh cũng chạy nền; request xóa không fail nếu MinIO đang lỗi.

## 2. Cấu hình

`src/FoodBlog.API/appsettings.json`:

```json
{
  "MinIO": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin123",
    "Bucket": "culinary-blog",
    "PublicUrl": "http://localhost:9000",
    "UseSsl": false
  },
  "OrphanCleanup": {
    "Enabled": true,
    "IntervalHours": 24,
    "MinAgeHours": 24,
    "Prefix": "recipes/"
  }
}
```

| Key | Ý nghĩa |
|---|---|
| `Endpoint` | host:port của MinIO API (không gồm scheme) |
| `Bucket` | bucket chứa ảnh; service tự tạo nếu chưa có |
| `PublicUrl` | base URL trả cho FE (`{PublicUrl}/{Bucket}/{objectKey}`) |
| `UseSsl` | bật khi MinIO sau TLS |
| `OrphanCleanup:Enabled` | bật/tắt worker quét định kỳ |
| `IntervalHours` | chu kỳ quét (giờ) |
| `MinAgeHours` | chỉ coi là orphan khi object cũ hơn N giờ (tránh xóa file presigned vừa upload chưa confirm) |
| `Prefix` | prefix quét, mặc định `recipes/` |

## 3. `IFileStorageService`

| Method | Mục đích |
|---|---|
| `UploadAsync(IFormFile, folder)` | upload multipart, trả public URL |
| `UploadStreamAsync(stream, key, contentType)` | upload stream (thumbnail job) |
| `DownloadAsync(key)` | tải object về `MemoryStream` (< 5MB) để resize |
| `GetMetadataAsync(key)` | size + content-type; `null` nếu không tồn tại |
| `ReadLeadingBytesAsync(key, n)` | đọc N byte đầu để check magic bytes |
| `GetPresignedPutUrlAsync` | tạo presigned PUT (mặc định 5 phút) |
| `GetPublicUrl(key)` | ghép public URL |
| `TryExtractKey(url)` | parse object key từ URL; chặn `..` |
| `DeleteByKeyAsync(key)` | xóa idempotent, retry 3 lần với backoff |
| `DeleteAsync(url)` | xóa theo URL |
| `DeleteImageVariantsAsync(url)` | xóa gốc + medium + thumb, best-effort từng key |
| `EnsureBucketExistsAsync` | tạo bucket + public-read policy, cache kết quả |
| `IsHealthyAsync` | health probe 5s, không ném exception |
| `ListKeysAsync(prefix)` | liệt kê object phân trang cho orphan scan |

## 4. Quy ước object key và biến thể

```
recipes/{recipeId}/{guid}.jpg          # gốc
recipes/{recipeId}/{guid}_medium.jpg   # 800×600 (crop)
recipes/{recipeId}/{guid}_thumb.jpg    # 300×300 (crop)
```

Quy ước nằm trong `FoodBlog.Application.Common.ImageVariants`:

```csharp
ImageVariants.MediumKey(originalKey);   // ..._medium.jpg
ImageVariants.ThumbKey(originalKey);    // ..._thumb.jpg
ImageVariants.AllKeys(originalKey);     // gốc, medium, thumb
```

Nhờ quy ước này, khi DB chỉ lưu `OriginalUrl` vẫn suy ra được các biến thể để xóa/cleanup.

## 5. Luồng upload

### 5.1 Multipart qua backend

1. `RecipeChildEndpoints` kiểm tra recipe tồn tại + quyền sở hữu.
2. `ValidateUploadMeta`: MIME, extension, `0 < size <= 5MB`.
3. Đọc 12 byte đầu → `ImageValidation.HasValidMagicBytes`.
4. `storage.UploadAsync(file, $"recipes/{id}")` → public URL.
5. Ghi `RecipeImage` (ảnh đầu tiên `IsPrimary = true`).
6. `jobs.EnqueueThumbnail(image.Id)` → trả `201` ngay.

Lỗi MinIO ở bước 4 → `503 STORAGE_UNAVAILABLE` (không phải `500`).

### 5.2 Presigned URL (khuyến nghị cho file lớn)

1. FE gọi `/images/presign` → BE validate metadata rồi ký URL PUT 5 phút.
2. FE `PUT` file trực tiếp lên MinIO.
3. FE gọi `/images/confirm` → BE kiểm tra object thật:
   - `GetMetadataAsync` (size, content-type)
   - `ReadLeadingBytesAsync` (magic bytes)
   - nếu sai: xóa object vừa upload rồi trả lỗi
4. Ghi DB + enqueue thumbnail.

Object không tồn tại → `422`; MinIO lỗi → `503`.

## 6. Thumbnail job — FR-JOB-002

`ImageJobQueue` dùng `Channel<ImageJob>` in-process; `ImageJobWorker` là `BackgroundService`.

```
EnqueueThumbnail(imageId) ─► worker ─► load image ─► DownloadAsync(originalKey)
        │                              │
        │                              ├─ ImageSharpResizer.ResizeAsync
        │                              │     ├─ medium 800×600, JPEG q=80
        │                              │     └─ thumb  300×300, JPEG q=80
        │                              ├─ UploadStreamAsync(mediumKey/thumbKey)
        │                              └─ image.SetThumbnails(...) + SaveChanges
        └─ retry tối đa 3 lần (5s, 10s)
```

Tính chất:
- **Idempotent**: nếu `MediumUrl` và `ThumbnailUrl` đã có thì bỏ qua.
- **An toàn khi ảnh bị xóa trước khi job chạy**: không tìm thấy row → return.
- **Ảnh không decode được** (AVIF trên ImageSharp 3.1.8): log warning, giữ trạng thái
  `pending`, không retry vô ích. Ảnh gốc vẫn dùng được.
- Response ảnh có `thumbnailStatus = "pending" | "ready"`.

> Khi nhóm chuyển sang Hangfire, chỉ cần thay implementation của `IImageJobQueue`,
> không đổi endpoint/worker contract.

## 7. Xóa ảnh và cleanup

### Xóa một ảnh
`DELETE /api/v1/recipes/{id}/images/{imageId}`:
1. Xóa row DB.
2. `DeleteImageVariantsAsync(originalUrl)` — xóa gốc + medium + thumb, best-effort.
3. Nếu ảnh bị xóa là primary → ảnh kế tiếp được promote.
4. `RenumberImagesAsync` đánh lại `OrderIndex`.

### Xóa recipe
`DeleteRecipeCommand` (soft-delete) gom toàn bộ `OriginalUrl` rồi
`jobs.EnqueueCleanup(urls)`; worker xóa đệ quy các biến thể.

### Orphaned files — Tuần 4

Orphan = object trên MinIO không còn row DB nào trỏ tới (presign bị hủy, confirm fail,
xóa tay DB...).

`OrphanCleanupRunner.ScanAsync`:
1. Thu thập key được DB tham chiếu: `RecipeImages.OriginalUrl`, `MediumUrl`, `ThumbnailUrl`,
   `RecipeSteps.ImageUrl`; với mỗi URL thêm cả 3 biến thể.
2. `ListKeysAsync(prefix)` lấy toàn bộ object.
3. Diff; bỏ qua object mới hơn `MinAgeHours`.
4. `dryRun = true` chỉ báo cáo; `false` xóa từng key (lỗi key nào log key đó).

Endpoint admin (đang mở, cần gắn `RequireAuthorization("Admin")` khi JWT hoàn thiện):

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/v1/admin/storage/orphans?prefix=&minAgeHours=` | dry-run, trả report |
| POST | `/api/v1/admin/storage/orphans/cleanup` | xóa thật |
| GET | `/api/v1/storage/health` | `200 healthy` / `503 unhealthy` cho banner FE |

`OrphanCleanupWorker` chạy lần đầu sau 2 phút khởi động, sau đó mỗi `IntervalHours`.

## 8. MinIO reliability — Tuần 4

| Tình huống | Hành vi |
|---|---|
| MinIO down khi upload multipart | `503 STORAGE_UNAVAILABLE`, DB không đổi |
| MinIO down khi confirm presign | `503 STORAGE_UNAVAILABLE` |
| MinIO down khi xóa ảnh | request vẫn `204`; cleanup best-effort, worker retry 3 lần |
| MinIO down khi `/storage/health` | `503 { status: "unhealthy" }` trong ≤ 5s |
| MinIO down khi orphan scan | `ListKeysAsync` dừng enumeration; report `Error = STORAGE_UNAVAILABLE` |
| Bucket chưa tồn tại | tự tạo ở lần upload đầu; cache để không check lại mỗi request |
| Bucket check lỗi | reset cache để lần sau thử lại |

FE (`ImageUploader`) poll `/storage/health` mỗi 30 giây, disable nút upload và hiện banner
khi storage down; ảnh đã có vẫn hiển thị qua fallback/cache.

## 9. Upload optimization — Tuần 4

- **Client-side compression** (`frontend/lib/compress.ts`):
  - file < 300KB: bỏ qua;
  - AVIF: bỏ qua (canvas không encode được ổn định);
  - cạnh dài > 1920px: resize;
  - JPEG q=0.82; PNG giữ PNG để không mất transparency;
  - nếu file sau nén không nhỏ hơn file gốc → dùng file gốc.
- **Drag & drop nhiều file** (tối đa 5 file/lần), upload song song, mỗi file một progress,
  một file lỗi không ảnh hưởng file khác.
- **Hủy upload**: `AbortController` cho từng file + nút "Hủy tất cả".
- **Bucket check cache**: `Interlocked` đảm bảo chỉ check bucket một lần.

## 10. Test

Xem [testing.md](testing.md). Tóm tắt:

```bash
# toàn bộ test (không cần Docker)
dotnet test tests/FoodBlog.Tests/FoodBlog.Tests.csproj

# test MinIO thật (sau docker compose up -d minio)
dotnet test tests/FoodBlog.Tests/FoodBlog.Tests.csproj --filter Category=RequiresMinIO
```

## 11. Lỗi thường gặp

| Hiện tượng | Nguyên nhân / cách xử lý |
|---|---|
| `503 STORAGE_UNAVAILABLE` khi upload | MinIO chưa chạy: `docker compose up -d minio`, kiểm tra `http://localhost:9000/minio/health/live` |
| Ảnh upload xong nhưng không có thumbnail | Xem log `ImageJobWorker`; AVIF có thể không decode được với ImageSharp 3.1.8 |
| Ảnh 404 trên browser | `MinIO:PublicUrl`/`Endpoint` sai hoặc bucket policy chưa public-read; kiểm tra URL trong DB |
| Orphan scan báo 503 | MinIO disconnect; kiểm tra container rồi chạy lại dry-run |
| Orphan cleanup xóa nhầm file mới | Tăng `MinAgeHours`; presigned URL có hạn 5 phút nên 24h là an toàn |
| `FILE_MIME_INVALID` dù đúng đuôi | Nội dung file không khớp MIME (magic bytes); thử mở file bằng trình xem ảnh |

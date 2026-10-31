# Handover — File Storage Service

> Người bàn giao: Trần Gia Huy (2312637) — mảng Child Data & File Storage.
> Tài liệu chi tiết: [file-storage.md](file-storage.md), API: [child-data.md](child-data.md).

## 1. Checklist bàn giao

- [x] `IFileStorageService` + implementation MinIO (`MinioFileStorageService`).
- [x] Upload multipart, presigned URL, confirm, validate MIME/magic bytes/5MB.
- [x] Biến thể ảnh 800×600 + 300×300, worker retry 3 lần.
- [x] Xóa đệ quy gốc/medium/thumb khi xóa ảnh hoặc recipe.
- [x] Health probe + graceful fallback (503 thay vì 500).
- [x] Orphan scan dry-run + cleanup định kỳ.
- [x] Test tự động cho toàn bộ luồng trên.
- [x] Tài liệu cấu hình, vận hành, troubleshooting.

## 2. Chạy thử trong 5 phút

```bash
# 1. Hạ tầng
docker compose up -d postgres minio
docker compose ps

# 2. Backend (tự migrate + seed lần đầu)
dotnet run --project src/FoodBlog.API --urls "http://localhost:5000"

# 3. Kiểm tra storage
curl http://localhost:5000/api/v1/storage/health
# {"status":"healthy","checkedAt":"..."}

# 4. Swagger/Scalar
# http://localhost:5000/scalar/
```

MinIO console: <http://localhost:9001> — `minioadmin` / `minioadmin123`.

## 3. Verify nhanh bằng API

Cần một recipeId có thật (lấy từ `GET /api/v1/recipes`) và một file ảnh < 5MB.

```bash
# Upload multipart
curl -X POST "http://localhost:5000/api/v1/recipes/{recipeId}/images?altText=cover" \
  -F "file=@./photo.jpg;type=image/jpeg"

# Đặt ảnh chính
curl -X PATCH "http://localhost:5000/api/v1/recipes/{recipeId}/images/{imageId}/primary"

# Xóa ảnh (DB + MinIO)
curl -X DELETE "http://localhost:5000/api/v1/recipes/{recipeId}/images/{imageId}"

# Presigned flow
curl -X POST "http://localhost:5000/api/v1/recipes/{recipeId}/images/presign" \
  -H "Content-Type: application/json" \
  -d '{"fileName":"cover.jpg","contentType":"image/jpeg","fileSize":1048576}'

curl -X POST "http://localhost:5000/api/v1/recipes/{recipeId}/images/confirm" \
  -H "Content-Type: application/json" \
  -d '{"objectKey":"recipes/{recipeId}/xxx.jpg"}'

# Orphan scan (dry-run) và cleanup
curl "http://localhost:5000/api/v1/admin/storage/orphans?minAgeHours=24"
curl -X POST "http://localhost:5000/api/v1/admin/storage/orphans/cleanup"
```

Kỳ vọng:
- Upload ảnh đầu tiên: `isPrimary = true`, `thumbnailStatus = "pending"`.
- Sau vài giây: `thumbnailStatus = "ready"`, DB có `mediumUrl` + `thumbnailUrl`.
- MinIO console thấy 3 object: gốc, `_medium`, `_thumb`.
- Xóa ảnh: cả 3 object biến mất.
- Upload file `.txt` đổi tên `.png` → `400 FILE_MIME_INVALID`.
- Upload file > 5MB → `400 FILE_SIZE_EXCEEDED`.

## 4. Cấu hình production cần đổi

| Key | Dev | Production |
|---|---|---|
| `MinIO:PublicUrl` | `http://localhost:9000` | domain/CDN public của bucket |
| `MinIO:UseSsl` | `false` | `true` |
| `MinIO:AccessKey/SecretKey` | minioadmin | secret manager, **không commit** |
| `OrphanCleanup:IntervalHours` | 24 | tùy lưu lượng, thường 24 |
| `OrphanCleanup:MinAgeHours` | 24 | ≥ thời gian sống presigned URL (5 phút) |

## 5. Điểm cần lưu ý / việc tiếp theo

1. **Phân quyền endpoint admin storage**: hiện `/api/v1/admin/storage/orphans*` chưa gắn
   `RequireAuthorization`. Khi JWT của Long hoàn thiện, thêm policy `Admin`:
   ```csharp
   app.MapGet("/api/v1/admin/storage/orphans", ...).RequireAuthorization("Admin");
   ```
2. **Soft-delete recipe**: ảnh của recipe bị xóa mềm vẫn còn trên MinIO cho tới khi job
   cleanup chạy. Nếu cần giải phóng ngay, enqueue cleanup đồng bộ trong `DeleteRecipeCommand`
   (hiện đã enqueue).
3. **Hangfire**: `IImageJobQueue` là seam để thay queue in-process bằng Hangfire khi cần
   job bền vững qua restart. Không đổi endpoint.
4. **AVIF**: ImageSharp 3.1.8 không decode được AVIF → thumbnail giữ `pending`. Nếu bắt buộc
   hỗ trợ, thêm `SixLabors.ImageSharp.Formats.Avif` hoặc convert ở client.
5. **Bucket policy**: service set public-read best-effort. Nếu MinIO chặn policy, cấu hình
   bucket public bằng console hoặc dùng presigned GET.
6. **Nâng cấp ImageSharp**: phiên bản 3.1.8 có cảnh báo bảo mật (NU1902/NU1903). Cân nhắc
   nâng lên bản mới nhất khi rảnh; API sử dụng không đổi.

## 6. Liên hệ khi lỗi

| Triệu chứng | Kiểm tra trước |
|---|---|
| Không upload được | `docker compose ps`, `/api/v1/storage/health`, log API |
| Không có thumbnail | log `ImageJobWorker`, định dạng ảnh (AVIF), quyền ghi bucket |
| Ảnh 404 | `MinIO:PublicUrl` + `Bucket`, object có tồn tại trong console |
| Orphan cleanup báo lỗi | MinIO disconnect, `OrphanCleanup:Enabled`, log `OrphanCleanupRunner` |

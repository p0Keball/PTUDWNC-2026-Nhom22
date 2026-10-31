# Child Data API — Step, Ingredient, Image

Phạm vi: **FR-RCP-008** (ảnh), **FR-RCP-009** (nguyên liệu), **FR-RCP-010** (bước thực hiện),
**FR-JOB-002** (thumbnail) và **FR-FILE** (validate file).

Tất cả endpoint nằm dưới `/api/v1/recipes/{recipeId}` và trả lỗi theo RFC 7807
(`application/problem+json`) qua `GlobalExceptionMiddleware`, riêng validate child-data
trả `422` với `{ title, errors: { field: [message] } }`.

## 1. Entity

### RecipeStep
| Field | Kiểu | Ghi chú |
|---|---|---|
| `Id` | Guid | khóa chính |
| `RecipeId` | Guid | FK → `Recipes.Id`, cascade delete |
| `StepNumber` | int | 1-based, **luôn liên tục** sau create/delete/reorder |
| `Title` | string(200) | bắt buộc, 3–200 ký tự |
| `Description` | text | tối đa 5000 ký tự |
| `TimerMinutes` | int? | `>= 0` |
| `ImageUrl` | string(500)? | URL ảnh minh họa (không bắt buộc) |

### RecipeIngredient
| Field | Kiểu | Ghi chú |
|---|---|---|
| `Id` | Guid | khóa chính |
| `RecipeId` | Guid | FK → `Recipes.Id`, cascade delete |
| `Name` | string(200) | bắt buộc, 2–200 ký tự, **không trùng trong cùng recipe** (không phân biệt hoa/thường) |
| `Quantity` | decimal? | `> 0` nếu có |
| `Unit` | string(50)? | |
| `Notes` | string(500)? | |
| `OrderIndex` | int | 0-based, liên tục sau delete/reorder |

### RecipeImage
| Field | Kiểu | Ghi chú |
|---|---|---|
| `Id` | Guid | khóa chính |
| `RecipeId` | Guid | FK → `Recipes.Id`, cascade delete |
| `OriginalUrl` | string(500) | URL trên MinIO |
| `MediumUrl` | string(500)? | 800×600, do thumbnail job điền |
| `ThumbnailUrl` | string(500)? | 300×300, do thumbnail job điền |
| `AltText` | string(200)? | |
| `IsPrimary` | bool | ảnh đầu tiên tự động là ảnh chính |
| `OrderIndex` | int | 0-based |

## 2. Endpoints

### 2.1 Steps — FR-RCP-010

| Method | Endpoint | Thành công | Lỗi |
|---|---|---|---|
| POST | `/steps` | `201` + `RecipeStepDto` | `404 RECIPE_NOT_FOUND`, `422 VALIDATION_ERROR` |
| PUT | `/steps/{stepId}` | `200` + `RecipeStepDto` | `404` (step/recipe), `422` |
| DELETE | `/steps/{stepId}` | `204` | `404` |
| PATCH | `/steps/reorder` | `200` + danh sách step mới | `404`, `422` (danh sách không khớp) |

Body tạo/cập nhật:

```json
{
  "title": "Nấu nước dùng",
  "description": "Hầm xương 2 giờ với gừng nướng.",
  "timerMinutes": 120,
  "imageUrl": null
}
```

**Quy tắc đánh số**
- Tạo mới: `StepNumber = số step hiện có + 1`.
- Xóa một step bất kỳ: các step còn lại được sắp theo `StepNumber` rồi đánh lại `1..n`
  (`RenumberStepsAsync`).
- Reorder: body là **toàn bộ** step hiện có, mỗi phần tử `{ id, order }` (order 1-based).
  Thiếu/thừa id → `422`.

### 2.2 Ingredients — FR-RCP-009

| Method | Endpoint | Thành công | Lỗi |
|---|---|---|---|
| POST | `/ingredients` | `201` + `RecipeIngredientDto` | `404`, `422` (validate hoặc trùng tên) |
| PUT | `/ingredients/{ingredientId}` | `200` | `404`, `422` |
| DELETE | `/ingredients/{ingredientId}` | `204` | `404` |
| PATCH | `/ingredients/reorder` | `200` | `404`, `422` |

Body:

```json
{
  "name": "Xương bò",
  "quantity": 1.5,
  "unit": "kg",
  "notes": "Chọn xương ống để nước ngọt"
}
```

**Trùng tên**: so sánh `Trim().ToLowerInvariant()` trong cùng recipe. Khi tạo hoặc đổi tên
trùng, trả `422`:

```json
{
  "title": "Nguyên liệu đã tồn tại trong công thức.",
  "errors": { "name": ["\"Xương bò\" đã có trong danh sách."] }
}
```

**OrderIndex**: tạo mới = số nguyên liệu hiện có; xóa → đánh lại `0..n-1`; reorder dùng
`order` 0-based.

### 2.3 Images — FR-RCP-008 / FR-FILE / FR-JOB-002

#### Cách A — upload trực tiếp qua backend (multipart)

```
POST /api/v1/recipes/{recipeId}/images?altText=Ảnh%20bìa
Content-Type: multipart/form-data
file=<binary>
```

- Field name bắt buộc: `file`.
- `altText` là query string (tùy chọn).
- Thành công: `201` + `RecipeImageDto`, ảnh đầu tiên `isPrimary = true`.
- Ảnh được lưu vào MinIO qua `IFileStorageService.UploadAsync` (folder `recipes/{recipeId}`),
  sau đó enqueue job thumbnail.

#### Cách B — presigned URL (FE upload trực tiếp lên MinIO)

1. `POST /images/presign`

```json
{ "fileName": "cover.jpg", "contentType": "image/jpeg", "fileSize": 1048576 }
```

→ `200`:

```json
{
  "uploadUrl": "http://localhost:9000/...&X-Amz-Signature=...",
  "objectKey": "recipes/{recipeId}/{guid}.jpg",
  "publicUrl": "http://localhost:9000/culinary-blog/recipes/...",
  "expiresAt": "2026-10-21T10:05:00Z"
}
```

2. FE `PUT` file lên `uploadUrl` với đúng `Content-Type`.
3. `POST /images/confirm`

```json
{ "objectKey": "recipes/{recipeId}/{guid}.jpg", "altText": "Ảnh bìa" }
```

Backend kiểm tra object thật trên MinIO (size, MIME, magic bytes) rồi mới ghi DB và enqueue
thumbnail. Object không tồn tại → `422`; MinIO lỗi → `503 STORAGE_UNAVAILABLE`.

#### Cách C — thêm ảnh từ URL có sẵn

`POST /images/by-url` với `{ "originalUrl", "altText"? }` (dùng cho seed/import).

#### Quản lý ảnh

| Method | Endpoint | Ghi chú |
|---|---|---|
| PATCH | `/images/{imageId}/primary` | đặt ảnh chính; các ảnh khác tự `false` |
| DELETE | `/images/{imageId}` | xóa DB + **xóa đệ quy** gốc/medium/thumb trên MinIO; nếu xóa ảnh chính thì ảnh kế tiếp được promote |

#### Validate file (áp dụng cho cả 3 cách)

| Quy tắc | Giá trị | Mã lỗi |
|---|---|---|
| MIME cho phép | `image/jpeg`, `image/png`, `image/webp`, `image/avif` | `FILE_MIME_INVALID` |
| Phần mở rộng | `.jpg`, `.jpeg`, `.png`, `.webp`, `.avif` | `FILE_MIME_INVALID` |
| Magic bytes | JPEG `FF D8 FF`, PNG `89 50 4E 47`, WebP `RIFF....WEBP`, AVIF `ftypavif/avis` | `FILE_MIME_INVALID` |
| Dung lượng | `> 0` và `<= 5 MB` | `FILE_SIZE_EXCEEDED` |
| Object key confirm | không rỗng, không chứa `..` | `422` |

Lỗi validate file trả `400`:

```json
{
  "code": "FILE_MIME_INVALID",
  "title": "Định dạng tệp không được hỗ trợ (chỉ nhận JPG, PNG, WebP, AVIF).",
  "errors": { "file": ["File không phải ảnh hợp lệ (magic bytes không khớp MIME)."] }
}
```

> Magic bytes được kiểm tra **sau khi** đọc 12 byte đầu, nên đổi đuôi `.txt` → `.png`
> vẫn bị chặn.

## 3. Phân quyền

`ForbidIfNotOwner` / `CheckOwnerAsync` đảm bảo Author chỉ thao tác recipe của mình, Admin
được bypass, request ẩn danh hiện được cho qua để tương thích khi JWT chưa đấu nối.
Khi JWT hoàn thiện, endpoint sẽ trả `403 RECIPE_FORBIDDEN` cho user khác chủ.

## 4. Frontend components

| Component | Vai trò |
|---|---|
| `IngredientForm.tsx` | dynamic rows + react-hook-form + Zod, drag & drop (`@dnd-kit`), phát hiện trùng tên, nút Lưu/Xóa từng dòng |
| `StepForm.tsx` | dynamic steps, drag & drop đánh lại thứ tự, validate title 3–200, timer ≥ 0 |
| `ImageUploader.tsx` | drag & drop nhiều file (tối đa 5/lần), nén client-side, progress từng file, hủy upload, set primary, xóa, lightbox, banner khi MinIO down |
| `RecipeGallery.tsx` | gallery xem ảnh, ưu tiên thumb → medium → original, fallback SVG khi ảnh lỗi |
| `lib/compress.ts` | resize cạnh dài về 1920px, JPEG q=0.82, bỏ qua file < 300KB hoặc AVIF |

## 5. Test tương ứng

| Test | File |
|---|---|
| Step create/update/delete/renumber/reorder/validate | `tests/FoodBlog.Tests/ChildData/StepEndpointsTests.cs` |
| Ingredient CRUD/trùng tên/renumber/reorder/validate | `tests/FoodBlog.Tests/ChildData/IngredientEndpointsTests.cs` |
| Image upload MIME/magic bytes/5MB, primary, delete variants, presign, confirm | `tests/FoodBlog.Tests/ChildData/ImageEndpointsTests.cs` |
| Resize 800×600 + 300×300 | `tests/FoodBlog.Tests/Storage/ImageSharpResizerTests.cs` |
| Thumbnail worker end-to-end | `tests/FoodBlog.Tests/Storage/ImageJobWorkerTests.cs` |

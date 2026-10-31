# Handover — Reusable UI Components

Các component dưới đây thuộc mảng Child Data & File Storage, dùng được lại cho mọi form
recipe. Tất cả là client component (`"use client"`) của Next.js App Router.

## 1. `ImageUploader`

`frontend/components/ImageUploader.tsx`

```tsx
<ImageUploader
  recipeId={recipeId}          // Guid recipe đã lưu
  images={images}              // RecipeImage[]
  onChange={setImages}         // nhận mảng ảnh mới sau upload/xóa/primary
/>
```

Tính năng:
- Drag & drop hoặc chọn nhiều file (tối đa 5 file/lần).
- Validate phía client: JPEG/PNG/WebP/AVIF, ≤ 5MB.
- Nén client-side trước khi upload (`lib/compress.ts`).
- Progress riêng từng file + nút hủy tất cả.
- Upload song song; một file lỗi không chặn file khác.
- Set ảnh chính / xóa ảnh / lightbox.
- Poll `/api/v1/storage/health` mỗi 30 giây; hiện banner và disable upload khi MinIO down.
- Fallback ảnh SVG khi URL ảnh lỗi.

Lưu ý: `recipeId` phải có trước khi upload. Với trang tạo recipe, hãy tạo recipe Draft trước,
sau đó render uploader.

## 2. `RecipeGallery`

`frontend/components/RecipeGallery.tsx`

```tsx
<RecipeGallery images={recipe.images} />
```

- Ảnh lớn dùng `mediumUrl ?? originalUrl`, strip dùng `thumbnailUrl ?? mediumUrl ?? originalUrl`.
- Click ảnh để mở lightbox (ảnh gốc).
- Tự chọn ảnh primary làm ảnh đang xem.
- Export helper dùng lại:
  ```ts
  import { thumbSrc, previewSrc, FALLBACK_IMG } from "@/components/RecipeGallery";
  ```
- `FALLBACK_IMG` là data-URI SVG, dùng cho mọi `<img onError>`.

Component này **chỉ đọc**, không sửa ảnh. Muốn sửa dùng `ImageUploader`.

## 3. `IngredientForm`

`frontend/components/IngredientForm.tsx`

```tsx
<IngredientForm
  recipeId={recipeId}                 // null = chế độ soạn cục bộ (gửi cùng lúc tạo recipe)
  defaultIngredients={ingredients}
  onChange={(items) => setIngredients(items)}
  onPersisted={() => refreshValidation()}  // gọi sau mỗi lần lưu/xóa/reorder thành công
/>
```

- react-hook-form + Zod: tên 2–200, quantity > 0, unit ≤ 50, notes ≤ 500.
- Phát hiện trùng tên ngay trên UI (normalize trim + lowercase).
- Drag & drop đổi thứ tự → gọi `PATCH /ingredients/reorder`.
- Mỗi dòng có nút Lưu / Xóa khi đã có `recipeId`.
- Cảnh báo khi chưa có nguyên liệu.

## 4. `StepForm`

`frontend/components/StepForm.tsx`

```tsx
<StepForm
  recipeId={recipeId}
  defaultSteps={steps}
  onChange={(items) => setSteps(items)}
  onPersisted={() => refreshValidation()}
/>
```

- Validate: title 3–200, description ≤ 5000, timer ≥ 0, imageUrl ≤ 500.
- Drag & drop đánh lại `stepNumber` qua `PATCH /steps/reorder`.
- Phát hiện trùng tiêu đề.
- Chế độ `recipeId = null`: giữ dữ liệu cục bộ để gửi kèm khi tạo recipe.

## 5. `lib/compress.ts`

```ts
import { compressImage, formatBytes } from "@/lib/compress";

const { file, originalSize, compressedSize, wasCompressed } = await compressImage(rawFile);
```

- Bỏ qua file < 300KB và AVIF.
- Resize cạnh dài về 1920px.
- JPEG quality 0.82; PNG giữ PNG.
- Nếu kết quả không nhỏ hơn file gốc → trả file gốc.
- Không thêm dependency (dùng `createImageBitmap` + canvas).

## 6. Ví dụ tích hợp trong trang edit

```tsx
const [images, setImages] = useState(recipe.images);

<ImageUploader
  recipeId={recipe.id}
  images={images}
  onChange={(next) => {
    setImages(next);
    refreshValidation();
  }}
/>
```

Checklist publish có thể lấy từ:

```
GET /api/v1/recipes/{id}/validation
```

Response gồm `hasSteps`, `hasIngredients`, `hasCoverImage`, `canPublish`, `issues[]` — dùng
để hiển thị tick/báo lỗi trước khi bấm Publish.

## 7. Quy ước code

- Không dùng `<Image>` của Next cho URL MinIO động (domain thay đổi theo môi trường);
  dùng `<img>` + `eslint-disable @next/next/no-img-element` như hiện tại.
- Mọi lời gọi API đi qua `frontend/lib/api.ts`, không fetch trực tiếp trong component.
- Lỗi hiển thị bằng text đỏ trong component; có thể thay bằng toast khi hệ thống toast
  của Tuần 4 hoàn thiện.

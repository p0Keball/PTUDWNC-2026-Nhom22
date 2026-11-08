# Đáp án ôn tập cuối kỳ — Culinary Blog (Nhóm 22)

> Tài liệu trả lời toàn bộ câu hỏi trong `Cau_Hoi_On_Tap.pdf`, bám sát mã nguồn
> thực tế của dự án (`src/`, `frontend/`, `tests/`) và chỉ rõ chỗ mâu thuẫn
> trong `SRS_culinary_blog.md`.
>
> Quy ước: “SRS” = `SRS_culinary_blog.md`; số dòng trích dẫn theo file hiện tại.

---

# PHẦN I — LÝ THUYẾT

## Câu 1. Ưu/nhược điểm của Monolithic Architecture. Khi nào vẫn nên chọn?

**Ưu điểm**
- Đơn giản: một codebase, một pipeline build/deploy, một process.
- Gọi hàm trực tiếp giữa các module, không cần network → latency thấp, dễ debug.
- Transaction ACID trong cùng database dễ dàng (ví dụ tạo Recipe + Steps +
  Ingredients trong một `SaveChangesAsync`).
- Chi phí hạ tầng thấp, phù hợp nhóm nhỏ và thời gian làm đồ án ngắn.
- Triển khai/rollback đơn giản (một image Docker).

**Nhược điểm**
- Coupling theo thời gian: module phình to, khó tách biên.
- Scale theo chiều dọc hoặc nhân bản toàn bộ ứng dụng, không scale riêng phần
  nóng (ví dụ chỉ search).
- Một lỗi bộ nhớ/rò rỉ có thể làm sập toàn bộ hệ thống.
- Nhiều nhóm cùng sửa một repo dễ conflict, khó chia ownership.
- Công nghệ bị “khóa” theo một stack.

**Khi nào vẫn nên chọn Monolithic cho dự án mới**
- Domain còn nhỏ, chưa rõ biên module; ưu tiên học hỏi và ra sản phẩm nhanh.
- Nhóm < 10 người, không có nhu cầu scale từng phần độc lập.
- Cần transaction mạnh giữa các nghiệp vụ.
- Chưa có hạ tầng observability/DevOps đủ để vận hành nhiều service.
- Có thể bắt đầu **Modular Monolith** (như dự án này: Domain/Application/
  Infrastructure/API + MediatR) rồi tách service khi thật cần.

**Liên hệ dự án:** Culinary Blog là modular monolith .NET 10; frontend Next.js
tách riêng nhưng backend vẫn một process. Đây là lựa chọn hợp lý cho đồ án:
đủ Clean Architecture để dễ mở rộng, không gánh vận hành microservices.

---

## Câu 2. Dependency Rule trong Clean Architecture

**Nội dung quy tắc:** phụ thuộc chỉ được trỏ **vào trong**:

```
API ──► Infrastructure ──► Application ──► Domain
                 │                │
                 └── implements ──┘ (interface do Application định nghĩa)
```

- `Domain` không biết gì về framework/database/HTTP.
- `Application` định nghĩa **interface** (`IFoodBlogDbContext`,
  `IJwtTokenService`, `IFileStorageService`...) và chỉ gọi interface.
- `Infrastructure` cài đặt các interface đó (EF Core, MinIO, JWT...).
- `API` là composition root, nối implementation vào interface.

**Vì sao Application không được import trực tiếp Infrastructure**
- Nếu import, Application sẽ phụ thuộc EF Core/Npgsql/MinIO → không thể unit
  test thuần, không thể thay database/storage.
- Đảo ngược phụ thuộc (Dependency Inversion) bị phá vỡ: business logic biết chi
  tiết hạ tầng.
- Build sẽ kéo theo toàn bộ package nặng, tăng coupling và thời gian build.

**Điều gì xảy ra nếu vi phạm**
- Không mock được DbContext/Storage khi test → test chậm, phụ thuộc môi trường.
- Đổi PostgreSQL sang provider khác hoặc MinIO sang S3 thật phải sửa cả
  Application.
- Vòng phụ thuộc (circular dependency) làm project không build được.
- Kiến trúc thoái hóa thành “big ball of mud”; SRS NFR-MAINT-004 yêu cầu
  Domain 0 external dependency (dòng 930).

**Thực tế dự án:** `FoodBlog.Application.csproj` chỉ reference `Domain`
(cộng MediatR/FluentValidation/EF Core **abstractions**), không reference
`Infrastructure`. `Program.cs` đăng ký `IFoodBlogDbContext` → `FoodBlogDbContext`,
`IJwtTokenService` → `JwtTokenService`.

> Điểm lệch đã ghi nhận: `ApplicationUser : IdentityUser` nằm trong Domain nên
> Domain phải tham chiếu `Microsoft.AspNetCore.App`. Đây là trade-off để dùng
> Identity; có thể tách `ApplicationUser` sang Infrastructure nếu muốn tuyệt đối
> tuân thủ NFR-MAINT-004.

---

## Câu 3. PUT vs PATCH trong RESTful API

| Tiêu chí | PUT | PATCH |
| :--- | :--- | :--- |
| Ngữ nghĩa | Thay thế **toàn bộ** resource | Cập nhật **một phần** |
| Body | Đầy đủ field | Chỉ field cần đổi |
| Idempotent | Có | Không bắt buộc (tùy thiết kế) |
| Rủi ro | Ghi đè field không gửi lên | Cần phân biệt “không gửi” vs “gửi null” |

**Ví dụ trong Culinary Blog**
- `PUT /api/v1/recipes/{id}` — cập nhật toàn bộ recipe (title, description,
  instructions, categoryId, prep/cook time, servings, difficulty, rowVersion).
  Đây là thay thế có kiểm soát optimistic concurrency.
- `PUT /api/v1/recipes/{id}/steps/{stepId}` — thay toàn bộ nội dung một bước.
- `PATCH /api/v1/recipes/{id}/publish` — chỉ đổi trạng thái sang Published.
- `PATCH /api/v1/recipes/{id}/archive` — chỉ đổi trạng thái sang Archived.
- `PATCH /api/v1/auth/me` — chỉ cập nhật `displayName`/`avatarUrl`/`bio`, không
  đụng email/mật khẩu.

**Lưu ý:** các `PATCH` trạng thái ở trên là “action endpoint” (không nhận body);
nếu cần cập nhật từng field lẻ của recipe, nên thêm
`PATCH /recipes/{id}` với body `{ "title"?: ..., "servings"?: ... }` và dùng
`JsonPatchDocument` hoặc DTO nullable.

---

## Câu 4. Cursor-based vs Offset-based pagination

**Offset-based:** `SKIP (page-1)*pageSize TAKE pageSize`.
- Ưu: đơn giản, nhảy trang tùy ý, hiển thị “Trang 3/20”, đếm `totalCount`.
- Nhược: khi dữ liệu thay đổi giữa các lần lật trang dễ **trùng/sót** item;
  `OFFSET` lớn chậm vì DB vẫn phải quét bỏ qua N dòng.

**Cursor-based:** dùng con trỏ (id/createdAt đã encode) của item cuối.
- Ưu: ổn định khi dữ liệu thay đổi, hiệu năng tốt ở trang sâu, phù hợp infinite
  scroll/feed.
- Nhược: không nhảy trang tùy ý, không có tổng số trang, cursor phải ổn định và
  duy nhất (thường `(createdAt, id)`).

**Dự án dùng gì?** Offset-based (`page`, `pageSize`, `totalCount`, `totalPages`,
`hasNextPage`, `hasPreviousPage`) trong `GetRecipesQuery`,
`GetMyRecipesQuery`, `CategoryEndpoints`, `RecipeEndpoints /search`.
SRS quy định rõ offset (FR-SRCH-004, dòng 864).

**Lý do chọn offset**
- Phù hợp UI có thanh phân trang và bộ lọc (category, difficulty, sort) — người
  dùng muốn nhảy trang.
- Cần `totalCount` để hiển thị “có bao nhiêu công thức”.
- Quy mô dữ liệu đồ án nhỏ, `OFFSET` chưa phải bottleneck.
- Dễ triển khai với EF Core `Skip/Take`.

**Khi nào nên chuyển cursor:** feed công khai rất lớn, infinite scroll, hoặc
khi `OFFSET` vượt vài chục nghìn dòng. Khi đó thêm `nextCursor` song song,
giữ offset cho màn hình admin.

---

## Câu 5. Vì sao DTO quan trọng? Nếu trả Entity trực tiếp thì sao?

**Vai trò của DTO**
- Tách **hợp đồng API** khỏi mô hình database: đổi cột/backing field không làm
  vỡ client.
- Chỉ trả field cần thiết → giảm payload, tránh lộ dữ liệu nhạy cảm.
- Kiểm soát shape/serialization (ví dụ `RowVersion` base64, enum → string).
- Cho phép projection tối ưu (`Select` chỉ cột cần) và tránh N+1.
- Là nơi đặt validation/annotation cho request.

**Nếu trả Entity trực tiếp**
- Lộ toàn bộ cột: `AuthorId`, `IsDeleted`, `RowVersion`, navigation có thể gây
  vòng lặp JSON (`Recipe.Author.Recipes...`) hoặc `null`/`$ref`.
- Lộ thông tin nội bộ (audit, soft-delete, hash mật khẩu nếu là User).
- Client phụ thuộc schema DB; đổi DB làm vỡ API.
- Không kiểm soát được quyền: dễ trả Draft/Archived cho guest nếu quên lọc.
- Khó versioning API.

**Thực tế dự án:** `RecipeDetailDto`, `RecipeSummaryDto`, `CategoryDto`,
`UserDto`, `UserProfileDto`, `NutritionDto`... `RecipeMapper.ToDetail` chuyển
entity → DTO, `RowVersion` được encode base64, enum chuyển `.ToString()`.

---

## Câu 6. Authentication vs Authorization

- **Authentication (xác thực):** “Bạn là ai?” — đăng nhập, xác minh JWT/cookie.
- **Authorization (phân quyền):** “Bạn được làm gì?” — role, ownership, policy.

**Trong Culinary Blog**
- Chỉ cần **Authentication**:
  - `GET /api/v1/auth/me` — xem hồ sơ của chính mình.
  - `PATCH /api/v1/auth/me` — sửa hồ sơ của chính mình.
  - `POST /api/v1/auth/logout` — thu hồi refresh token của chính mình.
  - `GET /api/v1/recipes/mine` — danh sách recipe của tôi.
- Cần **Authorization**:
  - `PATCH /recipes/{id}/publish|unpublish|archive` — phải là **Owner** hoặc
    **Admin** (resource-based).
  - `POST/PUT/DELETE /categories` — role **Admin**.
  - `PUT /recipes/{id}` — Owner/Admin + kiểm tra `RowVersion`.
  - `POST /recipes/{id}/images` — Owner/Admin.
- **Không cần cả hai:** `GET /recipes`, `GET /recipes/{slug}` (Published),
  `GET /categories`, `GET /recipes/search` — public.

**Cài đặt:** `RequireAuthorization()`, `RequireAuthorization(new AuthorizeAttribute
{ Roles = "Admin" })`, và kiểm tra ownership trong handler
(`currentUser.IsAdmin || recipe.AuthorId == currentUser.UserId`).

---

## Câu 7. Vì sao JWT là stateless? Ưu/nhược so với session

**Stateless:** server không lưu phiên. Mọi thông tin cần thiết (sub, email,
role, exp, jti) nằm trong token đã ký. Server chỉ cần **verify chữ ký** + hạn
dùng là biết danh tính. Trong dự án, `JwtTokenService` tạo HS256 access token
15 phút; `Program.cs` cấu hình `TokenValidationParameters` để verify.

**Ưu điểm**
- Scale ngang dễ: bất kỳ instance nào cũng verify được, không cần sticky session
  hay session store dùng chung (NFR-SCALE-001).
- Phù hợp mobile/SPA, gọi cross-origin.
- Giảm tải DB/Redis cho mỗi request.
- Có thể mang claims/role trực tiếp.

**Nhược điểm**
- **Không revoke ngay** access token trước khi hết hạn → phải dùng TTL ngắn
  (15 phút) + refresh token có lưu DB để thu hồi.
- Token bị lộ = kẻ tấn công dùng được đến khi hết hạn.
- Payload lớn hơn session id, gửi mỗi request.
- Khó “logout mọi thiết bị” nếu không có cơ chế blacklist/rotation.
- Phải quản lý khóa bí mật, xoay khóa, clock skew.

**So với session-based:** session lưu server-side (Redis/DB), revoke tức thì
nhưng cần shared store và dễ thành bottleneck; JWT nhẹ hơn nhưng đánh đổi khả
năng thu hồi. Dự án kết hợp cả hai: JWT cho API + refresh token trong DB để
rotation/reuse detection, đồng thời giữ Identity cookie làm fallback.

---

## Câu 8. Vì sao CORS không phải biện pháp bảo mật API?

CORS là **cơ chế của trình duyệt** để bảo vệ người dùng khỏi đọc response
cross-origin, không phải để bảo vệ API khỏi kẻ tấn công.

- Kẻ tấn công có thể gọi API trực tiếp bằng `curl`, Postman, server-side script
  → không bị CORS chặn (CORS chỉ áp cho browser same-origin policy).
- Nếu API dựa vào cookie, kẻ tấn công có thể thực hiện **CSRF** hoặc lừa trình
  duyệt nạn nhân gửi request; CORS không ngăn request “simple” (form POST).
- Cấu hình sai (`Access-Control-Allow-Origin: *` + `AllowCredentials`) còn làm
  lộ dữ liệu.
- CORS không chống được XSS (XSS chạy cùng origin, đọc được token).

**Trong dự án:** `Program.cs` cấu hình CORS chỉ cho origin trong
`Cors:Origins`, `AllowCredentials` vì frontend gửi cookie. Nhưng bảo mật thật
nằm ở: JWT signature + expiry, rate limiting, authorization/ownership, input
validation, HTTPS, CSP, không lưu token ở nơi XSS đọc được.

**Khi nào kẻ tấn công “bypass” CORS?** Khi gọi từ server/CLI; khi dùng
`no-cors` (không đọc được response nhưng vẫn gửi được request); khi endpoint
cho phép method/header nguy hiểm; khi lỗi cấu hình origin reflection.

---

## Câu 9. Resource-based authorization

Là kiểm tra quyền dựa trên **tài nguyên cụ thể** (không chỉ role), thường so
sánh chủ sở hữu hoặc trạng thái tài nguyên với danh tính người dùng.

**Ví dụ trong Culinary Blog**
- `UpdateRecipeCommandHandler`:
  `if (!currentUser.IsAdmin && recipe.AuthorId != currentUser.UserId) throw
  RecipeForbiddenException();` → Author chỉ sửa recipe của mình; Admin sửa tất
  cả.
- Tương tự cho publish/unpublish/archive/delete và upload ảnh.
- `GetMyRecipeByIdQuery` lọc `r.Id == id && (IsAdmin || r.AuthorId == UserId)`.
- Xem Draft/Archived qua slug: chỉ Owner/Admin (`RecipeEndpoints`).

**Ví dụ ngoài ứng dụng**
- Google Docs: chỉ owner/editor được sửa tài liệu; người được share chỉ đọc.
- GitHub: chỉ owner repo hoặc collaborator có quyền push mới merge PR.
- Ngân hàng: chỉ chủ tài khoản được xem sao kê tài khoản đó.

**Cài đặt .NET:** có thể dùng `IAuthorizationHandler` +
`AuthorizationHandler<ResourceRequirement, Recipe>` hoặc kiểm tra trong
Application handler (dự án chọn cách sau để giữ business rule trong Application,
đúng Dependency Rule).

---

## Câu 10. Vì sao cần Repository Pattern trên EF Core?

**Testability**
- Repository trả về abstraction (`IRepository<T>`, `ICategoryRepository`), cho
  phép mock/stub trong unit test mà không cần database thật.
- Có thể thay InMemory/SQLite/PostgreSQL mà không đổi Application.
- Các query phức tạp được đóng gói, test tập trung.

**Dependency Inversion**
- Application định nghĩa `ICategoryRepository`; Infrastructure cài đặt bằng EF
  Core. Application không biết `DbContext`, `Include`, `Npgsql`.
- `Program.cs`/`UnitOfWork` là nơi nối implementation.

**Lợi ích khác**
- Tái sử dụng query (ví dụ `GetVisibleRecipesAsync` dùng cho category detail).
- Kiểm soát soft delete/global filter, `AsNoTracking`, projection.
- Tránh rò rỉ `IQueryable` ra ngoài (dễ bị N+1 hoặc query không giới hạn).

**Phản biện:** Repository trên EF Core đôi khi bị coi là “anti-pattern” vì
`DbContext` đã là Unit of Work + Repository. Với dự án nhỏ có thể dùng
`DbContext` trực tiếp. Nhưng SRS yêu cầu Repository + Clean Architecture, và
việc bọc lại giúp test/đảo ngược phụ thuộc rõ ràng. Dự án dùng cả hai:
`IRepository<T>` generic + repository chuyên biệt (`ICategoryRepository`,
`IRecipeRepository`).

---

## Câu 11. Unit of Work giải quyết gì mà Repository không?

Repository quản lý **một aggregate/entity**; Unit of Work quản lý **một
transaction/nghiệp vụ** gồm nhiều repository và đảm bảo “tất cả hoặc không”.

**Vấn đề Repository một mình**
- Mỗi repository có thể `SaveChanges` riêng → nửa chừng lỗi sẽ để lại dữ liệu
  rác (ví dụ Recipe đã lưu nhưng Steps lỗi).
- Không có điểm chung để commit/rollback nhiều thay đổi.
- Khó đảm bảo optimistic concurrency trên nhiều entity.

**Ví dụ trong Culinary Blog**
- `CreateRecipeCommand`: tạo `Recipe` + `RecipeStep` + `RecipeIngredient` +
  `RecipeNutrition` rồi `db.SaveChangesAsync()` **một lần** → hoặc tất cả được
  lưu, hoặc không gì cả.
- `UpdateRecipeCommand`: dùng `IUnitOfWork`, kiểm tra `RowVersion`, cập nhật
  recipe + nutrition, gọi `uow.SaveChangesAsync()`; bắt
  `DbUpdateConcurrencyException` → trả 409.
- Xóa recipe: xóa cascade Steps/Ingredients/Images + gọi cleanup MinIO, tất cả
  trong một luồng nghiệp vụ.

**Cài đặt:** `UnitOfWork` giữ `FoodBlogDbContext`, expose các repository
(`Recipes`, `Categories`, ...), `SaveChangesAsync`, `BeginTransactionAsync`.

---

## Câu 12. Cartesian Explosion khi Include nhiều collection

**Cơ chế:** EF Core dịch `Include` các collection thành các `LEFT JOIN`. Khi
join nhiều collection 1-n với nhau, SQL trả về **tích Descartes** giữa các
nhánh, rồi EF khử trùng lặp ở client.

**Với recipe có 10 steps và 15 ingredients**
- Một query `Include(Steps).Include(Ingredients)`:
  `10 × 15 = 150` dòng (chưa tính Images; nếu có k ảnh thì `150 × k`).
- Dữ liệu Recipe bị lặp 150 lần → tốn băng thông, bộ nhớ, thời gian
  materialize; dễ gây N+1 ngược (over-fetch).

**Khi dùng `AsSplitQuery()`**
- EF Core tách thành 3 query:
  1. `Recipes`: 1 dòng.
  2. `RecipeSteps`: 10 dòng.
  3. `RecipeIngredients`: 15 dòng.
- Tổng `1 + 10 + 15 = 26` dòng, không nhân chéo. Đổi lại là 3 round-trip
  (nhiều hơn 1) nhưng dữ liệu sạch và ổn định khi số collection tăng.

**Khi nào dùng**
- `AsSplitQuery()` khi include từ 2 collection trở lên hoặc dữ liệu lớn.
- `AsSingleQuery()` khi include 1 collection hoặc cần transaction snapshot
  nhất quán.
- Luôn cân nhắc `AsNoTracking()` cho query đọc.

**Thực tế dự án:** `GetRecipeBySlugQuery` và `GetMyRecipeByIdQuery` include
Steps + Ingredients + Images. Với dữ liệu đồ án nhỏ, single query chấp nhận
được; nếu recipe lớn nên thêm `.AsSplitQuery()`.

---

## Câu 13. React Server Components vs Client Components

**RSC (mặc định trong App Router)**
- Render trên server, **không gửi JS component** xuống client → bundle nhỏ.
- Có thể `async` và fetch trực tiếp DB/API, giữ secret trên server.
- Không dùng được hook/state/event.
- Phù hợp nội dung tĩnh/đọc nhiều, SEO.

**Client Component (`"use client"`)**
- Render + hydrate trên client, gửi JS xuống → tăng bundle.
- Dùng được `useState`, `useEffect`, event handler, browser API.
- Cần cho tương tác, form, store, animation.

**Trong Culinary Blog**
- Nên là **RSC**: `/` (featured + categories, ISR), `/recipes` (list + filter
  đọc từ `searchParams`), `/recipes/[slug]` (chi tiết, JSON-LD/SEO),
  layout/header tĩnh, trang lỗi/loading.
- Cần **`"use client"`**: form login/register, `AuthProvider` (Zustand),
  dashboard quản lý recipe, `ImageUploader`, `IngredientForm`, `StepForm`,
  `RecipeContentEditor`, các nút publish/archive/delete, search input có
  debounce, toast/sonner.
- Kỹ thuật: đẩy `"use client"` xuống **leaf component** nhỏ, giữ page là RSC;
  truyền dữ liệu đã fetch từ RSC xuống client qua props.

**Lưu ý:** dự án hiện dùng Next.js 16; các page đọc `searchParams`/`params` là
async server component. Auth store là client-only nên được bọc trong
`AuthProvider`.

---

## Câu 14. Auth.js lưu session trong HTTP-only cookie vs localStorage

**HTTP-only cookie**
- JS không đọc được (`document.cookie` bị chặn với `HttpOnly`) → XSS không lấy
  được session token.
- Có thể đặt `Secure`, `SameSite=Lax/Strict` → giảm CSRF.
- Trình duyệt tự gửi kèm request cùng origin.
- Nhược: cần CSRF protection; khó dùng cross-domain; mobile/third-party client
  cần cơ chế khác.

**localStorage**
- JS đọc được → **XSS đánh cắp token dễ dàng**; token tồn tại lâu, không tự
  hết hạn theo phiên trình duyệt.
- Không gửi tự động; phải gắn `Authorization` thủ công.
- Dễ bị malware/extension đọc.

**XSS ảnh hưởng khác nhau**
- Với cookie HttpOnly: XSS vẫn có thể **gửi request thay người dùng** (CSRF-like)
  nhưng không đọc được token để mang sang nơi khác.
- Với localStorage: XSS đọc token → kẻ tấn công dùng token từ máy khác, chiếm
  phiên hoàn toàn.

**Khi accessToken backend được đưa vào session**
- Auth.js giữ accessToken trong session server-side (hoặc mã hóa trong cookie),
  client chỉ thấy session id/opaque token.
- Cần refresh flow ở server (callback/jwt) để làm mới access token mà không lộ
  refresh token ra client.
- Tăng độ phức tạp nhưng giảm rủi ro XSS; phải bảo vệ cookie bằng `HttpOnly`,
  `Secure`, `SameSite`, CSRF token.

**Thực tế dự án:** SRS dùng JWT Bearer; frontend hiện lưu token trong Zustand
(persist localStorage) để gọi API. Đây là **điểm cần cải thiện** theo hướng
Auth.js/HTTP-only cookie hoặc BFF pattern; nếu giữ localStorage phải siết CSP,
sanitize output và TTL ngắn.

---

## Câu 15. Server State, Client State, URL State, Form State

| Loại | Bản chất | Công cụ đề xuất | Vì sao phù hợp |
| :--- | :--- | :--- | :--- |
| **Server State** | Dữ liệu thuộc server, có cache/refresh/consistency | TanStack Query (hoặc Next fetch/ISR) | Tự cache, dedupe, retry, invalidate, background refetch; không cần tự viết loading/error. |
| **Client State** | UI state toàn cục: auth, theme, sidebar | Zustand | Nhẹ, không boilerplate, selector tối ưu re-render, persist dễ. |
| **URL State** | Trạng thái chia sẻ được qua link: page, filter, sort, q | URL search params (`useSearchParams`/props) | Share/bookmark/back-forward; SSR đọc được; không mất khi refresh. |
| **Form State** | Giá trị/nhập liệu/lỗi của form | React Hook Form (+ Zod) | Ít re-render, uncontrolled, validation schema, tích hợp resolver. |

**Trong Culinary Blog**
- Server State: danh sách recipe, category, profile — fetch bằng `lib/api.ts`
  + Next ISR/`revalidate` tags; có thể nâng cấp TanStack Query cho dashboard.
- Client State: `useAuthStore` (Zustand) cho user/token/status; theme/sidebar.
- URL State: `/recipes?page=&categoryId=&difficulty=&maxCookTime=&sort=`,
  `/recipes/search?q=`.
- Form State: `StepForm`, `IngredientForm` dùng `react-hook-form`
  (`useForm`, `useFieldArray`); login/register dùng RHF + Zod.

**Nguyên tắc:** không đưa server state vào Zustand; không đưa filter vào
component state nếu cần share link; không lưu form state vào global store.

---

## Câu 16. Next.js `<Image>`, `sizes`, Core Web Vitals và thay đổi `priority` ở Next 16

**Cách browser chọn ảnh**
- `<Image>` sinh `srcset` gồm nhiều biến thể width (ví dụ 640, 750, 828, 1080,
  1200, 1920, 2048, 3840) và `sizes` mô tả kích thước hiển thị theo viewport.
- Browser đọc `sizes`, tính **effective width** = kích thước CSS × DPR, rồi chọn
  candidate gần nhất trong `srcset` → tải ảnh nhỏ nhất đủ nét.
- `layout`/`fill`/`width-height` giúp reserve không gian, tránh layout shift.

**Vì sao quan trọng cho Core Web Vitals**
- **LCP**: chọn đúng ảnh + `preload`/`fetchPriority="high"` cho ảnh hero giúp
  LCP nhanh.
- **CLS**: width/height hoặc `fill` + container ratio giữ chỗ → không nhảy layout.
- **INP**: ảnh nhẹ, lazy load ảnh dưới màn hình → main thread rảnh hơn.

**Next.js 16 thay `priority`**
- `priority` cũ vừa preload vừa set fetch priority, hành vi không rõ ràng.
- Next 16 **deprecate `priority`**, thêm `preload` (chỉ preload) và khuyến nghị
  dùng `loading="eager"` hoặc `fetchPriority="high"` tùy mục đích.
- Lý do: tách bạch “có preload hay không” khỏi “mức ưu tiên fetch”, tránh
  preload quá nhiều ảnh làm nghẽn bandwidth.

**Thực tế dự án:** frontend hiện dùng thẻ `<img>` trong `RecipeCard`,
`ImageUploader` (chưa dùng `next/image`). Đây là việc cần làm để tối ưu LCP/CLS:
chuyển sang `next/image` với `sizes`, `preload` cho ảnh hero, `loading="lazy"`
cho ảnh gallery.

---

## Câu 17. Logging, Metrics, Distributed Tracing

| Loại | Trả lời câu hỏi | Ví dụ trong dự án |
| :--- | :--- | :--- |
| **Logging** | “Chuyện gì đã xảy ra?” — sự kiện rời rạc, có ngữ cảnh | Serilog Console/File/Seq; `CorrelationId`, `UserId`; `AuditLogger`. |
| **Metrics** | “Hệ thống đang khỏe không?” — số liệu tổng hợp theo thời gian | OpenTelemetry metrics; `foodblog.recipes.created`, `foodblog.recipes.published`; request rate/latency. |
| **Tracing** | “Request đi qua những đâu, chậm ở đâu?” — span liên kết | OTel AspNetCore + HttpClient + EF Core; trace một request từ API → DB/MinIO. |

**Khi nào dùng để debug performance**
- **Metrics trước:** phát hiện p95/p99 tăng, error rate tăng, throughput giảm →
  khoanh vùng thời điểm/module.
- **Tracing:** khi metrics báo endpoint `/recipes/search` chậm, mở trace để xem
  span DB chiếm bao nhiêu, có N+1 hay không.
- **Logging:** khi cần chi tiết cụ thể (correlation id, user id, tham số, lỗi
  exception) để tái hiện.

**Thực tế:** Serilog đã cấu hình sẵn; OTel đã đăng ký tracing/metrics; health
check `/health`, `/health/live`, `/health/ready` dùng để giám sát.

---

## Câu 18. Vì sao tách liveness và readiness?

- **Liveness** (`/health/live`): process còn sống, event loop còn chạy. Nếu fail
  → orchestrator **restart container**. Không kiểm tra dependency (DB/Redis) để
  tránh restart vô ích khi DB chậm.
- **Readiness** (`/health/ready`): sẵn sàng nhận traffic, dependency (DB, Redis,
  MinIO) OK. Nếu fail → **temporarily remove khỏi load balancer**, không restart.
- **`/health`**: tổng hợp, dùng cho dashboard/alert.

**Nếu chỉ có một `/health` kiểm tra cả DB**
- DB chậm → `/health` fail → orchestrator tưởng app chết → **restart loop**,
  trong khi app vẫn ổn và restart không sửa được DB.
- Hoặc load balancer ngừng route traffic → toàn bộ hệ thống downtime dù chỉ một
  dependency chậm.
- Không phân biệt được “app hỏng” vs “dependency hỏng” → khó chẩn đoán.

**Thực tế dự án:** `HealthEndpoints.cs` cài đúng 3 endpoint; `/health/live`
luôn trả Healthy; `/health/ready` kiểm tra database, redis, minio và trả 503
nếu có dependency lỗi.

---

## Câu 19. IMemoryCache vs IDistributedCache

- **IMemoryCache**: lưu trong RAM của **mỗi process**, nhanh nhất, không cần
  network, nhưng **không chia sẻ** giữa các instance và mất khi restart.
- **IDistributedCache**: abstraction trên Redis/SQL Server/NCache; dữ liệu dùng
  chung giữa các instance, sống sót restart (tùy backend), có TTL.

**Nếu scale 3 instances với IMemoryCache**
- Mỗi instance có cache riêng → **cache không nhất quán**: instance A đã
  invalidate sau khi update, instance B/C vẫn trả dữ liệu cũ.
- Cache hit rate thấp (mỗi instance phải tự warm).
- Tốn RAM ×3, khó quan sát tổng thể.
- Khi deploy/restart, cache mất → tăng tải DB đột ngột.

**IDistributedCache giải quyết**
- Một nguồn cache chung (Redis) → invalidate một lần, tất cả instance thấy.
- Hit rate cao hơn, TTL tập trung, có thể theo dõi.
- Redis có thể persistent (AOF) và scale riêng.

**Thực tế dự án:** `CategoryEndpoints` dùng `IDistributedCache` với key
`categories:all`, TTL 60 phút; `Program.cs` đăng ký Redis khi có
`Redis:Connection`, fallback `AddDistributedMemoryCache()` khi dev/test. Đây là
cách sửa mâu thuẫn SRS (FR-CAT-001 ghi `IMemoryCache`, NFR-PERF-003 yêu cầu
Redis).

---

## Câu 20. Full-Text Search và vì sao dùng `'simple'` cho tiếng Việt

**FTS là gì?** PostgreSQL lưu `tsvector` (các lexeme đã chuẩn hóa) và truy vấn
bằng `tsquery`, có index GIN, xếp hạng `ts_rank`. Nhanh hơn `LIKE '%...%'` và
hiểu ngôn ngữ (stop word, stemming).

**Vì sao `'simple'` thay vì `'english'` cho tiếng Việt**
- `'english'` áp dụng **stemming tiếng Anh** và stop-word tiếng Anh: biến đổi
  từ theo quy tắc tiếng Anh, bỏ các từ như “the”, “and”... không phù hợp tiếng
  Việt.
- Tiếng Việt có dấu, từ ghép, không chia thì; stemming tiếng Anh có thể cắt
  sai từ tiếng Việt.
- `'simple'` chỉ **lowercase + tách token theo khoảng trắng/dấu câu**, giữ
  nguyên từ; kết hợp `unaccent` để tìm không dấu.

**Ví dụ “phở bò”**
- `'simple'` + `unaccent`: token `pho`, `bo` → tìm được “phở bò”, “Pho Bo”,
  “pho bo”.
- `'english'`: có thể stem `phở`/`bò` theo quy tắc tiếng Anh (không có nghĩa),
  đồng thời bỏ stop-word tiếng Anh; kết quả token không ổn định, dễ trượt khi
  người dùng gõ không dấu hoặc có dấu.
- Ngoài ra `'english'` không xử lý `unaccent` trừ khi ta tự bọc.

**Thực tế dự án:** trigger `recipes_searchvector_update()` dùng
`to_tsvector('simple', unaccent(Title) || ' ' || unaccent(Description))`;
endpoint `/recipes/search` dùng
`websearch_to_tsquery('simple', unaccent(q))` + `ts_rank`. Migration
`RecipeSearchSimpleConfig` chuyển từ `'english'` sang `'simple'`.

---

# PHẦN II — BÁO CÁO VÀ GIẢI THÍCH MÃ NGUỒN

## II.1. Từng thành viên báo cáo nội dung đã làm

> Tổng hợp theo `README.md` (phân công tuần 1–5). Mỗi thành viên nên xác nhận
> lại phần của mình khi thuyết trình.

| Thành viên | Mảng phụ trách | Nội dung chính đã làm |
| :--- | :--- | :--- |
| **Ngô Văn Trường Long** (2312677) | Authentication & Observability | `ApplicationUser`/`RefreshTokens`, JWT 15 phút + refresh 7 ngày, register/login/logout/refresh/Google OAuth PKCE, profile, Hangfire welcome email, health checks, Serilog + CorrelationId + UserId, OpenTelemetry, frontend auth store/UI login/register/profile. |
| **Trần Đình Việt Huy** (2312635) | Category & Search | Entity `Category`, migration, `unaccent`/`pg_trgm`, GIN index, CRUD category + phân quyền Admin, cache danh mục, full-text search (`tsvector`, `tsquery`, `ts_rank`, `unaccent`), filter/sort/offset pagination, frontend `/search`, sitemap/robots. |
| **Trần Quốc Khánh** (2312646) | Recipe Core | Clean Architecture + MediatR + FluentValidation, entity `Recipe`/`RecipeNutrition`, `RowVersion`, CQRS Create/Get/Update, publish/unpublish/archive/delete, resource authorization, Output Cache + `EvictByTagAsync`, dashboard author, frontend home/list/detail/create/edit, SEO metadata. |
| **Trần Gia Huy** (2312637) | Child Data & File Storage | Entity `RecipeStep`/`RecipeIngredient`/`RecipeImage`, API + validation + renumber, MIME/magic-byte/5MB validation, MinIO upload/delete, thumbnail 300×300 & 800×600, orphan cleanup, fallback khi MinIO lỗi, frontend gallery/uploader/ingredient/step form, test child data/file storage. |

**Mẫu báo cáo cá nhân (mỗi bạn điền):**
1. Mảng phụ trách + file chính.
2. 3–5 việc đã hoàn thành, minh chứng (commit/PR/API).
3. Khó khăn và cách giải quyết.
4. Việc còn lại/bàn giao.
5. Tự đánh giá mức độ hoàn thành.

## II.2. Giải thích các đoạn mã tiêu biểu

### 2.1. `JwtTokenService.CreateAccessToken`
- Tạo claims: `sub`, `NameIdentifier`, `email`, `name`, `jti`, role.
- Ký HS256 bằng `JwtOptions.Secret` (bắt buộc ≥ 32 ký tự).
- TTL 15 phút; trả `(token, expiresAt)`.
- Ý nghĩa: đáp ứng CONS-004/NFR-SEC-002; token stateless, verify ở `Program.cs`.

### 2.2. Policy scheme `"Smart"` trong `Program.cs`
- Nếu request có header `Authorization` → forward sang `JwtBearer`.
- Ngược lại → forward sang Identity cookie (`IdentityConstants.ApplicationScheme`).
- Ý nghĩa: vừa đáp ứng JWT cho REST API, vừa giữ tương thích luồng cookie cũ;
  không phá vỡ frontend/test hiện tại.

### 2.3. `GetRecipesQueryHandler`
- Lọc visibility: Admin → tất cả; có `CurrentUserId` → Published + Draft/Archived
  của chính mình; guest → Published.
- Áp filter `categoryId`, `difficulty`, `maxCookTime`, `minServings`.
- Sort whitelist (`createdAt`, `title`, `cookTime`), `CountAsync` + `Skip/Take`.
- Projection sang `RecipeSummaryDto` (không trả entity).

### 2.4. `GetRecipeBySlugQueryHandler`
- `AsNoTracking`, include Steps/Ingredients/Images/Category/Author.
- Map qua `RecipeMapper.ToDetail` (DTO, base64 RowVersion, enum string).
- Endpoint chặn Draft/Archived với guest (403) — resource-based preview.

### 2.5. `CreateRecipeCommandHandler`
- Kiểm tra đăng nhập + category tồn tại.
- Sinh slug duy nhất (`SlugHelper.Generate` + đếm hậu tố).
- Tạo Recipe Draft + tuỳ chọn Steps/Ingredients/Nutrition trong cùng
  `SaveChangesAsync` (Unit of Work).
- Trả `RecipeDetailDto`.

### 2.6. `UpdateRecipeCommandHandler`
- Lấy recipe `GetForUpdateAsync`; kiểm tra Owner/Admin.
- So sánh `RowVersion` client gửi với DB → nếu khác ném
  `RecipeConcurrencyException` (409).
- Cập nhật field + nutrition; bắt `DbUpdateConcurrencyException`.

### 2.7. `CategoryEndpoints` + `IDistributedCache`
- Key `categories:all`, serialize JSON, TTL 60 phút.
- Sau Create/Update/Delete gọi `RemoveAsync` để invalidate.
- Fallback in-memory khi không có Redis.

### 2.8. `/recipes/search` raw SQL
- `websearch_to_tsquery('simple', unaccent(q))` + `ts_rank`.
- Subquery lấy `PrimaryImageUrl`.
- Phân trang offset, trả metadata đầy đủ.

### 2.9. `GlobalExceptionMiddleware`
- Map exception → RFC 7807 (`type`, `title`, `status`, `traceId`,
  `correlationId`, `code`, `errors`).
- Validation → 422 (theo FR-AUTH), Conflict → 409, NotFound → 404,
  Unauthorized → 401, Forbidden → 403.

### 2.10. `ImageJobWorker` + `ImageSharpResizer`
- Hangfire job nhận `recipeId`/`imageId`, tải ảnh gốc, resize 300×300 và
  800×600, upload lại MinIO, cập nhật DB.
- Retry/idempotent; orphan cleanup định kỳ.

## II.3. Câu hỏi thiết kế & cài đặt

### a. Ứng dụng có bao nhiêu module/tính năng?

Theo SRS có **7 nhóm chức năng, 34 FR**:
1. **FR-AUTH** (7): register, login, Google, refresh, logout, get/update profile.
2. **FR-CAT** (5): list, detail, create, update, delete category.
3. **FR-RCP** (10): list, detail, create, update, publish, unpublish, archive,
   delete, images, steps/ingredients.
4. **FR-SRCH** (4): FTS, filter, sort, pagination.
5. **FR-FILE** (2): upload/delete ảnh MinIO.
6. **FR-JOB** (3): welcome email, thumbnail, sitemap.
7. **FR-OBS** (3): health, structured logging, tracing/metrics.

Triển khai thực tế mở rộng thành **10 module kỹ thuật**: Auth, Categories,
Recipes, Child Data (Steps/Ingredients/Images), Search, Profile, Admin
(users/logs), Storage, Jobs, Observability + SEO (sitemap/robots) + frontend
(public/dashboard/admin/auth).

### b. Những điểm mâu thuẫn trong SRS

1. **Cache**: FR-CAT-001 (dòng 588) yêu cầu `IMemoryCache`; NFR-PERF-003
   (dòng 902) và kiến trúc (dòng 20) yêu cầu Redis distributed cache.
2. **Xóa**: `BaseEntity` có `IsDeleted` + Global Query Filter (dòng 1078) nhưng
   FR-RCP-007 (dòng 804/1222) ghi “Xóa vĩnh viễn” + cascade.
3. **Response envelope**: 5.2 (dòng 973) quy định `{data, meta}`; nhưng bảng
   endpoint và `PagedResult` (dòng 720) trả `{items, totalCount, page, pageSize}`
   trực tiếp.
4. **Mã lỗi validation**: FR-AUTH (dòng 409/413) và nhiều endpoint dùng **422**;
   Appendix B (dòng 1265/1299) lại ghi `VALIDATION_ERROR` = **400**.
5. **Google OAuth**: bảng endpoint (dòng 1194) nhận `{idToken}`; luồng chi tiết
   (dòng 460) dùng **Auth.js v5 + PKCE** (code/codeVerifier/redirectUri).
6. **Domain 0 external dependency**: NFR-MAINT-004 (dòng 930) yêu cầu Domain
   không phụ thuộc ngoài; nhưng `ApplicationUser : IdentityUser` buộc Domain
   tham chiếu ASP.NET Core Identity.
7. **Lưu token**: CONS-004 (dòng 342) yêu cầu JWT stateless Bearer; câu hỏi 14
   và luồng Google lại dùng **Auth.js session trong HTTP-only cookie**. Hai mô
   hình bảo mật khác nhau.
8. **Sitemap**: FR-JOB-003 (dòng 883) yêu cầu **Hangfire recurring job** tạo
   `sitemap.xml` và ping Google; NFR-SEO-003 (dòng 940) lặp lại. Trong khi
   frontend Next.js có thể tự sinh metadata route.
9. **Route category**: bảng UI (dòng 954–955) có `/categories` và
   `/categories/[slug]`; SRS 5.2 lại định nghĩa endpoint `GET /categories/{slug}`
   trả cả category + recipes, nên frontend dùng route riêng thay vì chỉ filter
   qua `/recipes?categoryId=`.

### c. Hướng giải quyết và lý do

| Mâu thuẫn | Cách giải quyết | Lý do |
| :--- | :--- | :--- |
| Cache | Dùng `IDistributedCache` (Redis), fallback `AddDistributedMemoryCache()` khi dev/test | Đáp ứng NFR-PERF-003 + scale ngang; fallback giữ app chạy độc lập. |
| Xóa | Soft delete + cascade mềm (`IsDeleted`), cleanup MinIO; không hard delete mặc định | Giữ audit/khôi phục; tránh mất dữ liệu; có thể thêm job hard-delete sau N ngày. |
| Envelope | Trả DTO trực tiếp + metadata phân trang phẳng (`items`, `totalCount`, ...) | Đơn giản, khớp bảng endpoint và frontend hiện tại; có thể bọc envelope ở gateway nếu cần. |
| 400/422 | Dùng **422** cho lỗi validation nghiệp vụ; 400 cho lỗi cú pháp/MIME | 422 đúng ngữ nghĩa “hiểu request nhưng dữ liệu không hợp lệ”; FR-AUTH yêu cầu 422. |
| Google | Dùng **PKCE** (`code`, `codeVerifier`, `redirectUri`) | An toàn hơn `idToken` ở SPA; tránh lộ client secret; chuẩn OAuth 2.1. |
| Domain | Chấp nhận `ApplicationUser : IdentityUser` trong Domain; ghi chú trade-off | Tránh phức tạp mapping Identity; có thể tách sau nếu cần tuyệt đối. |
| Token storage | JWT Bearer cho API; frontend Zustand persist; khuyến nghị chuyển HTTP-only cookie/BFF | Đáp ứng API-driven + SPA; ghi nhận rủi ro XSS và lộ trình cải thiện. |
| Sitemap | Next.js `app/sitemap.ts` + `app/robots.ts` (metadata route) | Tự cập nhật theo dữ liệu API, ít vận hành hơn cron; vẫn có thể thêm Hangfire job nếu cần ping Google. |
| Route category | Triển khai `/categories` (ISR 3600) + `/categories/[slug]` (ISR 600) dùng `GET /categories/{slug}`; sidebar trỏ về route này | Đúng URL semantic NFR-SEO-004; tái sử dụng API có sẵn, không nhân đôi logic filter. |

### d. Điểm/tính năng mới so với SRS

- **Visibility hardening**: Author/Admin thấy Draft/Archived của mình trên cả
  list và category detail (SRS chỉ nói public list Published). `/search` vẫn
  chỉ trả Published vì là trang công khai, không gửi token.
- **Output Cache vary theo `Authorization`/`Cookie`**: chống rò rỉ dữ liệu nháp
  qua cache.
- **Redis fallback**: `AddDistributedMemoryCache()` khi chưa cấu hình Redis.
- **Custom OpenTelemetry metrics**: `foodblog.recipes.created`,
  `foodblog.recipes.published`.
- **Hangfire retry policy**: `WelcomeEmailJob` retry 3 lần với backoff
  1m/5m/30m (NFR-REL-002).
- **`GET /recipes/mine/{id}`**: chi tiết đầy đủ cho dashboard, kể cả Draft.
- **Admin module**: quản lý users + audit logs (`/admin/users`, `/admin/logs`).
- **Audit log** cho các hành động quan trọng (login, recipe CRUD, publish...).
- **Next.js metadata route** `sitemap.ts`/`robots.ts` thay cho cron.
- **Rate limit headers**: middleware tự đếm fixed-window theo IP và trả
  `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`,
  `Retry-After` cho cả response thành công lẫn 429.
- **Trang `/search`** (SSR, FTS + phân trang) và **`/dashboard/categories`**
  (Admin CRUD danh mục) bổ sung đủ bảng route UI 5.1.
- **Test infrastructure**: `TestAuthHandler`, InMemory DB, `InMemoryFileStorage`,
  `RecordingImageJobQueue`.
- **Tài liệu**: `.editorconfig`, `ARCHITECTURE.md`, `CHANGELOG.md`,
  `docs/on-tap-cuoi-ky.md`.
- **JWT + Identity cookie song song** qua policy scheme `"Smart"`.

### e. Tính năng chưa cài đặt / việc chưa làm và lý do

| Hạng mục | Trạng thái | Lý do |
| :--- | :--- | :--- |
| `/categories`, `/categories/[slug]` (frontend) | Đã có | ISR 3600/600; dùng `GET /categories` và `GET /categories/{slug}`. |
| 301 redirect khi slug thay đổi (NFR-SEO-004) | Không áp dụng | Slug sinh một lần khi tạo và giữ nguyên khi cập nhật, nên không phát sinh URL cũ cần redirect. |
| `generateStaticParams` cho `/recipes/[slug]`, `/categories/[slug]` | Chưa | ISR on-demand + `revalidate` 300/600; không pre-render toàn bộ slug lúc build để tránh build phụ thuộc Backend/dữ liệu lớn. |
| Hangfire recurring job tạo sitemap + ping Google Search Console | Chưa | Đã thay bằng Next metadata route; ping Google cần quota/credentials. |
| JSON-LD Recipe Schema + OpenGraph động cho từng recipe | Đã có | `generateMetadata` + `application/ld+json` trong `app/recipes/[slug]/page.tsx`. |
| `next/image` + tối ưu LCP/CLS | Chưa | Đang dùng `<img>`; cần chuyển đổi và đo Core Web Vitals. |
| Auth.js/HTTP-only cookie thay localStorage | Chưa | Đang dùng Zustand persist; cần thiết kế BFF/session. |
| ArchUnit.NET + coverage ≥ 80% | Chưa | Đã có 75 test (unit + integration) nhưng chưa đo coverage/arch test. |
| Load test p95/p99, benchmark FTS < 150ms | Chưa | Cần môi trường PostgreSQL thật + dữ liệu lớn. |
| Google OAuth end-to-end test | Chưa | Cần client id/secret và tài khoản Google thật. |
| Redis hit-rate ≥ 80% monitoring | Chưa | Cần Prometheus/Grafana hoặc Redis INFO. |
| Hard delete theo lịch + cascade MinIO đầy đủ | Một phần | Đang soft delete; cleanup orphan có job riêng. |

---

# PHẦN III — GIẢI QUYẾT VẤN ĐỀ (THIẾT KẾ TÍNH NĂNG MỚI)

## III.1. Đăng ký nhận email khi có công thức mới

**Mục tiêu:** người dùng đăng ký nhận thông báo khi hệ thống publish recipe mới.

**Thiết kế**
1. **Database**: bảng `NewsletterSubscriptions`
   (`Id`, `Email` unique, `UserId?`, `IsConfirmed`, `ConfirmToken`,
   `UnsubscribeToken`, `CreatedAt`, `ConfirmedAt`, `UnsubscribedAt`).
2. **API**:
   - `POST /api/v1/newsletter/subscribe` `{ email }` → gửi email xác nhận
     (double opt-in).
   - `GET /api/v1/newsletter/confirm?token=...`.
   - `POST /api/v1/newsletter/unsubscribe` `{ token }`.
3. **Domain event**: khi `PublishRecipeCommand` thành công, phát
   `RecipePublishedEvent(recipeId, title, slug)`.
4. **Handler**: `RecipePublishedEventHandler` enqueue Hangfire job
   `SendNewRecipeNotificationsJob`.
5. **Job**: lấy danh sách subscriber đã confirm, gửi email theo batch (MailKit),
   template có tiêu đề, mô tả, ảnh, link `/recipes/{slug}`, link unsubscribe;
   retry 3 lần; ghi log.
6. **Idempotency**: bảng `NotificationLog(RecipeId, SubscriberId, SentAt)` để
   không gửi trùng khi retry.
7. **Hiệu năng**: batch 100–500 email/job, queue riêng; không chặn request
   publish (fire-and-forget qua Hangfire).
8. **Bảo mật**: token ngẫu nhiên, hết hạn; rate limit subscribe; SPF/DKIM/DMARC.

**Ưu:** không chặn publish; retry được; unsubscribe chuẩn.
**Nhược:** phức tạp thêm (double opt-in, template, chống spam); cần dịch vụ
email uy tín để tránh vào spam.

---

## III.2. Banner quảng cáo có thu phí

**Thiết kế**
1. **Database**:
   - `Advertisements` (`Id`, `Title`, `ImageUrl`, `TargetUrl`, `Placement`
     (home_top/sidebar/in_article), `StartsAt`, `EndsAt`, `IsActive`,
     `Weight`, `AdvertiserId`, `Price`, `Status`).
   - `AdEvents` (`Id`, `AdId`, `Type` impression/click, `OccurredAt`, `IpHash`,
     `UserAgent`) hoặc bảng aggregate theo ngày.
2. **API**:
   - Admin: CRUD + duyệt (`PATCH /ads/{id}/approve`).
   - Public: `GET /api/v1/ads?placement=home_top` → trả ad đang active, chọn
     theo weight; cache ngắn (1–5 phút).
   - Tracking: `POST /api/v1/ads/{id}/impression`, `POST /api/v1/ads/{id}/click`
     (rate limit, lọc bot).
3. **Frontend**: component `<AdSlot placement="home_top" />`, lazy load, reserve
   chiều cao để tránh CLS, `rel="sponsored nofollow"`, nhãn “Quảng cáo”.
4. **Thanh toán**: tích hợp VNPay/MoMo/Stripe; hợp đồng + invoice; trạng thái
   `Pending → Paid → Running → Expired`.
5. **Scale**: Redis counter cho impression/click, job tổng hợp vào DB; CDN cho
   ảnh; chọn ad theo thuật toán weight/round-robin.
6. **Pháp lý/riêng tư**: consent cookie, không tracking cá nhân hóa nếu chưa
   đồng ý; lưu IP hash.

**Ưu:** tách module, không ảnh hưởng recipe; doanh thu đo được.
**Nhược:** cần quy trình duyệt/nội dung, chống gian lận click, tích hợp thanh
toán; ảnh hưởng UX/CLS.

---

## III.3. RSS feed

**Thiết kế**
1. **Nguồn dữ liệu**: recipe Published, sắp xếp `PublishedAt DESC`, tối đa
   20–50 item.
2. **Endpoint**: `GET /rss.xml` (hoặc `/feed.xml`) trả `application/rss+xml;
   charset=utf-8`.
   - Có thể làm ở backend (`MapGet("/rss.xml")`) hoặc Next.js route handler
     `app/rss.xml/route.ts`.
3. **Nội dung**: `<channel>` (title, link, description, language `vi`),
   `<item>` (title, link, guid, pubDate RFC 822, description, category,
   `<enclosure url="..." type="image/jpeg">`).
4. **Cache**: `Cache-Control: public, max-age=600` + ETag; regenerate khi có
   recipe publish (tag cache).
5. **SEO**: thêm `<link rel="alternate" type="application/rss+xml"
   href="/rss.xml">` trong layout metadata.
6. **Bảo mật**: escape XML (`SecurityElement.Escape`) để tránh XML injection;
   giới hạn kích thước description.

**Ưu:** đơn giản, chuẩn, tăng phân phối nội dung; không cần DB mới.
**Nhược:** ít người dùng RSS hiện đại; cần escape đúng; không real-time nếu cache.

---

## III.4. Chia sẻ bài viết lên mạng xã hội

**Thiết kế**
1. **Frontend**: component `ShareButtons` (client) với:
   - Web Share API (`navigator.share`) cho mobile.
   - Fallback link:
     - Facebook: `https://www.facebook.com/sharer/sharer.php?u={url}`
     - X/Twitter: `https://twitter.com/intent/tweet?url={url}&text={title}`
     - LinkedIn: `https://www.linkedin.com/sharing/share-offsite/?url={url}`
     - Pinterest: `https://pinterest.com/pin/create/button/?url={url}&media={image}`
   - Nút “Copy link” dùng `navigator.clipboard`.
2. **Metadata**: Open Graph (`og:title`, `og:description`, `og:image`,
   `og:url`, `og:type=article`), Twitter Card
   (`summary_large_image`), canonical URL.
3. **Ảnh OG động**: Next.js `opengraph-image.tsx` dùng `ImageResponse` để tạo
   ảnh có tiêu đề/ảnh recipe.
4. **Tracking**: `POST /api/v1/recipes/{id}/share` `{ channel }` (rate limit)
   để thống kê.
5. **Bảo mật**: URL encode, chỉ cho phép domain tin cậy, `rel="noopener
   noreferrer"`.

**Ưu:** không cần backend phức tạp; tăng traffic; OG image đẹp.
**Nhược:** phụ thuộc API mạng xã hội (có thể đổi), popup blocker; cần ảnh OG
chuẩn kích thước.

---

## III.5. Đánh giá 1–5 sao công thức

**Thiết kế**
1. **Database** `RecipeRatings`:
   - `Id`, `RecipeId` FK, `UserId` FK, `Stars` smallint CHECK 1..5,
     `CreatedAt`, `UpdatedAt`.
   - Unique index `(RecipeId, UserId)` — mỗi user một đánh giá.
2. **Aggregate**: thêm `Recipe.AverageRating decimal(3,2)`, `RatingCount int`
   (denormalized) để đọc nhanh; cập nhật trong transaction khi thêm/sửa/xóa,
   hoặc job tổng hợp định kỳ.
3. **API**:
   - `POST /api/v1/recipes/{id}/ratings` `{ stars }` (auth) — tạo/cập nhật.
   - `DELETE /api/v1/recipes/{id}/ratings` (auth) — xóa đánh giá của mình.
   - `GET /api/v1/recipes/{id}/ratings?page=&pageSize=` (public) — danh sách +
     phân bố sao.
   - `GET /recipes/{slug}` trả thêm `averageRating`, `ratingCount`,
     `myRating` (nếu đã đăng nhập).
4. **Quy tắc**: không cho tự đánh giá recipe của mình (tùy chọn); rate limit;
   chống spam (một user/recipe).
5. **Frontend**: component `StarRating` (hover/click), optimistic update,
   hiển thị trung bình + số lượt; aria-label cho accessibility.
6. **Concurrency**: dùng transaction + `UPDATE ... SET RatingCount = ...` hoặc
   tính lại từ bảng rating trong job để tránh sai lệch.

**Ưu:** tăng tương tác, dữ liệu gợi ý; denormalized đọc nhanh.
**Nhược:** cần đồng bộ aggregate; dễ bị review bomb → cần moderation/rate limit.

---

## III.6. Module bình luận công thức

**Thiết kế**
1. **Database** `Comments`:
   - `Id`, `RecipeId` FK, `UserId` FK, `ParentId?` (reply, self-FK),
     `Content` (giới hạn 2000 ký tự), `Status`
     (Visible/Hidden/Deleted), `CreatedAt`, `UpdatedAt`, `IsDeleted`.
   - Index `(RecipeId, CreatedAt)`; giới hạn depth 2 (reply của reply).
2. **API**:
   - `GET /api/v1/recipes/{id}/comments?cursor=&pageSize=` (public) — cursor
     pagination phù hợp feed.
   - `POST /api/v1/recipes/{id}/comments` `{ content, parentId? }` (auth).
   - `PATCH /api/v1/comments/{id}` `{ content }` (owner, trong 15 phút).
   - `DELETE /api/v1/comments/{id}` (owner/Admin) — soft delete.
   - `PATCH /api/v1/comments/{id}/hide` (Admin) — moderation.
3. **Bảo mật**:
   - Auth required để viết; rate limit theo user/IP.
   - Sanitize/escape HTML; nếu cho markdown thì dùng thư viện sanitize.
   - Kiểm tra ownership (resource-based authorization).
   - Lọc spam/profanity; CAPTCHA cho guest (nếu cho phép).
   - Không render raw HTML từ user.
4. **Thông báo**: khi có reply → job gửi email/in-app notification cho
   người được reply.
5. **Frontend**: list + form + reply, optimistic update, phân trang “Xem thêm”,
   báo cáo vi phạm.

**Ưu:** tăng engagement/SEO (nội dung mới); moderation linh hoạt.
**Nhược:** phức tạp moderation, spam, XSS; cần pagination/threading cẩn thận.

---

## III.7. Trang Liên hệ + form liên hệ

**Thiết kế**
1. **Frontend** `/contact`:
   - Thông tin cửa hàng/công ty: địa chỉ, điện thoại, email, giờ mở cửa, bản
     đồ (Google Maps embed), link mạng xã hội.
   - Form: `name`, `email`, `subject`, `message`, `honeypot` (ẩn), captcha.
2. **Backend**:
   - `ContactMessages` (`Id`, `Name`, `Email`, `Subject`, `Message`,
     `Status` New/Read/Replied, `CreatedAt`, `IpHash`, `UserAgent`).
   - `POST /api/v1/contact` → validate (FluentValidation), rate limit, lưu DB,
     enqueue job gửi email cho admin + auto-reply cho người gửi.
   - Admin: `GET /api/v1/admin/contact-messages?status=`, `PATCH .../read`.
3. **Bảo mật**: honeypot + CAPTCHA (Cloudflare Turnstile/reCAPTCHA), rate
   limit, giới hạn độ dài, escape HTML, không log nội dung nhạy cảm, kiểm tra
   email header injection, SPF/DKIM.
4. **UX**: thông báo thành công/thất bại, disable nút khi submit, accessible
   label.

**Ưu:** kênh liên hệ chính thức, lưu vết, chống spam.
**Nhược:** cần cấu hình email/CAPTCHA; form dễ bị spam nếu không bảo vệ.

---

## III.8. Tính năng Câu hỏi thường gặp (FAQ)

**Thiết kế**
1. **Database** `FaqItems` (`Id`, `Question`, `Answer`, `Category`,
   `OrderIndex`, `IsPublished`, `CreatedAt`, `UpdatedAt`) hoặc static JSON/MDX
   nếu nội dung ít.
2. **API**:
   - Public: `GET /api/v1/faqs?category=` (cache 1 giờ).
   - Admin: CRUD + reorder + publish/unpublish.
3. **Frontend** `/faq`:
   - Accordion (Radix/shadcn), search/filter theo category.
   - SEO: `FAQPage` JSON-LD (`Question`/`AcceptedAnswer`) để hiển thị rich
     result.
4. **Cache**: Output Cache/Redis; invalidate khi admin sửa.
5. **Validation**: câu hỏi/trả lời không rỗng, giới hạn độ dài, escape HTML.

**Ưu:** giảm tải hỗ trợ, tốt SEO; admin tự quản lý.
**Nhược:** nội dung dễ lỗi thời; JSON-LD phải đúng schema.

---

## III.9. Rủi ro bảo mật của form Liên hệ/Bình luận và giải pháp

| Rủi ro | Mô tả | Giải pháp |
| :--- | :--- | :--- |
| **XSS** | User chèn `<script>` vào comment/contact, trình duyệt thực thi | Escape output (React tự escape), sanitize HTML nếu cho phép markdown, CSP chặt, không dùng `dangerouslySetInnerHTML` với dữ liệu user. |
| **SQL Injection** | Nối chuỗi SQL từ input | Dùng EF Core/parameterized query (dự án dùng EF + `SqlQuery` tham số hóa), không nối chuỗi. |
| **CSRF** | Form tự động gửi bằng cookie nạn nhân | SameSite=Lax/Strict, CSRF token, kiểm tra Origin/Referer; với Bearer JWT thì ít bị hơn nhưng vẫn cần. |
| **Spam/Bot** | Gửi hàng loạt comment/contact | CAPTCHA (Turnstile/reCAPTCHA), honeypot, rate limit theo IP/user, moderation queue, Akismet. |
| **DoS** | Payload lớn/lặp request | Giới hạn độ dài, `client_max_body_size`, rate limiting, timeout, queue. |
| **Email Header Injection** | Chèn `\r\n` vào subject/email | Validate email, strip CR/LF, dùng API email an toàn. |
| **IDOR** | Sửa `commentId`/`messageId` của người khác | Resource-based authorization (owner/Admin), không dựa vào id đoán được. |
| **PII Leak** | Lộ email/IP trong log/response | Không trả email công khai, hash IP, mask log, retention policy. |
| **File Upload (nếu có)** | Upload file độc hại | Validate MIME + magic bytes + size (đã có `ImageValidation`), lưu ngoài webroot, quét virus. |
| **Enumeration** | Dò email/tài khoản qua thông báo lỗi | Thông báo lỗi trung tính, rate limit, không tiết lộ tồn tại. |
| **Thiếu audit** | Không truy vết khi bị lạm dụng | Audit log + correlation id + alert bất thường. |

**Nguyên tắc chung:** validate ở cả client và server; không tin client;
least privilege; defense in depth; log/monitor; cập nhật dependency; HTTPS;
security headers (CSP, HSTS, X-Content-Type-Options, Referrer-Policy).

---

# Phụ lục — Bảng đối chiếu nhanh SRS ↔ mã nguồn

| Yêu cầu | Nơi cài đặt |
| :--- | :--- |
| JWT 15 phút + refresh 7 ngày | `JwtTokenService.cs`, `JwtOptions.cs`, `AuthEndpoints.cs` |
| Refresh rotation + reuse detection | `RefreshTokenCommandHandler.cs` |
| Phân quyền Admin/Owner | `RecipeEndpoints.cs`, `UpdateRecipeCommand.cs`, `CategoryEndpoints.cs` |
| Offset pagination | `GetRecipesQuery.cs`, `CategoryEndpoints.cs` |
| FTS `'simple'` + `unaccent` + `ts_rank` | `RecipeEndpoints.cs`, migration `RecipeSearchSimpleConfig` |
| Redis cache danh mục | `Program.cs`, `CategoryEndpoints.cs` |
| OpenTelemetry | `Program.cs`, `FoodBlogMetrics.cs` |
| Health checks | `HealthEndpoints.cs` |
| Serilog + CorrelationId | `Program.cs`, `CorrelationIdMiddleware.cs` |
| MinIO + thumbnail | `MinioFileStorageService.cs`, `ImageJobWorker.cs`, `ImageSharpResizer.cs` |
| Docker + Nginx | `src/FoodBlog.API/Dockerfile`, `frontend/Dockerfile`, `nginx/`, `docker-compose.yml` |
| Sitemap/robots | `frontend/app/sitemap.ts`, `frontend/app/robots.ts` |
| Tests | `tests/FoodBlog.Tests` (75 test) |

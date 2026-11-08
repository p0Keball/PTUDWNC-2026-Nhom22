# Changelog

Mọi thay đổi đáng chú ý của dự án được ghi tại đây.
Định dạng dựa trên [Keep a Changelog](https://keepachangelog.com/vi/1.1.0/).

## [Unreleased]

### Added
- Trang `/categories` (ISR 3600) và `/categories/[slug]` (ISR 600) dùng
  `GET /categories/{slug}`; sidebar danh mục trỏ về URL semantic.
- Trang `/search` (SSR, full-text search + phân trang) và `/dashboard/categories`
  (Admin CRUD danh mục) theo bảng route UI 5.1.
- Middleware rate limit theo IP trả `X-RateLimit-Limit`, `X-RateLimit-Remaining`,
  `X-RateLimit-Reset`, `Retry-After` (NFR-SEC-003 / 5.2).
- JWT Bearer authentication (HS256, access token 15 phút) song song Identity cookie
  qua policy scheme `"Smart"`; refresh token 7 ngày có rotation + reuse detection.
- Endpoint đăng ký trả `201 Created`; kiểm tra trùng email/username trả `409`
  (`AUTH_EMAIL_EXISTS`, `AUTH_USERNAME_EXISTS`).
- Phân quyền hiển thị công thức: Guest chỉ thấy Published; Author thấy thêm
  Draft/Archived của mình; Admin thấy tất cả (áp dụng cho danh sách, chi tiết
  và danh mục).
- `GET /api/v1/recipes` hỗ trợ `minServings` và trả metadata phân trang đầy đủ.
- Chi tiết công thức trả kèm `category` và `author` summary.
- Tạo công thức hỗ trợ tuỳ chọn `steps`, `ingredients`, `nutrition` trong cùng request.
- Redis `IDistributedCache` cho danh mục (fallback in-memory khi chưa cấu hình).
- OpenTelemetry tracing/metrics (AspNetCore, HttpClient, EF Core) + custom metrics
  `foodblog.recipes.created`, `foodblog.recipes.published`.
- `WelcomeEmailJob` retry 3 lần với backoff 1m/5m/30m (NFR-REL-002).
- `frontend/app/sitemap.ts` và `frontend/app/robots.ts` (NFR-SEO-003).
- Dockerfile multi-stage cho API và frontend (standalone), Nginx reverse proxy,
  Docker Compose đầy đủ `api`/`frontend`/`nginx`/`postgres`/`redis`/`minio`/`seq`.
- `.editorconfig`, `ARCHITECTURE.md`, `CHANGELOG.md`, `docs/on-tap-cuoi-ky.md`.

### Changed
- Rate limiting chuyển từ `UseRateLimiter()` sang middleware fixed-window theo IP
  để trả đầy đủ header cho cả response thành công lẫn response 429.
- Full-text search dùng cấu hình `'simple'` + `unaccent` (thay `'english'`),
  xếp hạng bằng `ts_rank`; thêm migration `RecipeSearchSimpleConfig`.
- Frontend profile page đọc access token từ Zustand auth store thay vì
  `localStorage.accessToken`; `AuthProvider` nối `setAccessTokenProvider`.
- Output Cache của danh sách công thức vary theo `Authorization`/`Cookie`.

### Fixed
- Rò rỉ Draft/Archived qua cache công khai.
- Đăng xuất chỉ thu hồi đúng refresh token hiện tại thay vì toàn bộ phiên.

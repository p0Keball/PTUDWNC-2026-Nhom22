# Kiến trúc FoodBlog (Culinary Blog)

Tài liệu này mô tả kiến trúc thực tế của mã nguồn, đối chiếu với SRS
(`SRS_culinary_blog.md`) và các quyết định triển khai.

## 1. Tổng quan

```
Browser / Next.js (frontend)
        │  HTTPS (Nginx reverse proxy)
        ▼
ASP.NET Core 10 Minimal API (FoodBlog.API)
        │  MediatR (CQRS) + FluentValidation
        ▼
FoodBlog.Application  ──►  FoodBlog.Domain
        ▲                        (entities, enums, exceptions)
        │  interfaces
FoodBlog.Infrastructure
  EF Core 10 + PostgreSQL 16 (unaccent, FTS 'simple')
  Redis (IDistributedCache) · MinIO (S3) · Hangfire · JWT
```

## 2. Các tầng và Dependency Rule

| Tầng | Vai trò | Phụ thuộc |
| :--- | :--- | :--- |
| `FoodBlog.Domain` | Entity, enum, domain exception, `BaseEntity` | Không phụ thuộc project khác |
| `FoodBlog.Application` | Use case (CQRS command/query), DTO, validator, interface hạ tầng | Domain |
| `FoodBlog.Infrastructure` | EF Core, repository, MinIO, JWT, Hangfire job, email | Application, Domain |
| `FoodBlog.API` | Endpoint, middleware, DI, cấu hình | Application, Infrastructure |
| `frontend` | Next.js 16 App Router (SSR/ISR), Zustand auth store | REST API |

Dependency Rule: mũi tên chỉ hướng vào trong. `Application` chỉ biết interface
(`IFoodBlogDbContext`, `IJwtTokenService`, `IFileStorageService`, ...), không
import `Infrastructure`. `Program.cs` (API) là composition root nối implementation.

> **Ghi chú trung thực:** `ApplicationUser : IdentityUser` nằm trong Domain để
> EF Core/Identity dùng chung, nên Domain có tham chiếu package
> `Microsoft.Extensions.Identity.Stores`. Đây là điểm lệch nhỏ so với lý thuyết
> "Domain 0 external dependency" và được ghi nhận trong báo cáo ôn tập.

## 3. Luồng xác thực

- Đăng ký/đăng nhập trả về **JWT access token** (HS256, TTL 15 phút) +
  **refresh token** (7 ngày, lưu hash SHA-256, rotation + reuse detection).
- `Program.cs` đăng ký policy scheme `"Smart"`:
  - Có header `Authorization` → xác thực `JwtBearer`.
  - Không có → fallback Identity cookie (giữ tương thích luồng cũ).
- Logout thu hồi đúng refresh token client gửi lên (hoặc toàn bộ nếu client cũ).

## 4. Dữ liệu & tìm kiếm

- Soft delete + Global Query Filter (`IsDeleted`).
- Optimistic concurrency bằng `RowVersion` (`bytea`).
- Full-text search: `tsvector` sinh bởi trigger PostgreSQL, cấu hình `'simple'`
  + `unaccent(...)` phù hợp tiếng Việt; truy vấn `websearch_to_tsquery` và
  xếp hạng `ts_rank`.
- Ảnh: upload lên MinIO, Hangfire job tạo thumbnail (ImageSharp).

## 5. Cache & quan sát

- `GET /api/v1/recipes` dùng Output Cache, vary theo `Authorization`/`Cookie`
  để không rò rỉ Draft/Archived.
- Danh mục dùng `IDistributedCache` (Redis khi có cấu hình, fallback in-memory).
- Serilog (Console + File + Seq) + OpenTelemetry (AspNetCore/HttpClient/EF Core
  traces, custom metrics `foodblog.recipes.created|published`).
- Rate limit fixed-window theo IP (`general` 100/phút, `auth` 10/phút,
  `upload` 5/phút, `recipe-write` 30/phút) kèm header `X-RateLimit-*` và
  `Retry-After`.
- Health check tại `/health`.

## 6. Frontend

Next.js App Router: `/`, `/recipes`, `/recipes/[slug]`, `/categories`,
`/categories/[slug]`, `/search`, `/auth/*`, `/dashboard/*`, `/profile`.
`/categories` và `/recipes/[slug]` dùng ISR; `/search` SSR; dashboard/profile
là CSR có auth gate. Sitemap/robots sinh bằng metadata route.

## 7. Triển khai

`docker-compose.yml` gồm: `postgres`, `redis`, `minio`, `seq`, `api`,
`frontend`, `nginx`. Nginx reverse proxy `/api`, `/hangfire`, `/health`,
`/scalar`, `/openapi` về API và `/` về Next.js. Frontend build standalone
(`output: "standalone"`).

## 8. Kiểm thử

- `tests/FoodBlog.Tests`: unit + integration test với `WebApplicationFactory`,
  EF Core InMemory, `TestAuthHandler` thay xác thực thật.
- Chạy: `dotnet test FoodBlog.slnx`.

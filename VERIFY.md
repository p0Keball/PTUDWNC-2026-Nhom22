# VERIFY — Bản đồ project + cách chạy chi tiết 

> Yêu cầu máy: .NET SDK `10.0.300+`, Node `20+`, Docker Desktop, Git.

## 1. Bản đồ

```
docker-compose.yml          # 4 service: Postgres, MinIO, Redis, Seq
FoodBlog.slnx               # Solution .NET 10 (định dạng .slnx mới, KHÔNG phải .sln)
src/FoodBlog.Domain/        # Entities (Category, Recipe, Step, Ingredient, Image,
                            #   ApplicationUser, RefreshToken) + BaseEntity + SlugHelper
src/FoodBlog.Application/   # (trống — dành cho MediatR/Validation Tuần 2)
src/FoodBlog.Infrastructure/# Configurations, FoodBlogDbContext, Migrations, Seed (Bogus)
src/FoodBlog.API/           # Program.cs + Endpoints/ (Category, Recipe, RecipeChild) + appsettings.json
frontend/                   # Next.js App Router template (chưa có trang blog)
README.md                   # Phân công TV1–TV4
KEHOACH.md / HOC.md         # Ghi chú cá nhân (đang gitignore, chỉ nằm trên máy)
CHECKLIST.md                # Checklist endpoints + FAILURE fixes (TV2, đã check xong)
FAILURE.md                  # 17 mâu thuẫn spec F-01–F-17
Plan_Frontend_srs_culinary_blog.md  # SRS v1.0.0 — nguồn chuẩn API/DB
```

## 2. Chạy hạ tầng (lần đầu + mỗi ngày mở máy)

```bash
# 1. Mở Docker Desktop trước, rồi:
docker compose up -d
docker compose ps
# Đúng = postgres/minio/redis Up. Seq Restarting trên Mac là bình thường (nợ kỹ thuật).
```

| Service | Cổng | Tài khoản |
|---|---|---|
| Postgres 16 | `5432` | db/user/pass: `foodblog` / `foodblog` / `foodblog123` |
| MinIO API / Console | `9000` / `9001` | `minioadmin` / `minioadmin123` |
| Redis 7 | `6379` | (không pass) |
| Seq UI / ingest | `1880` / `5341` | (có thể chưa vào được) |

> Lưu ý: image MinIO dùng `quay.io/minio/minio:latest` (Docker Hub lỗi `access denied`).

## 3. Chạy backend API

```bash
dotnet build FoodBlog.slnx   # build cả solution (0 error mới đúng)
dotnet run --project src/FoodBlog.API --urls "http://localhost:5000"
# Lần chạy đầu: tự Migrate + Seed (20 categories, 100 recipes,
# ~1100 ingredients, ~650 steps, 199 images, 6 users). Chạy lại: tự skip seed.
```

- Health check: `curl http://localhost:5000/health` → `"Healthy"`
- Tài khoản seed: admin `admin@foodblog.local` / `Foodblog123!` (+ 5 authors, pass giống nhau)
- Connection string: `src/FoodBlog.API/appsettings.json` → `ConnectionStrings:Default`

## 4. Endpoints hiện có (base `http://localhost:5000`)

### Category (FR-CAT-001→005)
| Method | Endpoint | Ghi chú |
|---|---|---|
| GET | `/api/v1/categories` | IMemoryCache 60p, kèm `recipeCount` |
| GET | `/api/v1/categories/{slug}?page=&pageSize=` | 404 nếu sai slug |
| POST | `/api/v1/categories` `{name, description?, imageUrl?}` | 201 + Location; trùng slug → auto-suffix; trùng tên → 409; sai → 422 |
| PUT | `/api/v1/categories/{id}` | slug KHÔNG đổi (F-14) |
| DELETE | `/api/v1/categories/{id}` | còn recipe → 409; rỗng → 204 |

### Recipe (FR-RCP)
| Method | Endpoint | Ghi chú |
|---|---|---|
| GET | `/api/v1/recipes?page=&pageSize=&categoryId=&difficulty=&maxCookTime=&sort=` | chỉ Published, cache 15p |
| GET | `/api/v1/recipes/search?q=` | FTS unaccent ("pho" tìm được "Phở"); thiếu `q` → 422 |
| GET | `/api/v1/recipes/{slug}` | Draft/Archived → 403 |
| POST | `/api/v1/recipes` | luôn tạo Draft → 201 |
| PUT | `/api/v1/recipes/{id}` | body bắt buộc `rowVersion` (base64); cũ → 422 `RECIPE_CONCURRENCY_CONFLICT` |
| PATCH | `/api/v1/recipes/{id}/publish` | cần ≥1 step, không thì 422 |
| PATCH | `/api/v1/recipes/{id}/unpublish`, `/archive` | đổi trạng thái |
| DELETE | `/api/v1/recipes/{id}` | **xóa mềm** (`IsDeleted=true`), row còn trong DB → 204 |

### Steps / Ingredients / Images của recipe
| Method | Endpoint | Ghi chú |
|---|---|---|
| POST/PUT/DELETE | `/api/v1/recipes/{id}/steps[/{stepId}]` | xóa giữa → tự renumber 1,2,3…; field `TimerMinutes` |
| POST/PUT/DELETE | `/api/v1/recipes/{id}/ingredients[/{ingId}]` | field `OrderIndex` |
| POST | `/api/v1/recipes/{id}/images` `{originalUrl, altText?}` | URL string (MinIO SDK để sau); ảnh đầu `IsPrimary`; trả `thumbnailStatus: pending/ready` |
| PATCH | `/api/v1/recipes/{id}/images/{imgId}/primary` | đặt ảnh chính |
| DELETE | `/api/v1/recipes/{id}/images/{imgId}` | xóa primary → tự gán ảnh tiếp theo |

Ví dụ nhanh:
```bash
curl -s "http://localhost:5000/api/v1/recipes?page=1&pageSize=2" | head -c 300
curl -s "http://localhost:5000/api/v1/recipes/search?q=banh" | head -c 300
```

## 5. Kiểm tra dữ liệu trong DB

```bash
docker exec foodblog-postgres psql -U foodblog -d foodblog -c \
"SELECT 'Categories', COUNT(*) FROM \"Categories\" UNION ALL \
 SELECT 'Recipes', COUNT(*) FROM \"Recipes\" UNION ALL \
 SELECT 'Ingredients', COUNT(*) FROM \"RecipeIngredients\" UNION ALL \
 SELECT 'Steps', COUNT(*) FROM \"RecipeSteps\";"
# Đúng = 20 / 100 / ~1099 / ~650
```

## 6. Chạy frontend (template, chưa nối API)

```bash
cd frontend && npm run dev     # http://localhost:3000
```

## 7. Tạo migration mới (khi đổi Entity/Config)

```bash
# Cần tool dotnet-ef (cài 1 lần): dotnet tool install --global dotnet-ef
dotnet ef migrations add TenMigration \
  --project src/FoodBlog.Infrastructure --startup-project src/FoodBlog.API
# Không cần database update tay — API tự Migrate khi start.
```

## 8. Lỗi hay gặp

| Hiện tượng | Nguyên nhân / cách fix |
|---|---|
| `dotnet build FoodBlog.sln` lỗi MSB1009 | .NET 10 dùng `.slnx`: build `dotnet build FoodBlog.slnx` |
| `dotnet ef` not found | cài `dotnet tool install --global dotnet-ef` |
| API crash lúc start (Migrate) | Postgres chưa chạy → `docker compose up -d` trước |
| `docker ps` trống | Docker Desktop chưa bật hoặc containers stop |
| MinIO pull `access denied` | đã fix: dùng image `quay.io/...` trong compose |
| Seq `Restarting` | nợ kỹ thuật Mac ARM, không chặn các service khác |
| `bin/`, `obj/` hiện lung tung | bình thường — đã gitignore, đừng commit |

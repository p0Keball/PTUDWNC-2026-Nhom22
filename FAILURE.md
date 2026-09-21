# FAILURE ANALYSIS - SRS Culinary Blog v1.0.0

> **Nguồn phân tích:** `Plan_Frontend_srs_culinary_blog.md` (1325 dòng, convert từ PDF) + `SRS_Culinary_Blog_v1.0.0.pdf`
> **Ngày phân tích:** 21/09/2026
> **Repo:** `PTUDWNC-2026-Nhom22` - Đề tài Blog Ẩm thực và Nấu ăn
> **Khái niệm:** failure = spec/code vẫn chạy được lúc đầu, nhưng về lâu dài gây lỗi lớn (bảo mật, mất dữ liệu, phình DB, stale cache, không implement được).

---

## NHÓM CRITICAL - Bảo mật / leak dữ liệu

### F-01. RefreshToken lưu plain vs hash + 512-bit vs 256-bit
- **Vị trí:** `FR-AUTH-001 bước 9-10`, `FR-AUTH-002 bước 8-9`, `FR-AUTH-004` vs `NFR-SEC-002` vs `7.8 RefreshTokens.TokenHash`
- **Mâu thuẫn:**
  - `NFR-SEC-002` ghi: `Refresh Token (256-bit random, SHA-256 hash in DB, TTL 7d, Rotation + Reuse Detection)`.
  - `FR-AUTH-001 bước 9` ghi: `tạo refresh token ngẫu nhiên (512-bit, 7 ngày)`.
  - `FR-AUTH-001 bước 10 / FR-AUTH-002 / FR-AUTH-004` ghi: `Lưu Refresh Token vào bảng refresh_tokens` (hiểu là lưu plain).
  - `7.8` lại định nghĩa cột `TokenHash varchar(64) UNIQUE` (đúng phải lưu hash).
- **Hậu quả dài hạn:** DB lộ là mất toàn bộ session, attacker dùng lại token. Không thống nhất độ dài thì dev đoán mò.
- **Hướng sửa:** Thống nhất `256-bit random`, chỉ lưu `SHA256(token)`, so sánh bằng hash. Sửa FR-AUTH ghi rõ `lưu TokenHash, trả plain 1 lần cho client`.

### F-02. Schema không có `IsRevoked` nhưng code dùng
- **Vị trí:** `FR-AUTH-004 bước 4-5`, `FR-AUTH-005 bước 4` vs `7.8 RefreshTokens`
- **Mâu thuẫn:** FR ghi `kiểm tra IsRevoked == false`, `đánh dấu IsRevoked = true`, nhưng bảng `7.8` chỉ có `RevokedAt, ReplacedByTokenHash, ExpiresAt`, không có cột `IsRevoked`.
- **Hậu quả:** Lệch migration, hoặc check sai -> reuse-attack lọt, logout không có tác dụng.
- **Hướng sửa:** Bỏ `IsRevoked`, quy ước `RevokedAt IS NULL = còn sống`. Sửa FR-AUTH-004/005 theo cột thật.

### F-03. Cache danh sách recipe gây leak Draft
- **Vị trí:** `FR-RCP-001` (OutputCache `RecipeList` TTL 15p, vary by query string) vs rule phân quyền trong cùng FR
- **Mâu thuẫn:** Response khác nhau theo user (Guest chỉ Published, Author thêm Draft của mình, Admin thấy tất cả) nhưng cache chỉ vary theo query string.
- **Hậu quả:** Author A trúng cache của Author B, thấy Draft người khác. Lỗi bảo mật + khó reproduce.
- **Hướng sửa:** `VaryByHeader Authorization` hoặc không cache endpoint có auth tùy biến, chỉ cache public Published. Ghi rõ trong FR.

### F-04. Google login trust-boundary sai
- **Vị trí:** `FR-AUTH-003 bước 4-5` vs `8.1 Authentication Module`
- **Mâu thuẫn:** FR ghi `Auth.js v5 ở Next.js lấy profile rồi Frontend gửi GoogleExternalLoginInfo lên Backend`, còn bảng `8.1` ghi body là `{idToken}`.
- **Hậu quả:** Nếu Backend tin profile Frontend gửi là bypass auth, tạo tài khoản giả.
- **Hướng sửa:** Backend phải tự verify `idToken` với Google (OpenID Connect), không tin Frontend. Thống nhất contract `POST /auth/google {idToken}`.

### F-05. Logging lộ secret
- **Vị trí:** `6.3 LoggingBehavior log request parameters` + `CONS-010` + `FR-OBS-002`
- **Mâu thuẫn:** Yêu cầu log mọi request/parameters nhưng không loại trừ `password, refreshToken`.
- **Hậu quả:** Secret chui vào Seq/Console/File log, vi phạm `NFR-SEC-007`, lộ khi share log.
- **Hướng sửa:** Redact field `[Sensitive]`, chỉ log `email, userId`, không log body auth.

---

## NHÓM HIGH - Data integrity / phình DB / stale

### F-06. Soft-delete vs hard-delete mâu thuẫn
- **Vị trí:** `7.1 BaseEntity (IsDeleted + Global Query Filter)` vs `FR-RCP-007 xóa vĩnh viễn cascade` vs `FR-CAT-005 xóa hẳn`
- **Mâu thuẫn:** Có cột `IsDeleted` nhưng FR recipe/category lại xóa hẳn vật lý.
- **Hậu quả:** Hoặc `IsDeleted` chết không bao giờ dùng, hoặc dev quên filter -> xóa mềm nhưng API vẫn trả về / mất dữ liệu vĩnh viễn khi muốn giữ.
- **Hướng sửa:** Chọn 1: Recipe hard-delete thì bỏ `IsDeleted` khỏi spec cho Recipe, hoặc chuyển sang soft-delete + job purge + sửa FR-RCP-007.

### F-07. RefreshTokens phình vô hạn, thiếu job purge
- **Vị trí:** `FR-AUTH-002/004` (mỗi login/refresh tạo 1 row, giữ row cũ để detect reuse) vs `3.6 FR-JOB` (chỉ có 3 job, không có purge)
- **Hậu quả:** Với scale `2.6.1: <=5000 users`, bảng tăng vô hạn, query refresh chậm dần, backup nặng.
- **Hướng sửa:** Thêm `FR-JOB-004 purge token hết hạn/revoked >30 ngày`, chạy daily.

### F-08. `SearchVector` không bao giờ được update
- **Vị trí:** `7.2 SearchVector tsvector GIN` vs `FR-RCP-003/004` vs `FR-SRCH-001`
- **Mâu thuẫn:** Có cột FTS + extension `unaccent, pg_trgm` nhưng không FR nào mô tả trigger/update `tsvector` khi đổi Title/Description.
- **Hậu quả:** Search rỗng/stale mãi, user tưởng search hỏng.
- **Hướng sửa:** Thêm trigger `tsvector_update_trigger` hoặc update trong `Create/Update Handler`, ghi vào FR.

### F-09. RowVersion không dùng được vì API không gửi
- **Vị trí:** `FR-RCP-004 concurrency qua RowVersion` + `Phụ lục A 422 RowVersion mismatch` vs `8.3 PUT /recipes/{id}`
- **Mâu thuẫn:** Body `PUT` không có `rowVersion`, không dùng `If-Match`.
- **Hậu quả:** Client không có gì để gửi -> mất update chéo (lost update), mã 409/422 unreachable.
- **Hướng sửa:** Thêm `rowVersion: string (base64)` vào DTO request/response `PUT`.

### F-10. Ảnh primary mồ côi + thumbnail race
- **Vị trí:** `FR-RCP-008 (ảnh đầu IsPrimary=true)` vs `FR-JOB-002 (tạo 300x300 + 800x600 async)` vs `7.5 RecipeImages`
- **Mâu thuẫn:** Không quy định khi xóa ảnh primary thì ai thay thế. `POST images 201` trả về ngay trong khi `MediumUrl/ThumbnailUrl NULL` một lúc.
- **Hậu quả:** Recipe không có primary -> UI vỡ, LCP xấu. Frontend không biết poll thumbnail.
- **Hướng sửa:** Quy tắc reassign primary khi delete + contract `thumbnailStatus: pending/ready` hoặc trả placeholder.

---

## NHÓM MEDIUM - Contract lệch, NFR không làm được

### F-11. `FullName` vs `DisplayName`
- **Vị trí:** `FR-AUTH-001/006/007 dùng fullName` vs `7.7 ApplicationUser chỉ có DisplayName, Bio, AvatarUrl`
- **Hậu quả:** Map sai tên field, lỗi runtime hoặc migration lệch.
- **Hướng sửa:** Thống nhất `DisplayName`, sửa FR-AUTH.

### F-12. `TimerMinutes` vs `DurationMinutes`, `SortOrder` vs `OrderIndex`
- **Vị trí:** `7.3 TimerMinutes` vs `FR-RCP-010 DurationMinutes` vs `8.5 timerMinutes`; `FR-RCP-002 SortOrder` vs `7.4 OrderIndex`
- **Hậu quả:** Mỗi dev hiểu 1 kiểu, seed + API lệch nhau (đúng bảng phân công Tuần 1: nhóm 4 làm Step/Ingredient/Image rất dễ dính).
- **Hướng sửa:** Thống nhất `TimerMinutes` + `OrderIndex`, sửa FR + 8.5/8.6.

### F-13. Policy `VerifiedAuthor` chết
- **Vị trí:** `2.3 phân quyền tầng 3: Policy VerifiedAuthor yêu cầu email đã xác nhận` vs `FR-AUTH-001..007`
- **Mâu thuẫn:** Không có endpoint confirm-email nào, chỉ có welcome-email `FR-JOB-001`.
- **Hậu quả:** Tất cả Author bị 403 mãi nếu bật policy.
- **Hướng sửa:** Hoặc bỏ policy, hoặc thêm `FR-AUTH-008 confirm-email + check EmailConfirmed`.

### F-14. Slug bất biến vs redirect 301
- **Vị trí:** `FR-CAT-004 Slug KHÔNG đổi khi đổi tên` vs `NFR-SEO-004 301 khi slug thay đổi` vs `5.1 /recipes/{slug}`
- **Mâu thuẫn:** Không có bảng `OldSlugs`, không FR nào làm redirect.
- **Hậu quả:** NFR không implement được, đổi title recipe là gãy SEO.
- **Hướng sửa:** Giữ slug bất biến cho Category, thêm bảng redirect cho Recipe nếu cho đổi slug.

### F-15. 3 cơ chế cache chồng nhau
- **Vị trí:** `FR-CAT-001 IMemoryCache 60p` + `FR-RCP-001 OutputCache 15p` + `6.3 CachingBehavior Redis`
- **Mâu thuẫn:** `IMemoryCache` vỡ khi scale horizontal (`NFR-SCALE-001 stateless`), invalidate chỗ này không xóa chỗ kia. `NFR-PERF-003 hit rate Redis >=80%` không đo được.
- **Hướng sửa:** Thống nhất Redis Distributed Cache + tag invalidation (`recipes`, `categories:all`).

### F-16. Rate-limit theo IP sau Nginx
- **Vị trí:** `NFR-SEC-003 (Auth 10 req/min/IP, Upload 5/min/IP)` vs `6.1 Nginx Reverse Proxy`
- **Mâu thuẫn:** Không forward `X-Forwarded-For` thì mọi user chung 1 IP.
- **Hậu quả:** Tự DDoS chính mình hoặc bypass limit.
- **Hướng sửa:** Cấu hình `ForwardedHeadersOptions` + test sau Nginx.

### F-17. Rác convert PDF->MD + status code mâu thuẫn
- **Vị trí:** `1.3 DXA: Device-independent pixel... OOXML`, `FR-CAT-003 409`, `FR-RCP-003 A3 409`, `Phụ lục A/B`
- **Mâu thuẫn:** DXA không liên quan blog. `FR-CAT-003` vừa auto-suffix `-2,-3` vừa khai báo `409 Conflict` -> 409 không bao giờ xảy ra. `VALIDATION_ERROR` lúc ghi `400` (Phụ lục B) lúc ghi `422` (FR-AUTH).
- **Hướng sửa:** Xóa DXA, bỏ 409 nếu auto-suffix, thống nhất validation `422` + business rule `400`.

---

## Khuyến nghị cho Nhóm 22 (theo phân công README)

- **TV1 (User/RefreshToken):** fix F-01, F-02, F-07, F-11 trước khi code Entity.
- **TV2 (Category):** fix F-14, F-15, F-17.
- **TV3 (Recipe/Nutrition):** fix F-06, F-08, F-09.
- **TV4 (Step/Ingredient/Image):** fix F-10, F-12.
- **Chung:** fix F-03, F-04, F-05, F-13, F-16 trong `Program.cs` (auth, cache, logging, rate-limit) trước Tuần 2.

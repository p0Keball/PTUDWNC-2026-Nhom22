# Quản trị người dùng, nhật ký hệ thống & trạng thái công thức

Tài liệu bàn giao phần việc của **Trần Gia Huy** trong Tuần 5, bổ sung cho
`child-data.md` và `file-storage.md`.

## 1. Phạm vi

| Hạng mục | Nội dung |
|---|---|
| Upload ảnh | Giữ luồng presigned MinIO; tự fallback sang `POST /images` (multipart) khi presigned PUT lỗi CORS/hết hạn |
| Trạng thái công thức | `Draft → Published → Archived`, khôi phục `Archived → Draft`, xóa mềm |
| Quản trị người dùng | Danh sách Admin/Author, lọc/tìm kiếm, kích hoạt/vô hiệu hóa, đổi role |
| Nhật ký hệ thống | Ghi audit trail vào bảng `AuditLogs` + structured log Serilog (Console/File/Seq) |
| Bảo mật | Endpoint admin chỉ role `Admin`; upload có rate limit riêng; lỗi trả RFC 7807 kèm `traceId`/`correlationId` |

## 2. Trạng thái công thức

### Luồng hợp lệ

```
Draft ──publish──▶ Published ──archive──▶ Archived
  ▲                    │                     │
  └──unpublish─────────┘                     │
  ◀────────────────unarchive─────────────────┘

Bất kỳ trạng thái ──delete──▶ Đã xóa mềm (IsDeleted = true)
```

### Endpoint

| Method | Endpoint | Trạng thái nguồn | Ghi chú |
|---|---|---|---|
| PATCH | `/api/v1/recipes/{id}/publish` | `Draft` | Kiểm tra checklist (tối thiểu 1 bước), sai → 422 `RECIPE_STATUS_TRANSITION_INVALID` |
| PATCH | `/api/v1/recipes/{id}/unpublish` | `Published` | Quay về `Draft` |
| PATCH | `/api/v1/recipes/{id}/archive` | `Draft`/`Published` | Chuyển `Archived`, gỡ khỏi danh sách công khai |
| PATCH | `/api/v1/recipes/{id}/unarchive` | `Archived` | Khôi phục về `Draft` |
| DELETE | `/api/v1/recipes/{id}` | Mọi trạng thái | Xóa mềm, ghi audit `RECIPE_DELETED` |

Mọi endpoint yêu cầu đăng nhập và chỉ chủ sở hữu hoặc Admin được thao tác.

### Giao diện

- `/dashboard/recipes`: tab theo trạng thái, nút Xuất bản / Gỡ xuất bản / Lưu trữ /
  Khôi phục / Xóa, có hộp thoại xác nhận trước thao tác ảnh hưởng.
- `/dashboard/recipes/[id]/edit`: panel "Trạng thái" hiển thị badge và các nút
  chuyển trạng thái; publish chạy lại checklist trước khi gọi API.

## 3. Upload ảnh

`frontend/components/ImageUploader.tsx` + `frontend/lib/api.ts`:

1. Nén ảnh phía client (`lib/compress.ts`) và kiểm tra MIME/kích thước.
2. Gọi `POST /api/v1/recipes/{id}/images/presign` → PUT file trực tiếp lên MinIO.
3. Gọi `POST /api/v1/recipes/{id}/images/confirm` để backend kiểm tra magic bytes
   và lưu bản ghi.
4. Nếu bước PUT presigned thất bại (CORS, URL hết hạn), tự động fallback sang
   `POST /api/v1/recipes/{id}/images` với `multipart/form-data`.

Upload áp dụng rate limit `upload` (5 request/phút/IP) và ghi audit
`IMAGE_UPLOADED`, `IMAGE_SET_PRIMARY`, `IMAGE_DELETED`.

## 4. Quản trị người dùng

### Endpoint (Admin-only)

| Method | Endpoint | Mô tả |
|---|---|---|
| GET | `/api/v1/admin/users?page=&pageSize=&search=&role=&status=&sort=` | Danh sách người dùng + thống kê `totalUsers/activeUsers/admins/authors` |
| PATCH | `/api/v1/admin/users/{id}/status` | Body `{ "isActive": true/false }`; vô hiệu hóa sẽ thu hồi refresh token |
| PATCH | `/api/v1/admin/users/{id}/role` | Body `{ "role": "Admin" }` hoặc `{ "role": "Author" }` |
| GET | `/api/v1/admin/audit-logs?page=&pageSize=&action=&entityType=&userId=&search=&from=&to=` | Nhật ký hệ thống |

Quy tắc an toàn:

- Không thể tự vô hiệu hóa tài khoản đang đăng nhập (`ADMIN_SELF_DEACTIVATE`).
- Không thể tự đổi role của chính mình (`ADMIN_SELF_ROLE_CHANGE`).
- Chỉ chấp nhận role `Admin` hoặc `Author` (`USER_INVALID_ROLE`).

### Giao diện

- `/admin/users`: thẻ thống kê, tìm kiếm theo email/tên, lọc role/trạng thái,
  sắp xếp, phân trang; đổi role và bật/tắt tài khoản đều có xác nhận.
- `/admin/logs`: lọc theo hành động, đối tượng, khoảng ngày, từ khóa; hiển thị
  thời gian, người dùng, mô tả, IP, Correlation ID (bấm để sao chép).
- `AdminAuthGate` chặn người không có role Admin và gọi lại `/auth/me` một lần
  để đồng bộ role khi dùng cookie auth.

## 5. Nhật ký hệ thống

### Audit trail (bảng `AuditLogs`)

Entity `FoodBlog.Domain/Entities/AuditLog.cs`, cấu hình
`AuditLogConfiguration`, migration `20261028104044_AddAuditLogs`.

| Cột | Ý nghĩa |
|---|---|
| `Action` | Mã hành động, ví dụ `RECIPE_PUBLISHED` |
| `EntityType` / `EntityId` | Đối tượng bị tác động |
| `UserId` / `UserEmail` | Người thực hiện (nếu có) |
| `Description` | Mô tả tiếng Việt dễ đọc |
| `IpAddress`, `UserAgent` | Thông tin request |
| `CorrelationId` | Nối audit log với structured log |
| `Succeeded` | Thành công hay thất bại |
| `CreatedAt` | UTC |

Các nhóm hành động đang ghi: `RECIPE_*`, `IMAGE_*`, `USER_*`, `STORAGE_*`.

### Structured logging (Serilog)

Cấu hình trong `src/FoodBlog.API/appsettings.json` → `Serilog`:

- Console sink (dev)
- File sink `logs/foodblog-YYYYMMDD.log`, giữ 14 ngày
- Seq sink `http://localhost:5341`

Mỗi request được enrich `CorrelationId`, `UserId`, `UserAgent`;
`CorrelationIdMiddleware` nhận header `X-Correlation-ID` từ client hoặc tự sinh.
MediatR `LoggingBehavior` ghi tên command/query và thời gian xử lý.

Lỗi RFC 7807 trả thêm `traceId` và `correlationId` để tra cứu log.

## 6. Kiểm thử

```bash
# Backend
dotnet test tests/FoodBlog.Tests/FoodBlog.Tests.csproj

# Frontend
cd frontend
npm run typecheck
npm run lint
npm run build
```

Integration test dùng `TestAuthHandler` để giả lập user Author đã seed, và loại
bỏ toàn bộ hosted service (Hangfire/worker) khi chạy test.

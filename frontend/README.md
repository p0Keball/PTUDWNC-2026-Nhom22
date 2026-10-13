# FoodBlog Frontend

Frontend của FoodBlog được xây dựng bằng Next.js App Router, React và
Tailwind CSS. Ứng dụng gọi FoodBlog API qua biến môi trường
`NEXT_PUBLIC_API_URL`.

## Yêu cầu môi trường

- Node.js 20 LTS trở lên
- npm 10 trở lên
- FoodBlog API đang chạy (mặc định `http://localhost:5000`)

## Cài đặt và chạy local

```bash
cd frontend
npm install
copy .env.example .env.local
npm run dev
```

Mở [http://localhost:3000](http://localhost:3000).

Trên macOS/Linux, dùng lệnh sau để tạo file môi trường:

```bash
cp .env.example .env.local
```

## Biến môi trường

| Biến | Bắt buộc | Mô tả |
| --- | --- | --- |
| `NEXT_PUBLIC_API_URL` | Không | URL backend API, mặc định `http://localhost:5000` |
| `NEXT_PUBLIC_GOOGLE_CLIENT_ID` | Khi dùng Google Login | Google OAuth Client ID public |

Không đặt client secret, database connection string hoặc token bí mật trong
biến `NEXT_PUBLIC_*`. Các biến này được đưa vào bundle trình duyệt.

Google OAuth callback cần được đăng ký tại Google Cloud:

```text
http://localhost:3000/auth/google/callback
```

## Cấu trúc thư mục

```text
frontend/
├── app/                    # Route và layout theo Next.js App Router
│   ├── auth/               # OAuth callback
│   ├── dashboard/          # Khu vực quản trị nội dung
│   ├── login/              # Đăng nhập email/Google
│   ├── profile/            # Xem và cập nhật profile
│   └── recipes/            # Danh sách, chi tiết, tạo và sửa recipe
├── components/             # UI component dùng lại
├── lib/
│   ├── api.ts              # API client và kiểu dữ liệu
│   └── config.ts           # Cấu hình runtime public
├── public/                 # Tài nguyên tĩnh
├── next.config.ts          # Cấu hình Next.js
├── postcss.config.mjs      # Tailwind/PostCSS
└── tsconfig.json           # TypeScript và alias @/*
```

## Kiểm tra trước khi commit

```bash
npm run typecheck
npm run lint
npm run build
```

`npm run build` tạo bản production. Chạy bản production bằng:

```bash
npm run start
```

## Ghi chú xác thực

Frontend lưu access/refresh token theo contract hiện tại của API và gửi
Identity cookie bằng `credentials: "include"`. CORS backend phải cho phép
đúng origin frontend, ví dụ `http://localhost:3000`; không dùng wildcard khi
credentials được bật.

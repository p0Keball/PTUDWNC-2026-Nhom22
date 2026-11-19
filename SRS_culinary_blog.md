# GIÁO TRÌNH PHÁT TRIỂN ỨNG DỤNG WEB NÂNG CAO

**Phiên bản V4**  
**.NET 10 + Next.js App Router**

# TÀI LIỆU ĐẶC TẢ YÊU CẦU PHẦN MỀM
**Software Requirements Specification (SRS)**  
Tiêu chuẩn IEEE 830/ISO/IEC/IEEE 29148:2018  
Dự án: Blog Ẩm thực và Nấu ăn (Culinary Blog)

| Thông tin | Giá trị |
| :--- | :--- |
| **Phiên bản tài liệu** | 1.0.0 |
| **Ngày phát hành** | 04/06/2026 |
| **Trạng thái** | Đã duyệt (Approved) |
| **Công nghệ Backend** | .NET 10 Minimal APIs, C# |
| **Công nghệ Frontend** | Next.js App Router, TypeScript |
| **Cơ sở dữ liệu** | PostgreSQL 16 |
| **Object Storage** | MinIO (S3-Compatible) |
| **Cache** | Redis 7 |

*Tài liệu này được biên soạn theo tiêu chuẩn IEEE 830/ISO/IEC/IEEE 29148:2018.*

---

## LỊCH SỬ THAY ĐỔI TÀI LIỆU

| Phiên bản | Ngày | Tác giả / Vai trò | Nội dung thay đổi | Trạng thái |
| :--- | :--- | :--- | :--- | :--- |
| 1.0.0 | 04/06/2026 | Senior BA / Architect | Phát hành lần đầu – Bản hoàn chỉnh theo IEEE 830 / ISO 29148. | Approved |
| 0.9.0 | 20/05/2026 | Senior BA | Bổ sung Chương 7 (Data Model), Chương 8 (API Spec) và Phụ lục. | Under Review |
| 0.8.0 | 05/05/2026 | Senior BA | Hoàn thiện Chương 3 (FR), bổ sung FR-FILE, FR-JOB, FR-OBS. | Draft |
| 0.5.0 | 15/04/2026 | Senior BA | Phác thảo ban đầu: Chương 1-4 (skeleton). | Draft |

**Phê duyệt tài liệu:** Tài liệu phiên bản 1.0.0 đã được xem xét và phê duyệt bởi Trưởng nhóm Kiến trúc Hệ thống (Lead Systems Architect). Mọi thay đổi từ phiên bản 1.0.0 trở đi đều phải thông qua quy trình Change Request (CR) và được cập nhật vào bảng này.

---

## MỤC LỤC

- [CHƯƠNG 1. GIỚI THIỆU](#chương-1-giới-thiệu)
  - [1.1. Mục đích Tài liệu](#11-mục-đích-tài-liệu)
  - [1.2. Phạm vi Sản phẩm](#12-phạm-vi-sản-phẩm)
    - [1.2.1. Tên và Định danh](#121-tên-và-định-danh)
    - [1.2.2. Mô tả Sản phẩm](#122-mô-tả-sản-phẩm)
    - [1.2.3. Những gì KHÔNG thuộc phạm vi](#123-những-gì-không-thuộc-phạm-vi)
  - [1.3. Định nghĩa, Từ viết tắt và Ký hiệu](#13-định-nghĩa-từ-viết-tắt-và-ký-hiệu)
  - [1.4. Tài liệu Tham chiếu](#14-tài-liệu-tham-chiếu)
  - [1.5. Tổng quan Tài liệu](#15-tổng-quan-tài-liệu)
- [CHƯƠNG 2. MÔ TẢ TỔNG QUAN HỆ THỐNG](#chương-2-mô-tả-tổng-quan-hệ-thống)
  - [2.1. Bối cảnh Sản phẩm](#21-bối-cảnh-sản-phẩm)
    - [2.1.1. Vị trí trong Hệ sinh thái](#211-vị-trí-trong-hệ-sinh-thái)
    - [2.1.2. Quan hệ với Hệ thống Ngoài](#212-quan-hệ-với-hệ-thống-ngoài)
  - [2.2. Chức năng Sản phẩm Tổng quát](#22-chức-năng-sản-phẩm-tổng-quát)
  - [2.3. Các Lớp Người dùng và Đặc điểm](#23-các-lớp-người-dùng-và-đặc-điểm)
  - [2.4. Môi trường Vận hành](#24-môi-trường-vận-hành)
    - [2.4.1. Môi trường Server (Production)](#241-môi-trường-server-production)
    - [2.4.2. Môi trường Phát triển (Development)](#242-môi-trường-phát-triển-development)
    - [2.4.3. Yêu cầu Trình duyệt Client](#243-yêu-cầu-trình-duyệt-client)
  - [2.5. Ràng buộc Thiết kế và Hiện thực](#25-ràng-buộc-thiết-kế-và-hiện-thực)
  - [2.6. Giả định và Phụ thuộc](#26-giả-định-và-phụ-thuộc)
    - [2.6.1. Giả định](#261-giả-định)
    - [2.6.2. Phụ thuộc Bên ngoài](#262-phụ-thuộc-bên-ngoài)
- [CHƯƠNG 3. YÊU CẦU CHỨC NĂNG CHI TIẾT](#chương-3-yêu-cầu-chức-năng-chi-tiết)
  - [3.1. Module Xác thực và Quản lý Người dùng (FR-AUTH)](#31-module-xác-thực-và-quản-lý-người-dùng-fr-auth)
  - [3.2. Module Quản lý Danh mục (FR-CAT)](#32-module-quản-lý-danh-mục-fr-cat)
  - [3.3. Module Quản lý Công thức Nấu ăn (FR-RCP)](#33-module-quản-lý-công-thức-nấu-ăn-fr-rcp)
  - [3.4. Module Tìm kiếm và Phân trang (FR-SRCH)](#34-module-tìm-kiếm-và-phân-trang-fr-srch)
  - [3.5. Module Quản lý Tệp tin (FR-FILE)](#35-module-quản-lý-tệp-tin-fr-file)
  - [3.6. Module Background Jobs (FR-JOB)](#36-module-background-jobs-fr-job)
  - [3.7. Module Quan sát Hệ thống (FR-OBS)](#37-module-quan-sát-hệ-thống-fr-obs)
- [CHƯƠNG 4. YÊU CẦU PHI CHỨC NĂNG (NFR)](#chương-4-yêu-cầu-phi-chức-năng-nfr)
  - [4.1. Hiệu năng (NFR-PERF)](#41-hiệu-năng-nfr-perf)
  - [4.2. Bảo mật (NFR-SEC)](#42-bảo-mật-nfr-sec)
  - [4.3. Khả năng Sử dụng (NFR-USE)](#43-khả-năng-sử-dụng-nfr-use)
  - [4.4. Độ tin cậy (NFR-REL)](#44-độ-tin-cậy-nfr-rel)
  - [4.5. Khả năng Bảo trì (NFR-MAINT)](#45-khả-năng-bảo-trì-nfr-maint)
  - [4.6. Khả năng Mở rộng (NFR-SCALE)](#46-khả-năng-mở-rộng-nfr-scale)
  - [4.7. Tối ưu SEO (NFR-SEO)](#47-tối-ưu-seo-nfr-seo)
- [CHƯƠNG 5. YÊU CẦU GIAO DIỆN NGOÀI](#chương-5-yêu-cầu-giao-diện-ngoài)
  - [5.1. Giao diện Người dùng (UI)](#51-giao-diện-người-dùng-ui)
  - [5.2. Giao diện Phần mềm – REST API](#52-giao-diện-phần-mềm--rest-api)
  - [5.3. Giao diện Dịch vụ Bên thứ ba](#53-giao-diện-dịch-vụ-bên-thứ-ba)
  - [5.4. Giao diện Phần cứng](#54-giao-diện-phần-cứng)
- [CHƯƠNG 6. KIẾN TRÚC HỆ THỐNG](#chương-6-kiến-trúc-hệ-thống)
  - [6.1. Tổng quan Kiến trúc](#61-tổng-quan-kiến-trúc)
  - [6.2. Kiến trúc Backend - Clean Architecture](#62-kiến-trúc-backend---clean-architecture)
  - [6.3. CQRS + MediatR Pipeline](#63-cqrs--mediatr-pipeline)
  - [6.4. Mô hình Quan hệ Thực thể (ERD tóm tắt)](#64-mô-hình-quan-hệ-thực-thể-erd-tóm-tắt)
  - [6.5. Triển khai – Docker Compose](#65-triển-khai--docker-compose)
- [CHƯƠNG 7. MÔ HÌNH DỮ LIỆU](#chương-7-mô-hình-dữ-liệu)
  - [7.1. BaseEntity (Abstract)](#71-baseentity-abstract)
  - [7.2. Recipe](#72-recipe)
    - [7.2.1. Recipe Nutrition (Owned Entity)](#721-recipe-nutrition-owned-entity)
  - [7.3. RecipeStep](#73-recipestep)
  - [7.4. RecipeIngredient](#74-recipeingredient)
  - [7.5. RecipeImage](#75-recipeimage)
  - [7.6. Category](#76-category)
  - [7.7. ApplicationUser (extends Identity User)](#77-applicationuser-extends-identity-user)
  - [7.8. Refresh Token](#78-refresh-token)
- [CHƯƠNG 8. ĐẶC TẢ REST API](#chương-8-đặc-tả-rest-api)
  - [8.1. Authentication Module (/auth)](#81-authentication-module-auth)
  - [8.2. Categories Module (/categories)](#82-categories-module-categories)
  - [8.3. Recipes Module (/recipes)](#83-recipes-module-recipes)
  - [8.4. Recipe Images (/recipes/{id}/images)](#84-recipe-images-recipesidimages)
  - [8.5. Recipe Steps (/recipes/{id}/steps)](#85-recipe-steps-recipesidsteps)
  - [8.6. Recipe Ingredients (/recipes/{id}/ingredients)](#86-recipe-ingredients-recipesidingredients)
  - [8.7. Health Check Endpoints](#87-health-check-endpoints)
- [PHỤ LỤC A – HTTP STATUS CODES](#phụ-lục-a--http-status-codes)
- [PHỤ LỤC B – APPLICATION ERROR CODES](#phụ-lục-b--application-error-codes)
- [PHỤ LỤC C – TỪ ĐIỂN THUẬT NGỮ](#phụ-lục-c--từ-điển-thuật-ngữ)

---

# CHƯƠNG 1. GIỚI THIỆU

## 1.1. Mục đích Tài liệu
Tài liệu Đặc tả Yêu cầu Phần mềm (Software Requirements Specification - SRS) này được biên soạn theo tiêu chuẩn IEEE 830-1998 và ISO/IEC/IEEE 29148:2018 nhằm mô tả đầy đủ, chính xác và nhất quán toàn bộ yêu cầu chức năng (Functional Requirements) và yêu cầu phi chức năng (Non-Functional Requirements) của dự án ứng dụng web Blog Ẩm thực và Nấu ăn (Culinary Blog).

Tài liệu này phục vụ các đối tượng sau:
- **Nhóm phát triển Backend (.NET 10/C#):** Căn cứ thiết kế API, domain model, và business rules.
- **Nhóm phát triển Frontend (Next.js/TypeScript):** Căn cứ thiết kế giao diện, luồng người dùng và tích hợp API.
- **Kỹ sư Kiểm thử (QA/QC):** Cơ sở xây dựng test cases, kiểm thử chấp nhận (acceptance testing).
- **Kiến trúc sư Hệ thống:** Tham chiếu khi đưa ra quyết định kiến trúc (architecture decisions).
- **Giảng viên và Sinh viên:** Tài liệu học thuật mẫu cho dự án thực hành xuyên suốt giáo trình.
- **Stakeholder / Product Owner:** Phê duyệt phạm vi và ưu tiên tính năng.

**Phạm vi hiệu lực:** Tài liệu này có hiệu lực từ phiên bản 1.0.0 và là tài liệu nền tảng (baseline) cho toàn bộ vòng đời phát triển dự án. Mọi thay đổi yêu cầu sau khi tài liệu được phê duyệt phải tuân theo quy trình quản lý thay đổi (Change Management Process).

## 1.2. Phạm vi Sản phẩm

### 1.2.1. Tên và Định danh
| Thuộc tính | Giá trị |
| :--- | :--- |
| **Tên sản phẩm** | Culinary Blog - Blog Ẩm thực và Nấu ăn |
| **Định danh dự án** | CULINARY-BLOG-V1 |
| **Loại hệ thống** | Ứng dụng Web Full-Stack (API-Driven Architecture) |
| **Phiên bản sản phẩm** | 1.0.0 |
| **Môi trường đích** | Cloud/On-premise (Docker Compose + Nginx) |

### 1.2.2. Mô tả Sản phẩm
Culinary Blog là một nền tảng web cho phép người dùng chia sẻ, khám phá và lưu trữ các công thức nấu ăn từ nhiều nền ẩm thực khác nhau. Ứng dụng cung cấp hệ sinh thái hoàn chỉnh bao gồm:
- **Nền tảng chia sẻ công thức:** Tác giả (Author) đăng tải công thức với hình ảnh, danh sách nguyên liệu chi tiết, hướng dẫn từng bước thực hiện và thông tin dinh dưỡng.
- **Tổ chức nội dung:** Phân loại công thức theo danh mục (Category), độ khó (Difficulty Level), thời gian chuẩn bị và nấu.
- **Tìm kiếm thông minh:** Full-Text Search tiếng Việt sử dụng PostgreSQL `tsvector`/`tsquery` với `unaccent` extension.
- **Bảo mật đa lớp:** Xác thực JWT stateless, phân quyền theo vai trò (RBAC) và theo tài nguyên (Resource-Based Authorization), đăng nhập Google OAuth 2.0.
- **Tối ưu hiệu năng và SEO:** Redis distributed cache, Next.js ISR, Open Graph Protocol, JSON-LD Schema.org Recipe markup.
- **Quan sát hệ thống:** Structured logging (Serilog), distributed tracing (OpenTelemetry), health check endpoints.

### 1.2.3. Những gì KHÔNG thuộc phạm vi
Các tính năng sau đây nằm ngoài phạm vi phiên bản 1.0.0:
- Hệ thống bình luận (Comment System) và đánh giá sao (Rating System).
- Tính năng lưu/đánh dấu công thức yêu thích (Bookmark/Favorite).
- Thông báo real-time (SignalR/WebSocket).
- Ứng dụng di động native (iOS/Android).
- Thanh toán / Tính năng thương mại điện tử.
- Hệ thống nhắn tin trực tiếp giữa người dùng.
- GraphQL API (định hướng sau khóa học).

## 1.3. Định nghĩa, Từ viết tắt và Ký hiệu

| Thuật ngữ / Viết tắt | Định nghĩa đầy đủ |
| :--- | :--- |
| **SRS** | Software Requirements Specification – Đặc tả Yêu cầu Phần mềm. |
| **FR** | Functional Requirement – Yêu cầu chức năng. |
| **NFR** | Non-Functional Requirement – Yêu cầu phi chức năng. |
| **API** | Application Programming Interface – Giao diện lập trình ứng dụng. |
| **REST** | Representational State Transfer – Kiểu kiến trúc API phổ biến nhất. |
| **JWT** | JSON Web Token – Chuẩn token xác thực stateless (RFC 7519). |
| **RBAC** | Role-Based Access Control – Kiểm soát truy cập dựa trên vai trò. |
| **CQRS** | Command Query Responsibility Segregation – Pattern tách biệt lệnh và truy vấn. |
| **DDD** | Domain-Driven Design – Phương pháp thiết kế phần mềm lấy domain làm trung tâm. |
| **ORM** | Object-Relational Mapper – Công cụ ánh xạ object-database (EF Core). |
| **FTS** | Full-Text Search – Tìm kiếm toàn văn bản. |
| **ISR** | Incremental Static Regeneration – Kỹ thuật tái tạo trang tĩnh của Next.js. |
| **LCP** | Largest Contentful Paint – Core Web Vital đo tốc độ tải nội dung lớn nhất. |
| **CLS** | Cumulative Layout Shift – Core Web Vital đo độ ổn định bố cục trang. |
| **INP** | Interaction to Next Paint – Core Web Vital đo thời gian phản hồi tương tác. |
| **CI/CD** | Continuous Integration / Continuous Delivery – Tích hợp và triển khai liên tục. |
| **DXA** | Device-independent pixel unit used in OOXML (1 inch = 1440 DXA). |
| **TTL** | Time-To-Live – Thời gian sống của dữ liệu trong cache. |
| **SSR** | Server-Side Rendering – Render HTML trên server. |
| **SSG** | Static Site Generation – Tạo trang tĩnh lúc build time. |
| **MoSCoW** | Must Have / Should Have / Could Have / Won't Have – Mô hình phân loại ưu tiên. |
| **RFC** | Request For Comments – Tài liệu tiêu chuẩn kỹ thuật (e.g., RFC 7807). |
| **ERD** | Entity Relationship Diagram – Sơ đồ quan hệ thực thể. |
| **PBKDF2** | Password-Based Key Derivation Function 2 – Thuật toán hash mật khẩu an toàn. |
| **CDN** | Content Delivery Network – Mạng phân phối nội dung. |
| **MIME** | Multipurpose Internet Mail Extensions – Chuẩn định dạng tệp trên Internet. |
| **JSON-LD** | JavaScript Object Notation for Linked Data – Định dạng dữ liệu có cấu trúc cho SEO. |

## 1.4. Tài liệu Tham chiếu

| STT | Tài liệu / Tiêu chuẩn | Nguồn / URL |
| :--- | :--- | :--- |
| 1 | IEEE Std 830-1998 Recommended Practice for Software Requirements Specifications | https://ieeexplore.ieee.org/document/720574 |
| 2 | ISO/IEC/IEEE 29148:2018 Requirements Engineering | https://www.iso.org/standard/72089.html |
| 3 | OWASP Top 10:2021 Top 10 Web Application Security Risks | https://owasp.org/www-project-top-ten/ |
| 4 | RFC 7807 Problem Details for HTTP APIs | https://datatracker.ietf.org/doc/html/rfc7807 |
| 5 | RFC 7519 JSON Web Token (JWT) | https://datatracker.ietf.org/doc/html/rfc7519 |
| 6 | RFC 6749 The OAuth 2.0 Authorization Framework | https://datatracker.ietf.org/doc/html/rfc6749 |
| 7 | .NET 10 Minimal APIs - Microsoft Learn | https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis |
| 8 | ASP.NET Core Identity - Microsoft Learn | https://learn.microsoft.com/aspnet/core/security/authentication/identity |
| 9 | Entity Framework Core 10 Documentation | https://learn.microsoft.com/ef/core/ |
| 10 | Next.js 15 App Router Documentation | https://nextjs.org/docs |
| 11 | PostgreSQL 16 Documentation - Full-Text Search | https://www.postgresql.org/docs/16/textsearch.html |
| 12 | Redis 7 Documentation | https://redis.io/docs/ |
| 13 | MinIO S3-Compatible Object Storage | https://min.io/docs/ |
| 14 | Google Web Vitals Core Web Vitals | https://web.dev/explore/learn-core-web-vitals |
| 15 | Schema.org Recipe - Structured Data | https://schema.org/Recipe |
| 16 | OpenTelemetry .NET Documentation | https://opentelemetry.io/docs/languages/dotnet/ |
| 17 | Serilog Documentation | https://serilog.net/ |
| 18 | Hangfire Documentation | https://docs.hangfire.io/ |
| 19 | FluentValidation Documentation | https://docs.fluentvalidation.net/ |
| 20 | Giáo trình Phát triển Ứng dụng Web Nâng cao V4 | N/A (tài liệu nội bộ) |

## 1.5. Tổng quan Tài liệu
Tài liệu SRS này được tổ chức thành 8 chương chính và 3 phụ lục, theo cấu trúc từ tổng quan đến chi tiết:
- **Chương 2 – Mô tả Tổng quan:** Bối cảnh sản phẩm, chức năng tóm tắt, các lớp người dùng, môi trường vận hành và ràng buộc thiết kế.
- **Chương 3 – Yêu cầu Chức năng:** 27 FR được đặc tả chi tiết theo format chuẩn, nhóm thành 7 module chức năng.
- **Chương 4 – Yêu cầu Phi chức năng:** Hiệu năng, bảo mật, khả năng sử dụng, độ tin cậy, khả năng bảo trì/mở rộng và SEO.
- **Chương 5 – Giao diện Ngoài:** Tích hợp với các hệ thống và dịch vụ ngoài (Google OAuth, MinIO, Redis, SendGrid).
- **Chương 6 – Kiến trúc Hệ thống:** Clean Architecture Backend, Next.js App Router Frontend, chiến lược caching và deployment.
- **Chương 7 – Mô hình Dữ liệu:** ERD mô tả văn bản và bảng định nghĩa chi tiết từng entity/table.
- **Chương 8 – Đặc tả API REST:** Quy ước, chuẩn lỗi RFC 7807, và bảng tổng hợp tất cả ~30 endpoint.
- **Phụ lục A-C:** HTTP Status Codes, Application Error Codes, và Từ điển thuật ngữ.

---

# CHƯƠNG 2. MÔ TẢ TỔNG QUAN HỆ THỐNG

## 2.1. Bối cảnh Sản phẩm

### 2.1.1. Vị trí trong Hệ sinh thái
Culinary Blog vận hành theo mô hình API-Driven Architecture, trong đó Backend (.NET 10) và Frontend (Next.js) là hai hệ thống độc lập giao tiếp hoàn toàn qua HTTP/JSON RESTful API. Không có server-side rendering truyền thống (MVC Razor/Blazor) hay shared view engine giữa hai tầng.

**Sơ đồ bối cảnh hệ thống (Context Diagram):**

```
+-------------------------------------------------------------------------------+
|                             CULINARY BLOG SYSTEM                              |
|                                                                               |
|   +-----------------------+   REST    +-----------------------------------+   |
|   |   NEXT.JS FRONTEND    |<--------->|        .NET 10 BACKEND API        |   |
|   |     (App Router)      |   JSON    |   (Minimal APIs + Clean Arch)     |   |
|   |     Port: 3000        |           |            Port: 5000             |   |
|   +-----------------------+           +-----------------------------------+   |
|                                                         |                     |
|     +---------+  +---------+  +---------+  +------------+  +--------------+   |
|     | Pgsql   |  | Redis   |  | MinIO   |  | Hangfire   |  | Google Auth  |   |
|     | :5432   |  | :6379   |  | :9000   |  | Jobs       |  | OAuth2.0     |   |
|     +---------+  +---------+  +---------+  +------------+  +--------------+   |
+-------------------------------------------------------------------------------+
```
*Hình 2.1. Sơ đồ bối cảnh hệ thống Culinary Blog*

### 2.1.2. Quan hệ với Hệ thống Ngoài

| Hệ thống Ngoài | Vai trò | Giao thức / Chuẩn | Hướng tích hợp |
| :--- | :--- | :--- | :--- |
| **PostgreSQL 16** | Hệ quản trị CSDL quan hệ chính (RDBMS) | TCP + Npgsql Driver (EF Core) | Backend -> PostgreSQL |
| **Redis 7** | Distributed Cache & Session Store | TCP + StackExchange.Redis | Backend -> Redis |
| **MinIO (S3)** | Object Storage cho ảnh công thức | HTTP/S3 API + MinIO.NET SDK | Backend -> MinIO |
| **Google OAuth 2.0** | Đăng nhập bên thứ ba (Identity Provider) | HTTPS + OpenID Connect | Client -> Google -> Backend |
| **Hangfire** | Background Job Processing (embedded) | In-process (.NET) | Backend (internal) |
| **Serilog / Seq** | Structured Log Aggregation (development) | HTTP Sink -> Seq | Backend -> Seq |
| **OpenTelemetry Collector** | Distributed Tracing & Metrics (production) | OTLP / gRPC | Backend -> Collector |
| **Nginx (Reverse Proxy)** | SSL termination, load balancing, static serving | HTTP/HTTPS | Client -> Nginx -> Services |

## 2.2. Chức năng Sản phẩm Tổng quát
Culinary Blog cung cấp 7 nhóm chức năng chính, được hiện thực hóa qua 27 Functional Requirements chi tiết tại Chương 3:

| Nhóm chức năng | Mã nhóm | Số FR | Mô tả tóm tắt |
| :--- | :--- | :--- | :--- |
| **Xác thực & Quản lý Người dùng** | FR-AUTH | 7 | Đăng ký, đăng nhập (email + Google), JWT refresh token, logout, quản lý profile. |
| **Quản lý Danh mục** | FR-CAT | 5 | CRUD danh mục công thức (Category) - phân quyền Admin. |
| **Quản lý Công thức nấu ăn** | FR-RCP | 10 | CRUD recipe, publish/archive, quản lý ảnh/bước/nguyên liệu. |
| **Tìm kiếm & Phân trang** | FR-SRCH | 4 | Full-Text Search (PostgreSQL), filter, sort, offset pagination. |
| **Quản lý Tệp tin** | FR-FILE | 2 | Upload/Delete ảnh trên MinIO S3-compatible. |
| **Background Jobs** | FR-JOB | 3 | Email chào mừng, thumbnail generation, sitemap XML (Hangfire). |
| **Quan sát Hệ thống** | FR-OBS | 3 | Health checks, structured logging, distributed tracing. |

## 2.3. Các Lớp Người dùng và Đặc điểm
Hệ thống định nghĩa 3 loại tác nhân (Actor) với quyền hạn khác nhau:

| Vai trò | Mô tả | Điều kiện | Quyền hạn chính | Ưu tiên phục vụ |
| :--- | :--- | :--- | :--- | :--- |
| **Khách (Guest / Anonymous)** | Người dùng chưa xác thực, truy cập ứng dụng mà không có tài khoản. | Không cần tài khoản | Xem danh sách & chi tiết recipe (Published), xem danh mục, tìm kiếm. KHÔNG được tạo/sửa/xóa. | **Cao** (đại đa số người dùng) |
| **Tác giả (Author)** | Người dùng đã đăng ký và xác thực thành công. Được tự động gán khi đăng ký. | Có tài khoản & JWT hợp lệ | + Tất cả quyền của Guest.<br>+ Tạo/sửa/xóa recipe CỦA MÌNH.<br>+ Upload ảnh, quản lý steps/ingredients.<br>+ Publish/Archive recipe của mình. | **Cao** (nhà sản xuất nội dung) |
| **Quản trị viên (Admin)** | Người quản lý hệ thống với quyền cao nhất. Được gán thủ công qua database seeding. | Có tài khoản & role Admin | + Tất cả quyền của Author.<br>+ Quản lý (CRUD) danh mục.<br>+ Sửa/xóa bất kỳ recipe của bất kỳ Author.<br>+ Truy cập Hangfire Dashboard.<br>+ Xem structured logs. | **Trung bình** (số lượng ít) |

**Ghi chú về phân quyền:** Hệ thống triển khai 3 tầng phân quyền:
1. **Role-Based Authorization:** phân biệt quyền dựa trên role (Guest/Author/Admin).
2. **Resource-Based Authorization:** Author chỉ sửa/xóa được recipe của chính mình (`AuthorId == currentUserId`).
3. **Policy-Based Authorization:** Policy "VerifiedAuthor" yêu cầu email đã xác nhận. Admin có quyền bypass resource ownership check.

## 2.4. Môi trường Vận hành

### 2.4.1. Môi trường Server (Production)
| Thành phần | Yêu cầu tối thiểu | Khuyến nghị | Ghi chú |
| :--- | :--- | :--- | :--- |
| **Hệ điều hành** | Linux Ubuntu 22.04 LTS | Ubuntu 22.04 LTS / Debian 12 | Docker phải được cài đặt |
| **.NET Runtime** | .NET 10.0 Runtime (aspnet) | .NET 10.0.x latest patch | Container: `mcr.microsoft.com/dotnet/aspnet:10.0` |
| **Node.js** | Node.js 20 LTS (build only) | Node.js 22 LTS | Chỉ cần lúc build Next.js; production dùng standalone |
| **PostgreSQL** | PostgreSQL 16.x | PostgreSQL 16.x | Extensions: `unaccent`, `pg_trgm` bắt buộc |
| **Redis** | Redis 7.x | Redis 7.2.x | Persistent mode với AOF |
| **MinIO** | MinIO RELEASE.2024+ | MinIO latest stable | Bucket policy: `public-read` cho recipe images |
| **Docker** | Docker Engine 24.x | Docker Engine 27.x + Compose v2 | Docker Compose cho local dev và staging |
| **Nginx** | Nginx 1.24+ | Nginx 1.26+ (stable) | Reverse proxy, SSL termination |
| **RAM** | 4 GB minimum | 8 GB+ | RAM cần tăng nếu Redis cache lớn |
| **CPU** | 2 vCPU minimum | 4 vCPU+ | CPU-intensive: FTS indexing, image processing |
| **Disk** | 20 GB SSD minimum | 50 GB+ SSD | MinIO object storage tốn nhiều disk |

### 2.4.2. Môi trường Phát triển (Development)
| Thành phần | Yêu cầu |
| :--- | :--- |
| **.NET 10 SDK** | `dotnet` SDK 10.0.x (bao gồm CLI và runtime) |
| **Node.js** | Node.js 20+ LTS với npm 10+ |
| **Docker Desktop** | Docker Desktop 4.x+ (Windows/macOS) hoặc Docker Engine (Linux) - để chạy PostgreSQL, Redis, MinIO local |
| **IDE / Editor** | Visual Studio 2022 v17.12+ / Rider 2024+ / VS Code với C# Dev Kit extension |
| **Git** | Git 2.40+ với Git LFS (nếu lưu asset lớn) |
| **Postman / Scalar** | Postman hoặc Scalar UI (tích hợp sẵn, chạy tại `/scalar`) để test API |

### 2.4.3. Yêu cầu Trình duyệt Client
| Trình duyệt | Phiên bản tối thiểu | Ghi chú |
| :--- | :--- | :--- |
| **Google Chrome** | 90+ | Khuyến nghị chính – tốt nhất cho Developer Tools |
| **Mozilla Firefox** | 88+ | Hỗ trợ đầy đủ |
| **Microsoft Edge** | 90+ (Chromium) | Hỗ trợ đầy đủ (Chromium-based) |
| **Safari** | 14+ (macOS 11+) | Hỗ trợ đầy đủ; Safari 13 trở xuống KHÔNG đảm bảo |
| **Mobile Chrome** | 90+ | Responsive design, touch-friendly |
| **Mobile Safari** | iOS 14+ | Hỗ trợ đầy đủ |
| **Internet Explorer**| Mọi phiên bản | KHÔNG hỗ trợ (EOL) |

## 2.5. Ràng buộc Thiết kế và Hiện thực

| Mã ràng buộc | Loại | Mô tả ràng buộc |
| :--- | :--- | :--- |
| **CONS-001** | Kiến trúc | Backend PHẢI tuân thủ Clean Architecture với 4 tầng riêng biệt: Domain, Application, Infrastructure, Presentation. Tầng Domain không được phụ thuộc bất kỳ thư viện ngoài nào. |
| **CONS-002** | Pattern | CQRS với MediatR là pattern bắt buộc cho tầng Application. Mỗi use case được hiện thực dưới dạng Command hoặc Query Handler riêng biệt. |
| **CONS-003** | Ngôn ngữ / Framework | Backend: .NET 10 Minimal APIs (không dùng MVC Controllers). Frontend: Next.js App Router (không dùng Pages Router). |
| **CONS-004** | Bảo mật | Xác thực PHẢI sử dụng JWT stateless (access token 15 phút, refresh token 7 ngày). Mật khẩu PHẢI được hash với PBKDF2 qua ASP.NET Core Identity. |
| **CONS-005** | API Design | API PHẢI tuân thủ RESTful design. Phản hồi lỗi PHẢI theo RFC 7807 (`application/problem+json`). API versioning qua URL path (`/api/v1/`). |
| **CONS-006** | Database | PostgreSQL là DBMS duy nhất. Migrations qua EF Core Code-First. Không viết raw SQL trực tiếp (dùng LINQ hoặc Raw SQL có parameterization qua EF Core). |
| **CONS-007** | File Upload | Kích thước tệp tải lên tối đa 5 MB. Định dạng chỉ chấp nhận: `image/jpeg`, `image/png`, `image/webp`, `image/avif`. Kiểm tra MIME type (không chỉ extension). |
| **CONS-008** | Validation | Input validation PHẢI qua FluentValidation kết hợp MediatR Pipeline Behavior. Không validation trong Endpoint handler. |
| **CONS-009** | Container | Ứng dụng PHẢI được đóng gói Docker. Dockerfile multi-stage build (SDK -> aspnet runtime). Docker Compose cho local development. |
| **CONS-010** | Logging | Structured logging với Serilog là bắt buộc. Mọi log entry PHẢI có CorrelationId, RequestPath, UserId (khi đã xác thực). |

## 2.6. Giả định và Phụ thuộc

### 2.6.1. Giả định
1. Môi trường development có kết nối Internet để pull Docker images và package NuGet/npm.
2. PostgreSQL, Redis và MinIO được cung cấp qua Docker Compose trong development và dưới dạng managed service (hoặc VPS) trong production.
3. Người dùng cuối có trình duyệt hiện đại và kết nối Internet đủ ổn định để load ảnh từ MinIO.
4. Dữ liệu test (seed) được tạo bằng thư viện Bogus với 50 recipe mẫu và 5 tác giả mẫu.
5. Email service (SendGrid hoặc SMTP) được cấu hình sẵn khi triển khai production để gửi email chào mừng.
6. Giới hạn dữ liệu kỳ vọng (initial scale): $\le 10.000$ công thức, $\le 5.000$ người dùng, $\le 50$ danh mục – phù hợp với single-server deployment.

### 2.6.2. Phụ thuộc Bên ngoài

| Phụ thuộc | Phiên bản | Mức độ ảnh hưởng nếu không khả dụng | Kế hoạch dự phòng |
| :--- | :--- | :--- | :--- |
| **Google OAuth 2.0 API** | v2 (OpenID Connect) | **Cao** – Mất chức năng đăng nhập Google | Vẫn có đăng nhập email/password. Hiển thị thông báo "Google login tạm thời không khả dụng". |
| **MinIO / S3** | MinIO RELEASE.2024+ | **Cao** – Không upload/xem được ảnh | Fallback về local FileSystem storage (development only). Production cần MinIO. |
| **Redis** | 7.x | **Trung bình** – Mất cache, hiệu năng giảm | Hệ thống tiếp tục hoạt động nhưng mọi request đều query database. Cache miss graceful degradation. |
| **PostgreSQL** | 16.x | **Rất cao** – Toàn bộ hệ thống ngừng | Backup định kỳ (`pg_dump`). Readiness probe sẽ fail, Nginx trả 503. |
| **Hangfire (in-process)** | v1.8+ | **Thấp** – Background jobs không chạy | Fire-and-forget jobs sẽ bị mất; Recurring jobs bỏ qua chu kỳ. Không ảnh hưởng core functionality. |

---

# CHƯƠNG 3. YÊU CẦU CHỨC NĂNG CHI TIẾT

Quy ước mức ưu tiên MoSCoW:
- **M** (Must Have – Bắt buộc)
- **S** (Should Have – Nên có)
- **C** (Could Have – Có thể có)
- **W** (Won't Have – Không trong scope hiện tại)

## 3.1. Module Xác thực và Quản lý Người dùng (FR-AUTH)

### FR-AUTH-001: Đăng ký Tài khoản (User Registration)
- **Mã yêu cầu:** FR-AUTH-001
- **Tên yêu cầu:** Đăng ký Tài khoản Mới
- **Nhóm chức năng:** Module Xác thực và Quản lý Người dùng (FR-AUTH)
- **Tác nhân:** Khách (Guest / Anonymous User)
- **Mức ưu tiên (MoSCoW):** M - Must Have (Bắt buộc)
- **Mô tả:** Hệ thống cho phép người dùng chưa có tài khoản tạo một tài khoản mới bằng cách cung cấp thông tin cơ bản. Sau khi đăng ký thành công, người dùng tự động được gán role "Author" và nhận bộ token để truy cập ngay lập tức (auto-login sau đăng ký). Hệ thống kích hoạt job gửi email chào mừng bất đồng bộ qua Hangfire.
- **Điều kiện tiên quyết:**
  1. Người dùng chưa đăng nhập vào hệ thống.
  2. Endpoint `POST /api/v1/auth/register` đang hoạt động.
  3. PostgreSQL database đang kết nối thành công.
- **Luồng chính (Happy Path):**
  1. Người dùng (client) gửi HTTP POST đến `/api/v1/auth/register` với JSON body: `{"fullName": "...", "email": "...", "userName": "...", "password": "..."}`.
  2. `RegisterCommand` được tạo và dispatch đến MediatR.
  3. `ValidationBehavior` chạy `RegisterCommandValidator`: kiểm tra `fullName` không rỗng, `email` đúng format, `userName` không chứa ký tự đặc biệt, `password` tối thiểu 8 ký tự (1 chữ hoa, 1 chữ số, 1 ký tự đặc biệt).
  4. `RegisterCommandHandler` kiểm tra email chưa tồn tại trong database (`UserManager.FindByEmailAsync`).
  5. Tạo `ApplicationUser` mới qua factory method `ApplicationUser.Create(fullName, email, userName)`.
  6. `UserManager.CreateAsync(user, password)` – ASP.NET Core Identity tự hash password với PBKDF2.
  7. `UserManager.AddToRoleAsync(user, "Author")` – gán role mặc định.
  8. `JwtService.GenerateAccessToken()` tạo JWT access token (HS256, 15 phút).
  9. `JwtService.GenerateRefreshToken()` tạo refresh token ngẫu nhiên (512-bit, 7 ngày).
  10. Lưu Refresh Token vào bảng `refresh_tokens` trong database.
  11. `BackgroundJob.Enqueue<WelcomeEmailJob>()` đẩy job gửi email chào mừng vào Hangfire queue (fire-and-forget).
  12. Trả về HTTP 201 Created với `AuthResponseDto`: `{ accessToken, refreshToken, expiresAt, user: { id, fullName, email, userName, avatarUrl, roles } }`.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Email đã tồn tại:** Tại bước 4, nếu email đã được đăng ký -> Throw `ConflictException` -> GlobalException Middleware trả về HTTP 409 Conflict với RFC 7807 body.
  - **A2 - Password không đủ mạnh:** Tại bước 3 hoặc 6, `UserManager.CreateAsync` trả về `IdentityError` -> Throw `ValidationException` -> HTTP 422 Unprocessable Entity với danh sách lỗi chi tiết.
  - **A3 - Dữ liệu đầu vào không hợp lệ:** Tại bước 3, FluentValidation fail -> HTTP 422 với từng field lỗi (theo RFC 7807 `ValidationProblemDetails`).
  - **A4 - Database không kết nối:** EF Core ném `DbUpdateException` -> HTTP 500 Internal Server Error (GlobalException Middleware log lỗi, không expose stack trace).
- **HTTP Method & Endpoint:** `POST /api/v1/auth/register`
- **Kết quả mong đợi:** Tài khoản mới được tạo trong database, role "Author" được gán, refresh token được persist, email chào mừng được đẩy vào Hangfire queue. Client nhận được access token và refresh token.
- **HTTP Status Code:** `201 Created` (Thành công), `409 Conflict` (Email đã tồn tại), `422 Unprocessable Entity` (Dữ liệu không hợp lệ), `500 Internal Server Error` (Lỗi hệ thống).

### FR-AUTH-002: Đăng nhập bằng Email Mật khẩu (Local Login)
- **Mã yêu cầu:** FR-AUTH-002
- **Tên yêu cầu:** Đăng nhập bằng Email và Mật khẩu
- **Nhóm chức năng:** Module Xác thực và Quản lý Người dùng (FR-AUTH)
- **Tác nhân:** Tác giả đã đăng ký (Author) hoặc Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have (Bắt buộc)
- **Mô tả:** Hệ thống cho phép người dùng đã có tài khoản đăng nhập bằng email và mật khẩu. Mỗi lần đăng nhập thành công tạo ra một cặp access token mới (JWT, 15 phút) và refresh token mới (7 ngày). Cơ chế Token Rotation: refresh token cũ KHÔNG bị xóa ngay mà được đánh dấu đã sử dụng (để phát hiện token reuse attack).
- **Điều kiện tiên quyết:**
  1. Người dùng đã có tài khoản hợp lệ trong hệ thống.
  2. Tài khoản chưa bị khóa (`LockoutEnabled = false` hoặc chưa đến lockout deadline).
- **Luồng chính (Happy Path):**
  1. Client gửi `POST /api/v1/auth/login` với body: `{"email": "...", "password": "..."}`.
  2. `LoginCommand` được dispatch qua MediatR.
  3. `ValidationBehavior` kiểm tra email format và password không rỗng.
  4. `LoginCommandHandler` tìm user: `UserManager.FindByEmailAsync(email)`.
  5. Xác minh mật khẩu: `UserManager.CheckPasswordAsync(user, password)` – so sánh với PBKDF2 hash.
  6. Kiểm tra tài khoản không bị lockout: `UserManager.IsLockedOutAsync(user)`.
  7. Tạo access token mới: `JwtService.GenerateAccessToken(user, roles)`.
  8. Tạo refresh token mới: `JwtService.GenerateRefreshToken(userId)`.
  9. Lưu refresh token mới vào database.
  10. Ghi nhận đăng nhập thành công: `UserManager.ResetAccessFailedCountAsync(user)`.
  11. Trả về HTTP 200 OK với `AuthResponseDto`.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Tài khoản không tồn tại hoặc mật khẩu sai:** HTTP 401 Unauthorized với message generic "Email hoặc mật khẩu không đúng" (KHÔNG tiết lộ tài khoản có tồn tại hay không để tránh User Enumeration Attack).
  - **A2 - Tài khoản bị lockout:** HTTP 423 Locked với thông báo thời gian unlock còn lại.
  - **A3 - Vượt quá số lần thử sai (5 lần):** `AccessFailedCount` tăng lên, sau 5 lần tài khoản bị lockout 15 phút (cấu hình qua `LockoutOptions`).
- **HTTP Method & Endpoint:** `POST /api/v1/auth/login`
- **Kết quả mong đợi:** Access token và refresh token mới được tạo và trả về. Refresh token được lưu vào database.
- **HTTP Status Code:** `200 OK`, `401 Unauthorized`, `422 Unprocessable Entity`, `423 Locked`.

### FR-AUTH-003: Đăng nhập bằng Google OAuth 2.0
- **Mã yêu cầu:** FR-AUTH-003
- **Tên yêu cầu:** Đăng nhập / Đăng ký bằng Google OAuth 2.0
- **Nhóm chức năng:** Module Xác thực và Quản lý Người dùng (FR-AUTH)
- **Tác nhân:** Khách (Guest) – lần đầu / Người dùng đã đăng ký trước đó qua Google
- **Mức ưu tiên (MoSCoW):** S - Should Have
- **Mô tả:** Hệ thống hỗ trợ đăng nhập qua tài khoản Google sử dụng OAuth 2.0 Authorization Code Flow với PKCE. Nếu đây là lần đăng nhập Google đầu tiên, hệ thống tự động tạo tài khoản mới từ thông tin Google profile (email, display name, avatar URL) và gán role "Author". Nếu email đã tồn tại từ đăng ký thủ công trước đó, hệ thống liên kết Google login với tài khoản hiện có.
- **Điều kiện tiên quyết:**
  1. Google OAuth 2.0 Credentials (`ClientId`, `ClientSecret`) đã được cấu hình trong `appsettings`.
  2. Redirect URI đã được đăng ký trong Google Cloud Console.
  3. Người dùng có tài khoản Google hợp lệ.
- **Luồng chính (Happy Path):**
  1. Frontend (Next.js) redirect người dùng đến Google Authorization Endpoint với scopes: `openid`, `email`, `profile`.
  2. Người dùng xác nhận cấp quyền trên Google Consent Screen.
  3. Google redirect về callback URL (Next.js) với Authorization Code.
  4. Auth.js v5 (Next.js) xử lý callback, lấy access token từ Google và lấy profile.
  5. Frontend gửi `POST /api/v1/auth/google` với `GoogleExternalLoginInfo`.
  6. `GoogleLoginCommandHandler` tìm user bằng `UserManager.FindByLoginAsync("Google", providerKey)`.
  7. Nếu chưa có tài khoản: kiểm tra email; nếu email chưa tồn tại thì tạo `ApplicationUser` mới từ Google profile, gán role "Author" -> `AddLoginAsync`.
  8. Nếu email đã tồn tại (đã đăng ký thủ công): liên kết Google login -> `AddLoginAsync` với tài khoản hiện có.
  9. Tạo access token và refresh token, lưu vào database.
  10. Trả về HTTP 200 OK với `AuthResponseDto`.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Google token không hợp lệ hoặc hết hạn:** HTTP 401 Unauthorized.
  - **A2 - Email Google bị revoke quyền:** HTTP 400 Bad Request.
  - **A3 - Google API không khả dụng:** HTTP 502 Bad Gateway.
- **HTTP Method & Endpoint:** `POST /api/v1/auth/google`
- **Kết quả mong đợi:** Người dùng được đăng nhập (hoặc tự động đăng ký), nhận `AuthResponseDto`. Tài khoản mới (nếu có) được tạo với role "Author".
- **HTTP Status Code:** `200 OK`, `400 Bad Request`, `401 Unauthorized`.

### FR-AUTH-004: Làm mới Access Token (Token Refresh)
- **Mã yêu cầu:** FR-AUTH-004
- **Tên yêu cầu:** Làm mới Access Token bằng Refresh Token
- **Nhóm chức năng:** Module Xác thực và Quản lý Người dùng (FR-AUTH)
- **Tác nhân:** Tác giả (Author) / Quản trị viên (Admin) có refresh token hợp lệ
- **Mức ưu tiên (MoSCoW):** M - Must Have (Bắt buộc)
- **Mô tả:** Khi access token hết hạn (sau 15 phút), client sử dụng refresh token còn hiệu lực để lấy cặp token mới mà không cần người dùng đăng nhập lại. Cơ chế Token Rotation bắt buộc: mỗi lần refresh, refresh token cũ bị vô hiệu hóa (`IsRevoked = true`, `RevokedAt = DateTime.UtcNow`) và một refresh token MỚI được tạo ra. Đây là biện pháp chống Refresh Token Reuse Attack.
- **Điều kiện tiên quyết:**
  1. Client có refresh token hợp lệ (chưa hết hạn, chưa bị revoke, chưa bị thay thế).
  2. Người dùng tương ứng vẫn còn tồn tại trong database và chưa bị khóa.
- **Luồng chính (Happy Path):**
  1. Client gửi `POST /api/v1/auth/refresh` với body: `{"refreshToken": "..."}`.
  2. `RefreshTokenCommand` dispatch qua MediatR.
  3. Handler tìm refresh token trong database bao gồm User navigation property.
  4. Kiểm tra: token tồn tại, `IsRevoked == false`, `ExpiresAt > DateTime.UtcNow`, user vẫn active.
  5. Đánh dấu token cũ: `IsRevoked = true`, `ReplacedByToken = newToken`, `RevokedAt = DateTime.UtcNow`.
  6. Tạo access token mới cho user.
  7. Tạo refresh token mới, lưu vào database.
  8. Trả về HTTP 200 OK với `AuthResponseDto` chứa cặp token mới.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Refresh token không tìm thấy trong database:** HTTP 401 Unauthorized.
  - **A2 - Refresh token đã hết hạn:** HTTP 401 Unauthorized, client phải đăng nhập lại.
  - **A3 - Refresh token đã bị revoke (Reuse Attack detected):** HTTP 401 Unauthorized. LOG SECURITY ALERT với mức WARNING. Kích hoạt revoke toàn bộ refresh tokens của user đó (paranoid mode).
  - **A4 - User bị xóa hoặc bị khóa sau khi token được cấp:** HTTP 401 Unauthorized.
- **HTTP Method & Endpoint:** `POST /api/v1/auth/refresh`
- **Kết quả mong đợi:** Refresh token cũ bị invalidate. Access token mới (15 phút) và refresh token mới (7 ngày) được tạo và trả về.
- **HTTP Status Code:** `200 OK`, `401 Unauthorized`.

### FR-AUTH-005: Đăng xuất (Logout / Token Revocation)
- **Mã yêu cầu:** FR-AUTH-005
- **Tên yêu cầu:** Đăng xuất và Thu hồi Refresh Token
- **Nhóm chức năng:** Module Xác thực và Quản lý Người dùng (FR-AUTH)
- **Tác nhân:** Tác giả (Author) / Quản trị viên (Admin) đang đăng nhập
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Người dùng đăng xuất khỏi hệ thống. Vì JWT access token là stateless (không thể revoke trực tiếp trước khi hết hạn), hành động logout chủ yếu là revoke refresh token tương ứng trong database. Client có trách nhiệm xóa access token khỏi bộ nhớ (localStorage/cookie) phía client.
- **Điều kiện tiên quyết:**
  1. Người dùng đang đăng nhập với access token hợp lệ trong Authorization header.
  2. Client gửi refresh token muốn revoke.
- **Luồng chính (Happy Path):**
  1. Client gửi `POST /api/v1/auth/logout` với Authorization: `Bearer {accessToken}` header và body: `{"refreshToken": "..."}`.
  2. Middleware xác thực JWT (`UseAuthentication`) xác minh access token.
  3. `LogoutCommandHandler` tìm refresh token trong database.
  4. Nếu tìm thấy và thuộc về user hiện tại: đánh dấu `IsRevoked = true`, `RevokedAt = DateTime.UtcNow`.
  5. Lưu thay đổi vào database.
  6. Trả về HTTP 204 No Content.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Refresh token không tìm thấy:** Vẫn trả về HTTP 204 (idempotent – không tiết lộ trạng thái).
  - **A2 - Access token đã hết hạn:** Vẫn cho phép logout nếu refresh token hợp lệ; hoặc HTTP 401 nếu không cung cấp refresh token.
- **HTTP Method & Endpoint:** `POST /api/v1/auth/logout`
- **Kết quả mong đợi:** Refresh token bị đánh dấu `IsRevoked = true` trong database. Các lần refresh tiếp theo với token này sẽ thất bại.
- **HTTP Status Code:** `204 No Content`, `401 Unauthorized`.

### FR-AUTH-006: Xem Hồ sơ Cá nhân (View Profile)
- **Mã yêu cầu:** FR-AUTH-006
- **Tên yêu cầu:** Xem Hồ sơ Cá nhân
- **Nhóm chức năng:** Module Xác thực và Quản lý Người dùng (FR-AUTH)
- **Tác nhân:** Tác giả (Author) / Quản trị viên (Admin) đang đăng nhập
- **Mức ưu tiên (MoSCoW):** S - Should Have
- **Mô tả:** Trả về thông tin hồ sơ của người dùng hiện đang đăng nhập, dựa trên UserId được trích xuất từ JWT claims. Không bao giờ trả về Password Hash hoặc SecurityStamp.
- **Điều kiện tiên quyết:** Người dùng đang đăng nhập với access token hợp lệ.
- **Luồng chính (Happy Path):**
  1. Client gửi `GET /api/v1/auth/me` với Authorization: `Bearer {accessToken}`.
  2. Middleware xác thực JWT, trích xuất UserId từ claim `NameIdentifier`.
  3. `GetCurrentUserQuery` dispatch qua MediatR.
  4. Handler tìm user: `UserManager.FindByIdAsync(userId)`.
  5. Map sang `UserProfileDto`: `{ id, fullName, email, userName, avatarUrl, roles, emailConfirmed, createdAt }`.
  6. Trả về HTTP 200 OK với `UserProfileDto`.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - User đã bị xóa khỏi database sau khi token được cấp:** HTTP 404 Not Found.
- **HTTP Method & Endpoint:** `GET /api/v1/auth/me`
- **Kết quả mong đợi:** Trả về thông tin hồ sơ đầy đủ của người dùng (không có thông tin nhạy cảm như password hash).
- **HTTP Status Code:** `200 OK`, `401 Unauthorized`, `404 Not Found`.

### FR-AUTH-007: Cập nhật Hồ sơ Cá nhân (Update Profile)
- **Mã yêu cầu:** FR-AUTH-007
- **Tên yêu cầu:** Cập nhật Hồ sơ Cá nhân
- **Nhóm chức năng:** Module Xác thực và Quản lý Người dùng (FR-AUTH)
- **Tác nhân:** Tác giả (Author) / Quản trị viên (Admin) đang đăng nhập
- **Mức ưu tiên (MoSCoW):** S - Should Have
- **Mô tả:** Người dùng có thể cập nhật `FullName` và `AvatarUrl` của mình. Email và UserName không thể thay đổi qua endpoint này. Sử dụng PATCH (partial update) để chỉ cập nhật các field được cung cấp.
- **Điều kiện tiên quyết:**
  1. Người dùng đang đăng nhập.
  2. Dữ liệu mới phải hợp lệ (`FullName` không rỗng, `AvatarUrl` là URL hợp lệ nếu cung cấp).
- **Luồng chính (Happy Path):**
  1. Client gửi `PATCH /api/v1/auth/me` với body: `{"fullName": "...", "avatarUrl": "..."}`.
  2. `UpdateProfileCommand` dispatch qua MediatR, UserId lấy từ JWT claims.
  3. `ValidationBehavior` kiểm tra: `fullName` 2-100 ký tự, `avatarUrl` là URL hợp lệ (nếu cung cấp).
  4. Handler tìm user, cập nhật FullName và/hoặc AvatarUrl.
  5. `UserManager.UpdateAsync(user)`.
  6. Trả về HTTP 200 OK với `UserProfileDto` đã cập nhật.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Dữ liệu không hợp lệ:** HTTP 422 Unprocessable Entity.
- **HTTP Method & Endpoint:** `PATCH /api/v1/auth/me`
- **Kết quả mong đợi:** Hồ sơ người dùng được cập nhật trong database. Trả về hồ sơ mới.
- **HTTP Status Code:** `200 OK`, `401 Unauthorized`, `422 Unprocessable Entity`.

---

## 3.2. Module Quản lý Danh mục (FR-CAT)

### FR-CAT-001: Xem Danh sách Danh mục
- **Mã yêu cầu:** FR-CAT-001
- **Tên yêu cầu:** Xem Danh sách Tất cả Danh mục
- **Nhóm chức năng:** Module Quản lý Danh mục (FR-CAT)
- **Tác nhân:** Tất cả (Guest / Author / Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Trả về danh sách tất cả danh mục công thức hiện có trong hệ thống, kèm số lượng công thức đã xuất bản (Published) trong mỗi danh mục. Kết quả được cache với `IMemoryCache` (TTL 60 phút) và sắp xếp theo Name tăng dần.
- **Điều kiện tiên quyết:**
  1. Ít nhất một danh mục tồn tại trong database (hoặc trả về mảng rỗng).
  2. Không yêu cầu xác thực.
- **Luồng chính (Happy Path):**
  1. Client gửi `GET /api/v1/categories`.
  2. `GetCategoriesQuery` dispatch qua MediatR.
  3. Handler kiểm tra `IMemoryCache` với key `"categories:all"`.
  4. Cache hit: trả về dữ liệu từ cache.
  5. Cache miss: query database (`_unitOfWork.Categories.GetAllWithRecipeCount()`), map sang `CategoryDto[]`.
  6. Lưu vào `IMemoryCache` với TTL 60 phút.
  7. Trả về HTTP 200 OK với `CategoryDto[]`.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Không có danh mục nào:** HTTP 200 OK với mảng rỗng `[]`.
- **HTTP Method & Endpoint:** `GET /api/v1/categories`
- **Kết quả mong đợi:** Mảng `CategoryDto[]` với các field: `{ id, name, slug, description, recipeCount }`. Kết quả được serve từ cache khi có.
- **HTTP Status Code:** `200 OK`.

### FR-CAT-002: Xem Chi tiết Danh mục và Công thức
- **Mã yêu cầu:** FR-CAT-002
- **Tên yêu cầu:** Xem Chi tiết Danh mục và Danh sách Công thức thuộc Danh mục
- **Nhóm chức năng:** Module Quản lý Danh mục (FR-CAT)
- **Tác nhân:** Tất cả (Guest / Author / Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Trả về thông tin chi tiết của một danh mục cụ thể (theo Slug) kèm danh sách phân trang các công thức đã xuất bản (Published) thuộc danh mục đó. Guest chỉ thấy Published recipes; Author thấy thêm Draft recipes của chính mình trong danh mục.
- **Điều kiện tiên quyết:** Danh mục với slug tương ứng phải tồn tại.
- **Luồng chính (Happy Path):**
  1. Client gửi `GET /api/v1/categories/{slug}?page=1&pageSize=12`.
  2. `GetCategoryBySlugQuery` dispatch qua MediatR.
  3. Handler tìm category theo slug: `_unitOfWork.Categories.GetBySlugAsync(slug)`.
  4. Query recipes thuộc category với `Status == Published` (+ Draft của currentUser nếu đã đăng nhập).
  5. Apply pagination (OFFSET-based: `SKIP (page-1)*pageSize TAKE pageSize`).
  6. Map sang Category DetailDto kèm `PagedResult<RecipeSummaryDto>`.
  7. Trả về HTTP 200 OK.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Slug không tồn tại:** HTTP 404 Not Found với RFC 7807 body.
- **HTTP Method & Endpoint:** `GET /api/v1/categories/{slug}?page={n}&pageSize={n}`
- **Kết quả mong đợi:** `{ category: CategoryDto, recipes: { items: RecipeSummaryDto[], totalCount, page, pageSize, totalPages } }`
- **HTTP Status Code:** `200 OK`, `404 Not Found`.

### FR-CAT-003: Tạo Danh mục Mới [Admin]
- **Mã yêu cầu:** FR-CAT-003
- **Tên yêu cầu:** Tạo Danh mục Công thức Mới
- **Nhóm chức năng:** Module Quản lý Danh mục (FR-CAT)
- **Tác nhân:** Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Admin tạo danh mục công thức mới. Slug được tự động sinh từ Name (slugify: chuyển sang chữ thường, bỏ dấu, thay khoảng trắng bằng `-`). Nếu Slug đã tồn tại, hệ thống thêm suffix số (e.g., `"mon-chinh-2"`). Sau khi tạo, cache danh mục (`IMemoryCache` key `"categories:all"`) bị invalidate.
- **Điều kiện tiên quyết:**
  1. Người dùng đang đăng nhập với role Admin.
  2. Name chưa tồn tại trong database.
- **Luồng chính (Happy Path):**
  1. Admin gửi `POST /api/v1/categories` với Authorization: `Bearer {adminJwt}` và body: `{"name": "...", "description": "..."}`.
  2. `RequireAuthorization("Admin")` middleware kiểm tra role.
  3. `CreateCategoryCommand` dispatch qua MediatR.
  4. `ValidationBehavior`: `name` 2-50 ký tự, không chứa HTML.
  5. `SlugHelper.Generate(name)` tạo slug.
  6. Kiểm tra slug chưa tồn tại. Nếu trùng, thêm `"-2"`, `"-3"`... cho đến khi unique.
  7. `Category.Create(name, slug, description)` tạo entity.
  8. `_unitOfWork.Categories.AddAsync(entity)`.
  9. `_unitOfWork.SaveChangesAsync()`.
  10. `_memoryCache.Remove("categories:all")` – invalidate cache.
  11. Trả về HTTP 201 Created với `CategoryDto` và `Location` header.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Thiếu role Admin:** HTTP 403 Forbidden.
  - **A2 - Dữ liệu không hợp lệ:** HTTP 422 Unprocessable Entity.
- **HTTP Method & Endpoint:** `POST /api/v1/categories`
- **Kết quả mong đợi:** Danh mục mới được tạo trong database. Cache danh mục bị xóa. Location header trỏ đến `/api/v1/categories/{newSlug}`.
- **HTTP Status Code:** `201 Created`, `403 Forbidden`, `409 Conflict`, `422 Unprocessable Entity`.

### FR-CAT-004: Cập nhật Danh mục [Admin]
- **Mã yêu cầu:** FR-CAT-004
- **Tên yêu cầu:** Cập nhật Thông tin Danh mục
- **Nhóm chức năng:** Module Quản lý Danh mục (FR-CAT)
- **Tác nhân:** Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Admin cập nhật Name và/hoặc Description của danh mục. Slug KHÔNG thay đổi khi đổi tên (để tránh broken links). Sau khi cập nhật, cache bị invalidate.
- **Điều kiện tiên quyết:** Admin đang đăng nhập. Danh mục với ID tương ứng tồn tại.
- **Luồng chính (Happy Path):**
  1. Admin gửi `PUT /api/v1/categories/{id}` với body: `{"name": "...", "description": "..."}`.
  2. Kiểm tra role Admin.
  3. `UpdateCategoryCommand` dispatch.
  4. Tìm category theo ID, cập nhật Name và Description.
  5. Lưu thay đổi, invalidate cache.
  6. Trả về HTTP 200 OK với `CategoryDto` đã cập nhật.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - ID không tồn tại:** HTTP 404 Not Found.
  - **A2 - Thiếu role Admin:** HTTP 403 Forbidden.
- **HTTP Method & Endpoint:** `PUT /api/v1/categories/{id:guid}`
- **Kết quả mong đợi:** Thông tin danh mục được cập nhật. Cache invalidated.
- **HTTP Status Code:** `200 OK`, `403 Forbidden`, `404 Not Found`, `422 Unprocessable Entity`.

### FR-CAT-005: Xóa Danh mục [Admin]
- **Mã yêu cầu:** FR-CAT-005
- **Tên yêu cầu:** Xóa Danh mục
- **Nhóm chức năng:** Module Quản lý Danh mục (FR-CAT)
- **Tác nhân:** Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** S - Should Have
- **Mô tả:** Admin xóa một danh mục. Quy tắc nghiệp vụ: KHÔNG được xóa danh mục còn chứa công thức (dù là Published hay Draft). Admin phải chuyển tất cả công thức sang danh mục khác trước khi xóa.
- **Điều kiện tiên quyết:** Admin đang đăng nhập. Danh mục tồn tại và không còn công thức nào.
- **Luồng chính (Happy Path):**
  1. Admin gửi `DELETE /api/v1/categories/{id}`.
  2. Kiểm tra role Admin.
  3. `DeleteCategoryCommand` dispatch.
  4. Đếm số recipe trong category: nếu `> 0` -> Throw `ConflictException("Danh mục còn chứa {count} công thức.")`.
  5. Xóa entity, lưu thay đổi, invalidate cache.
  6. Trả về HTTP 204 No Content.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Danh mục có recipe:** HTTP 409 Conflict với thông báo số lượng recipe.
  - **A2 - ID không tồn tại:** HTTP 404 Not Found.
- **HTTP Method & Endpoint:** `DELETE /api/v1/categories/{id:guid}`
- **Kết quả mong đợi:** Danh mục bị xóa khỏi database. HTTP 204 được trả về.
- **HTTP Status Code:** `204 No Content`, `403 Forbidden`, `404 Not Found`, `409 Conflict`.

---

## 3.3. Module Quản lý Công thức Nấu ăn (FR-RCP)

### FR-RCP-001: Xem Danh sách Công thức (Paginated + Filtered + Sorted)
- **Mã yêu cầu:** FR-RCP-001
- **Tên yêu cầu:** Xem Danh sách Công thức Nấu ăn với Phân trang, Lọc và Sắp xếp
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tất cả (Guest / Author / Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Trả về danh sách phân trang các công thức. Guest và Author khác chỉ thấy `Status == Published`. Author thấy thêm Draft/Archived của chính mình. Admin thấy tất cả trạng thái. Hỗ trợ lọc theo CategoryId, Difficulty, thời gian nấu; sắp xếp theo `createdAt`, `title`, `cookTime`. Kết quả được cache với Output Cache (.NET 10) policy `"RecipeList"` (TTL 15 phút, vary by query string).
- **Điều kiện tiên quyết:** Tham số `page >= 1`, `pageSize` trong `[1..50]`.
- **Luồng chính (Happy Path):**
  1. Client gửi `GET /api/v1/recipes?page=1&pageSize=12&categoryId={guid}&difficulty=Easy&maxCookTime=30&sort=-createdAt`.
  2. `GetRecipesQuery` dispatch qua MediatR.
  3. Handler xây dựng `IQueryable` với filters từ query params.
  4. Áp dụng Authorization filter: nếu Guest -> chỉ Published; nếu Author -> Published OR (Draft AND AuthorId == userId); nếu Admin -> tất cả.
  5. Apply sorting: `sort="-createdAt"` -> `ORDER BY CreatedAt DESC`.
  6. COUNT total trước khi pagination.
  7. Apply OFFSET-LIMIT pagination.
  8. Map sang `PagedResult<RecipeSummaryDto>`.
  9. Trả về HTTP 200 OK.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - page hoặc pageSize không hợp lệ:** HTTP 422 Unprocessable Entity.
  - **A2 - categoryId không tồn tại:** HTTP 200 với items rỗng.
- **HTTP Method & Endpoint:** `GET /api/v1/recipes?page={n}&pageSize={n}&categoryId={guid}&difficulty={level}&maxCookTime={min}&sort={field}`
- **Kết quả mong đợi:** `PagedResult<RecipeSummaryDto>`: `{ items[], totalCount, page, pageSize, totalPages, hasNextPage, hasPreviousPage }`.
- **HTTP Status Code:** `200 OK`, `422 Unprocessable Entity`.

### FR-RCP-002: Xem Chi tiết Công thức
- **Mã yêu cầu:** FR-RCP-002
- **Tên yêu cầu:** Xem Chi tiết Công thức Nấu ăn
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tất cả (Guest / Author / Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Trả về toàn bộ thông tin chi tiết của một công thức cụ thể: thông tin cơ bản, danh sách nguyên liệu (`RecipeIngredient[]`) sắp xếp theo SortOrder, các bước thực hiện (`RecipeStep[]`) sắp xếp theo StepNumber, ảnh minh họa (`RecipeImage[]`), thông tin dinh dưỡng (`RecipeNutrition`), thông tin danh mục và tác giả. Recipe Draft/Archived chỉ được xem bởi tác giả sở hữu hoặc Admin.
- **Điều kiện tiên quyết:** Recipe với slug tương ứng tồn tại.
- **Luồng chính (Happy Path):**
  1. Client gửi `GET /api/v1/recipes/{slug}`.
  2. `GetRecipeBySlugQuery` dispatch.
  3. Handler query Recipe với Eager Loading: `Include(Steps)`, `Include(Ingredients)`, `Include(Images)`, `Include(Category)`, `Include(Author)`.
  4. Kiểm tra null -> `NotFoundException` nếu không tìm thấy.
  5. Kiểm tra Status: nếu Draft/Archived -> chỉ tác giả hoặc Admin mới được xem.
  6. Map sang `RecipeDetailDto`.
  7. Trả về HTTP 200 OK.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Slug không tồn tại:** HTTP 404 Not Found.
  - **A2 - Recipe Draft/Archived, người dùng không có quyền:** HTTP 403 Forbidden.
- **HTTP Method & Endpoint:** `GET /api/v1/recipes/{slug}`
- **Kết quả mong đợi:** `RecipeDetailDto` đầy đủ gồm tất cả nested data.
- **HTTP Status Code:** `200 OK`, `403 Forbidden`, `404 Not Found`.

### FR-RCP-003: Tạo Công thức Nấu ăn Mới [Author/Admin]
- **Mã yêu cầu:** FR-RCP-003
- **Tên yêu cầu:** Tạo Công thức Nấu ăn Mới
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tác giả (Author) / Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Author hoặc Admin tạo mới một công thức nấu ăn. Trạng thái ban đầu luôn là Draft. Slug được tự động sinh từ Title.
- **Điều kiện tiên quyết:** Người dùng đang đăng nhập với role Author hoặc Admin. CategoryId tồn tại.
- **Luồng chính (Happy Path):**
  1. Author gửi `POST /api/v1/recipes` với body: `{ title, description, categoryId, prepTimeMinutes, cookTimeMinutes, servings, difficulty, instructions?, nutrition?: {...}, steps?: [...], ingredients?: [...] }`.
  2. `CreateRecipeCommand` dispatch.
  3. `ValidationBehavior`: `title` 5-200 ký tự, `prepTime`/`cookTime`/`servings` > 0, `categoryId` valid Guid.
  4. `SlugHelper.Generate(title)`, kiểm tra slug unique.
  5. `Recipe.Create(...)` entity.
  6. Nếu có steps/ingredients/nutrition: thêm vào entity.
  7. `_unitOfWork.Recipes.AddAsync(recipe)`, `SaveChangesAsync()`.
  8. Invalidate Output Cache tag `"recipes"`.
  9. Trả về HTTP 201 Created với `RecipeDto`.
- **Luồng thay thế / Ngoại lệ:**
  - **A1 - Không có quyền Author/Admin:** HTTP 401/403.
  - **A2 - CategoryId không tồn tại:** HTTP 422.
  - **A3 - Slug trùng:** HTTP 409 Conflict.
- **HTTP Method & Endpoint:** `POST /api/v1/recipes`
- **Kết quả mong đợi:** Recipe mới được tạo với `Status = Draft`.
- **HTTP Status Code:** `201 Created`, `401 Unauthorized`, `403 Forbidden`, `409 Conflict`, `422 Unprocessable Entity`.

### FR-RCP-004: Cập nhật Công thức [Author-Owner/Admin]
- **Mã yêu cầu:** FR-RCP-004
- **Tên yêu cầu:** Cập nhật Thông tin Công thức Nấu ăn
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tác giả sở hữu (Author - Owner) / Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Cập nhật thông tin của một công thức. Chỉ Author sở hữu recipe (`AuthorId == currentUserId`) hoặc Admin được phép. Concurrency control qua RowVersion.
- **HTTP Method & Endpoint:** `PUT /api/v1/recipes/{id:guid}`
- **HTTP Status Code:** `200 OK`, `403 Forbidden`, `404 Not Found`, `409 Conflict`, `422 Unprocessable Entity`.

### FR-RCP-005: Xuất bản / Hủy Xuất bản Công thức
- **Mã yêu cầu:** FR-RCP-005
- **Tên yêu cầu:** Xuất bản (Publish) / Hủy Xuất bản (Unpublish) Công thức
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tác giả sở hữu (Author - Owner) / Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Thay đổi trạng thái công thức: Draft -> Published hoặc Published -> Draft. Business rule: KHÔNG thể publish nếu recipe không có ít nhất 1 bước thực hiện (`Steps.Count > 0`).
- **HTTP Method & Endpoint:** `PATCH /api/v1/recipes/{id:guid}/publish` | `PATCH /api/v1/recipes/{id:guid}/unpublish`
- **HTTP Status Code:** `200 OK`, `403 Forbidden`, `404 Not Found`, `422 Unprocessable Entity`.

### FR-RCP-006: Lưu trữ Công thức (Archive)
- **Mã yêu cầu:** FR-RCP-006
- **Tên yêu cầu:** Lưu trữ Công thức (Archive / Unarchive)
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tác giả sở hữu / Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** S - Should Have
- **Mô tả:** Chuyển Recipe sang trạng thái Archived. Recipe Archived không hiển thị trong danh sách công khai nhưng không bị xóa khỏi database.
- **HTTP Method & Endpoint:** `PATCH /api/v1/recipes/{id:guid}/archive`
- **HTTP Status Code:** `200 OK`, `403 Forbidden`, `404 Not Found`.

### FR-RCP-007: Xóa Công thức [Author-Owner/Admin]
- **Mã yêu cầu:** FR-RCP-007
- **Tên yêu cầu:** Xóa Vĩnh viễn Công thức Nấu ăn
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tác giả sở hữu (Author - Owner) / Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Xóa vĩnh viễn một công thức và tất cả dữ liệu liên quan (cascade delete: Steps, Ingredients, Images). Các file ảnh trên MinIO được xóa bất đồng bộ qua Hangfire job.
- **HTTP Method & Endpoint:** `DELETE /api/v1/recipes/{id:guid}`
- **HTTP Status Code:** `204 No Content`, `403 Forbidden`, `404 Not Found`.

### FR-RCP-008: Quản lý Ảnh Công thức (Upload / Set Primary / Delete)
- **Mã yêu cầu:** FR-RCP-008
- **Tên yêu cầu:** Upload Ảnh, Đặt Ảnh Chính, Xóa Ảnh Công thức
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tác giả sở hữu / Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Upload dùng `multipart/form-data`. Ảnh được lưu trên MinIO với path: `recipes/{recipeId}/{uuid}.{ext}`. Ảnh đầu tiên tự động được đặt làm ảnh chính (`IsPrimary = true`). Validation bắt buộc: MIME type (`image/jpeg`, `image/png`, `image/webp`, `image/avif`) và kích thước tối đa 5MB.
- **HTTP Method & Endpoint:** 
  - `POST /api/v1/recipes/{id}/images`
  - `PATCH /api/v1/recipes/{id}/images/{imgId}/primary`
  - `DELETE /api/v1/recipes/{id}/images/{imgId}`
- **HTTP Status Code:** `201 Created` (Upload), `200 OK` (Set Primary), `204 No Content` (Delete), `400 Bad Request`, `403 Forbidden`, `404 Not Found`.

### FR-RCP-009: Quản lý Nguyên liệu (CRUD RecipeIngredient)
- **Mã yêu cầu:** FR-RCP-009
- **Tên yêu cầu:** Thêm / Cập nhật / Xóa Nguyên liệu Công thức
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tác giả sở hữu / Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Quản lý danh sách nguyên liệu (`RecipeIngredient`). Mỗi nguyên liệu có: Name, Quantity, Unit, Notes, SortOrder.
- **HTTP Method & Endpoint:** `POST/PUT/DELETE /api/v1/recipes/{id}/ingredients/{ingId?}`
- **HTTP Status Code:** `201 Created`, `200 OK`, `204 No Content`, `403 Forbidden`, `404 Not Found`, `422 Unprocessable Entity`.

### FR-RCP-010: Quản lý Các bước Thực hiện (CRUD RecipeStep)
- **Mã yêu cầu:** FR-RCP-010
- **Tên yêu cầu:** Thêm / Cập nhật / Xóa Bước Thực hiện Công thức
- **Nhóm chức năng:** Module Quản lý Công thức Nấu ăn (FR-RCP)
- **Tác nhân:** Tác giả sở hữu / Quản trị viên (Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Quản lý các bước thực hiện (`RecipeStep`). Mỗi bước có: StepNumber, Title, Description, DurationMinutes, ImageUrl. Khi xóa một bước, hệ thống tự động renumber để giữ StepNumber liên tục.
- **HTTP Method & Endpoint:** `POST/PUT/DELETE /api/v1/recipes/{id}/steps/{stepId?}`
- **HTTP Status Code:** `201 Created`, `200 OK`, `204 No Content`, `403 Forbidden`, `404 Not Found`, `422 Unprocessable Entity`.

---

## 3.4. Module Tìm kiếm và Phân trang (FR-SRCH)

### FR-SRCH-001: Tìm kiếm Toàn văn bản (Full-Text Search)
- **Mã yêu cầu:** FR-SRCH-001
- **Tên yêu cầu:** Tìm kiếm Toàn văn bản Công thức (Full-Text Search)
- **Nhóm chức năng:** Module Tìm kiếm và Phân trang (FR-SRCH)
- **Tác nhân:** Tất cả (Guest / Author / Admin)
- **Mức ưu tiên (MoSCoW):** M - Must Have
- **Mô tả:** Tìm kiếm toàn văn bản (FTS) cho công thức sử dụng PostgreSQL `tsvector`/`tsquery` với cấu hình tiếng Việt. Kết quả được xếp hạng bởi `ts_rank()`. Hỗ trợ tìm kiếm gần đúng với `unaccent` extension (bỏ dấu tiếng Việt: "pho" tìm được "phở").
- **HTTP Method & Endpoint:** `GET /api/v1/recipes/search?q={searchTerm}&page={n}&pageSize={n}`
- **HTTP Status Code:** `200 OK`, `422 Unprocessable Entity`.

### FR-SRCH-002/003/004: Lọc, Sắp xếp và Phân trang (Tóm tắt)
| Mã FR | Tên | Tham số Query | Mô tả |
| :--- | :--- | :--- | :--- |
| **FR-SRCH-002** | Lọc công thức | `categoryId={guid}`, `difficulty={level}`, `maxCookTime={min}`, `minServings={n}` | Lọc kết quả theo một hoặc nhiều tiêu chí. Các filter kết hợp bằng AND logic. |
| **FR-SRCH-003** | Sắp xếp kết quả | `sort={field}` (Ví dụ: `sort=-createdAt`, `sort=title`) | Tiền tố "-" = descending. Mặc định: `sort=-createdAt` (mới nhất trước). |
| **FR-SRCH-004** | Phân trang | `page={n}` (default: 1), `pageSize={n}` (default: 12, max: 50) | Offset-based pagination (SKIP/TAKE). Response bao gồm metadata phân trang. |

---

## 3.5. Module Quản lý Tệp tin (FR-FILE)

| Mã FR | Tên | Mô tả | Ràng buộc kỹ thuật |
| :--- | :--- | :--- | :--- |
| **FR-FILE-001** | Upload File lên MinIO | `IFileStorageService.UploadAsync(...)` -> string (public URL). Tạo unique filename = `{folder}/{Guid.NewGuid()}{ext}` để ngăn path traversal. | Max size: 5MB. MIME: JPEG/PNG/WebP/AVIF. Magic bytes validation. Bucket: `"culinary-blog"`. Policy: `public-read`. |
| **FR-FILE-002** | Xóa File khỏi MinIO | `IFileStorageService.DeleteAsync(...)`. Trích xuất object name từ URL, gọi `RemoveObjectAsync()`. Gọi từ Hangfire background job. | Nếu object không tồn tại -> không throw exception (idempotent). Lỗi kết nối -> retry tối đa 3 lần. |

---

## 3.6. Module Background Jobs (FR-JOB)

| Mã FR | Tên Job | Loại | Trigger | Mô tả | Retry Policy |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **FR-JOB-001** | Welcome Email Job | Fire-and-forget | Sau FR-AUTH-001 thành công | Gửi email HTML chào mừng đến địa chỉ email vừa đăng ký. | Retry 3 lần với exponential backoff (1m, 5m, 30m). Sau 3 lần fail -> Failed state, log error. |
| **FR-JOB-002** | Image Resize / Thumbnail Job | Fire-and-forget | Sau FR-RCP-008 upload ảnh thành công | Tạo thumbnail (300x300px) và medium image (800x600px) từ ảnh gốc. Lưu cả 3 phiên bản lên MinIO. | Retry 3 lần. Nếu fail: ảnh gốc vẫn hiển thị, chỉ thiếu thumbnail. |
| **FR-JOB-003** | Sitemap Generation Job | Recurring | Hàng ngày lúc 02:00 AM UTC (`cron: "0 2 * * *"`) | Tạo file `sitemap.xml` chứa URL tất cả Published recipes, categories và pages tĩnh. Ping Google Search Console. | Retry 2 lần nếu fail. Log kết quả qua Serilog. |

---

## 3.7. Module Quan sát Hệ thống (FR-OBS)

| Mã FR | Tên | Mô tả | Kỹ thuật / Công cụ |
| :--- | :--- | :--- | :--- |
| **FR-OBS-001** | Health Check Endpoints | 3 endpoints: `GET /health` (tổng hợp), `GET /health/live` (Liveness), `GET /health/ready` (Readiness). | `IHealthCheck`, `AspNetCore.HealthChecks.NpgSql`, `AspNetCore.HealthChecks.Redis`, `AspNetCore.HealthChecks.Minio`. |
| **FR-OBS-002** | Structured Logging | Mọi HTTP request được log với CorrelationId, Method, Path, Status, Duration, UserId. MediatR Pipeline Behavior log all commands/queries. | Serilog + CorrelationId Middleware. Sinks: Console (JSON), File (rolling daily), Seq. Log levels: Debug (dev), Information (prod). |
| **FR-OBS-003** | Distributed Tracing & Metrics | OpenTelemetry instrumentation: HTTP traces, EF Core DB traces, custom metrics (recipe created/published count). | OpenTelemetry .NET SDK, OTLP exporter. Traces export sang Seq (dev) hoặc Jaeger/Grafana (prod). |

---

# CHƯƠNG 4. YÊU CẦU PHI CHỨC NĂNG (NFR)

## 4.1. Hiệu năng (NFR-PERF)
- **NFR-PERF-001 (Response Time API):** p50 $\le 150$ms cho tất cả GET endpoints với dữ liệu cache. p95 $\le 500$ms cho tất cả API endpoints (kể cả write operations). p99 $\le 1000$ms.
- **NFR-PERF-002 (Throughput):** Xử lý đồng thời $\ge 100$ concurrent users không degradation trên VPS 2 vCPU, 4GB RAM.
- **NFR-PERF-003 (Cache Effectiveness):** Redis Cache hit rate $\ge 80\%$ trong điều kiện steady-state.
- **NFR-PERF-004 (Database Query):** Không có N+1 query problem (dùng `Include()` / `ThenInclude()` / Projection). Index đầy đủ trên các cột `WHERE` / `ORDER BY`. Log cảnh báo khi query $> 100$ms.
- **NFR-PERF-005 (Frontend Core Web Vitals):** LCP $\le 2.5$s, CLS $\le 0.1$, INP $\le 200$ms. JS Bundle ban đầu $\le 200$KB (gzipped).

## 4.2. Bảo mật (NFR-SEC)
- **NFR-SEC-001 (Password & Hashing):** PBKDF2-HMACSHA512 với iteration count $\ge 100.000$. Mật khẩu $\ge 8$ ký tự, gồm chữ hoa, chữ thường, số, ký tự đặc biệt.
- **NFR-SEC-002 (JWT Token Security):** Access Token (HS256, TTL 15m). Refresh Token (256-bit random, SHA-256 hash in DB, TTL 7d, Rotation + Reuse Detection).
- **NFR-SEC-003 (Rate Limiting):** Auth endpoints: 10 req/min/IP. Upload: 5 req/min/IP. General API: 100 req/min/IP. HTTP 429 kèm `Retry-After`.
- **NFR-SEC-004 (Input Validation & File Upload Security):** FluentValidation cho mọi input. Parameterized queries ngăn SQL Injection. Magic bytes checking cho file upload. GUID-based filename ngăn Path Traversal.
- **NFR-SEC-005 (HTTPS & CORS):** Bắt buộc HTTPS (TLS 1.2+). CORS whitelist theo domain cấu hình (không wildcard `*`).
- **NFR-SEC-006 (Authorization & Resource Ownership):** Kiểm tra phân quyền tại Application Layer. Author chỉ sửa/xóa dữ liệu của chính mình. Audit log mọi thao tác ghi.
- **NFR-SEC-007 (Secrets Management):** Dùng User Secrets trong Dev, Environment Variables / Secrets Store trong Prod. Không commit secret lên Git.

## 4.3. Khả năng Sử dụng (NFR-USE)
- **NFR-USE-001 (Responsive Design):** Mobile (320px-767px), Tablet (768px-1199px), Desktop ($\ge 1200$px) hiển thị chuẩn mực bằng Tailwind CSS.
- **NFR-USE-002 (Accessibility - a11y):** Tuân thủ WCAG 2.1 Level AA. HTML5 Semantic, ARIA attributes đầy đủ, Keyboard Nav, Contrast Ratio $\ge 4.5:1$.
- **NFR-USE-003 (Error Messages):** API trả lỗi chuẩn RFC 7807. UI hiển thị lỗi inline cạnh form field.
- **NFR-USE-004 (Loading States):** Skeleton loader khi fetch dữ liệu. Toast notification cho hành động thành công/thất bại. Real-time progress bar khi upload ảnh.

## 4.4. Độ tin cậy (NFR-REL)
- **NFR-REL-001 (Uptime SLA):** Uptime $\ge 99.5\%$ ($\le 3.65$ giờ downtime/năm). Health check probe mỗi 10 giây.
- **NFR-REL-002 (Error Handling & Resilience):** Global Exception Middleware bắt unhandled error. Redis down -> graceful fallback về Direct DB Query. Hangfire retry 3 lần với exponential backoff.
- **NFR-REL-003 (Data Durability):** PostgreSQL WAL enabled. Automated database backup `pg_dump` hàng ngày 03:00 AM UTC, lưu trữ 30 ngày.

## 4.5. Khả năng Bảo trì (NFR-MAINT)
- **NFR-MAINT-001 (Code Quality):** StyleCop/EditorConfig cho C#, ESLint/Prettier cho TypeScript. Không compiler warning. Minimum 1 Code Reviewer approval per PR.
- **NFR-MAINT-002 (Test Coverage):** Unit Tests $\ge 80\%$ line coverage cho Application Layer. Integration Tests phủ 100% endpoints (Happy path + Error path).
- **NFR-MAINT-003 (Documentation):** `README.md` setup dev $< 5$ phút. API Doc tự động với Scalar UI tại `/scalar`. ARCHITECTURE.md + CHANGELOG.md.
- **NFR-MAINT-004 (Clean Architecture Compliance):** Domain Layer 0 external dependency. ArchUnit.NET tests để tự động kiểm tra vi phạm phụ thuộc tầng.

## 4.6. Khả năng Mở rộng (NFR-SCALE)
- **NFR-SCALE-001 (Stateless Backend):** REST API hoàn toàn stateless. Auth qua JWT. Shared state duy nhất nằm ở Redis và Database, dễ dàng scale horizontally.
- **NFR-SCALE-002 (Database Scaling):** Npgsql Connection Pooling. Thiết kế sẵn sàng hỗ trợ Read Replicas (EF Core Split Queries) và Partitioning theo thời gian (`CreatedAt`).
- **NFR-SCALE-003 (Infrastructure Scaling):** Đóng gói Docker container hoàn chỉnh. Nginx làm Reverse Proxy / Load Balancer. MinIO hỗ trợ Distributed Mode khi cần mở rộng storage.

## 4.7. Tối ưu SEO (NFR-SEO)
- **NFR-SEO-001 (Structured Data):** Tự động nhúng JSON-LD Schema.org `Recipe` markup trên trang chi tiết công thức.
- **NFR-SEO-002 (Meta Tags & Open Graph):** Dynamic `<title>`, `<meta name="description">`, Open Graph tags (`og:title`, `og:image`, `og:type`), Twitter Cards.
- **NFR-SEO-003 (Sitemap & Robots):** Tự động cập nhật `sitemap.xml` hàng ngày qua Hangfire Job. Cung cấp file `robots.txt` chuẩn SEO.
- **NFR-SEO-004 (URL Structure):** Semantic URLs thân thiện: `/recipes/{slug}` và `/categories/{slug}`. 301 Permanent Redirect khi slug thay đổi.

---

# CHƯƠNG 5. YÊU CẦU GIAO DIỆN NGOÀI

## 5.1. Giao diện Người dùng (UI)

| Màn hình / Route | Mô tả | Loại Rendering | Yêu cầu Auth |
| :--- | :--- | :--- | :--- |
| `/` | Trang chủ: công thức nổi bật + danh mục | ISR (revalidate = 3600) | Không |
| `/recipes` | Danh sách công thức + Filter/Sort/Search | SSR (dynamic) | Không |
| `/recipes/[slug]` | Chi tiết công thức: các bước, nguyên liệu, nutrition, JSON-LD | ISR (revalidate = 300) | Không |
| `/categories` | Danh sách tất cả danh mục | ISR (revalidate = 3600) | Không |
| `/categories/[slug]` | Danh sách công thức thuộc danh mục | ISR (revalidate = 600) | Không |
| `/auth/login` | Form đăng nhập (Email + Google OAuth) | CSR | Không (Redirect nếu đã đăng nhập) |
| `/auth/register` | Form đăng ký tài khoản | CSR | Không |
| `/dashboard` | Tổng quan cho Author/Admin | CSR | Bắt buộc (Author/Admin) |
| `/dashboard/recipes` | Quản lý danh sách recipe cá nhân | CSR | Bắt buộc (Author/Admin) |
| `/dashboard/recipes/new` | Form tạo mới recipe (Multi-step Wizard) | CSR | Bắt buộc (Author/Admin) |
| `/dashboard/recipes/[id]/edit` | Form chỉnh sửa recipe | CSR | Bắt buộc (Owner/Admin) |
| `/dashboard/categories` | Quản lý danh mục công thức | CSR | Bắt buộc (Admin) |
| `/profile` | Xem và cập nhật hồ sơ cá nhân | CSR | Bắt buộc |
| `/search` | Trang tìm kiếm công thức | SSR | Không |

## 5.2. Giao diện Phần mềm – REST API
- **Giao thức:** HTTP/1.1 & HTTP/2 over HTTPS (TLS 1.2+). Nginx termination SSL.
- **Base URL (Dev):** `http://localhost:5000/api/v1`
- **Base URL (Prod):** `https://api.culinaryblog.com/api/v1`
- **Content-Type:** `application/json; charset=utf-8`. `multipart/form-data` cho file upload.
- **Authentication:** `Authorization: Bearer <access_token>`.
- **Response Format:**
  - Success: `{"data": {...}, "meta": {"page": 1, "pageSize": 10, "total": 100}}`
  - Error: RFC 7807 `{"type": "...", "title": "...", "status": 400, "detail": "...", "errors": {...}}`
- **Versioning:** URL Path `/api/v1/`.
- **Rate Limit Headers:** `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`, `Retry-After`.
- **Correlation ID:** Header `X-Correlation-ID` sinh tự động hoặc nhận từ client, gắn vào mọi log context.

## 5.3. Giao diện Dịch vụ Bên thứ ba
- **Google OAuth 2.0:** Xử lý xác thực người dùng qua Google Sign-In (OpenID Connect).
- **MinIO Object Storage:** Lưu trữ file ảnh công thức và avatar qua AWS S3-compatible SDK.
- **Hangfire Server:** Quản lý và thực thi background jobs trong cùng process API.
- **Serilog + Seq:** Đẩy log hệ thống dạng cấu trúc (JSON) phục vụ monitoring.
- **OpenTelemetry Collector:** Thu thập Tracing/Metrics đẩy sang Jaeger/Grafana.
- **SMTP Service (MailKit):** Gửi email thông báo (Welcome Email) qua cổng SMTP mã hóa TLS.

## 5.4. Giao diện Phần cứng

| Thành phần | Development (local) | Production (minimum) |
| :--- | :--- | :--- |
| **CPU** | 2 cores (Intel/AMD/ARM64) | 2 vCPU (VPS/Cloud instance) |
| **RAM** | 8 GB (chạy full Docker stack) | 4 GB (API + dependencies) |
| **Storage** | 20 GB SSD | 50 GB SSD |
| **Network** | Internet Connection | Bandwidth $\ge 1$ Gbps, Public Static IP |

---

# CHƯƠNG 6. KIẾN TRÚC HỆ THỐNG

## 6.1. Tổng quan Kiến trúc
Hệ thống được thiết kế theo mô hình **Client-Server** với sự phân tách hoàn toàn giữa Frontend (Next.js 15 App Router) và Backend (.NET 10 Minimal APIs).

```
[ Client Browser ] 
       |
       v
[ Nginx Reverse Proxy ] (Port 80/443)
       |
       +---> [ Next.js Frontend ] (Port 3000) -- Web Pages (SSR/ISR/CSR)
       |
       +---> [ .NET 10 Backend API ] (Port 5000) -- REST API
                    |
                    +---> [ PostgreSQL 16 ] (Database)
                    +---> [ Redis 7 ] (Cache Layer)
                    +---> [ MinIO ] (Object Storage)
                    +---> [ SMTP Server ] (Email Service)
```

## 6.2. Kiến trúc Backend - Clean Architecture
Backend tuân thủ nghiêm ngặt Clean Architecture 4 tầng:
1. **Domain Layer (`CulinaryBlog.Domain`):** Chứa Core Entities, Value Objects, Domain Events, Enums, Interfaces. KHÔNG chứa bất kỳ phụ thuộc thư viện ngoài nào.
2. **Application Layer (`CulinaryBlog.Application`):** Chứa Commands, Queries, Handlers (MediatR), DTOs, FluentValidation Validators, Pipeline Behaviors, Interfaces.
3. **Infrastructure Layer (`CulinaryBlog.Infrastructure`):** Triển khai các Interfaces từ Application (EF Core DbContext, Repositories, Redis Cache, MinIO Storage, JWT Token Provider, MailKit Email Service).
4. **Presentation Layer (`CulinaryBlog.API`):** Minimal API Endpoints, Middlewares (Global Exception, CorrelationId, RateLimiting), DI Configuration, Program.cs.

## 6.3. CQRS + MediatR Pipeline
Các HTTP Requests ghi (POST/PUT/PATCH/DELETE) và đọc (GET) được tách thành **Commands** và **Queries**. Mỗi Request gửi qua MediatR sẽ chạy qua Pipeline Behaviors theo thứ tự:

```
[ HTTP Request ] 
       |
       v
1. [ LoggingBehavior ] -------> Log request parameters & duration
       |
2. [ ValidationBehavior ] ----> Run FluentValidation rules (Throw if invalid)
       |
3. [ CachingBehavior ] -------> Check Redis cache for Query (Return early if Hit)
       |
4. [ Handler Execution ] -----> Execute Core Business Logic
       |
5. [ CacheInvalidationBehavior ] -> Evict relative cache tags for Command
       |
       v
[ HTTP Response ]
```

## 6.4. Mô hình Quan hệ Thực thể (ERD tóm tắt)
- `Recipe` (1) <---> (N) `RecipeStep` (Cascade Delete)
- `Recipe` (1) <---> (N) `RecipeIngredient` (Cascade Delete)
- `Recipe` (1) <---> (N) `RecipeImage` (Cascade Delete)
- `Recipe` (1) <---> (1) `RecipeNutrition` (Owned Entity - Inline columns)
- `Category` (1) <---> (N) `Recipe` (Restrict Delete)
- `ApplicationUser` (1) <---> (N) `Recipe` (Author)
- `ApplicationUser` (1) <---> (N) `RefreshToken` (Cascade Delete)

## 6.5. Triển khai – Docker Compose
Hệ thống triển khai bằng Docker Compose với các container:
- `nginx`: Reverse proxy & SSL termination.
- `api`: .NET 10 Web API.
- `frontend`: Next.js Standalone Node server.
- `postgres`: PostgreSQL 16 DB với unaccent module.
- `redis`: Redis 7 in-memory cache.
- `minio`: S3-compatible object storage.
- `seq`: Log server (Dev environment).

---

# CHƯƠNG 7. MÔ HÌNH DỮ LIỆU

## 7.1. BaseEntity (Abstract)
Tất cả thực thể trong hệ thống kế thừa từ `BaseEntity`.

| Column | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | `uuid` (Guid) | PRIMARY KEY, DEFAULT `gen_random_uuid()` | Khóa chính UUID v4. |
| **CreatedAt** | `timestamptz` | NOT NULL, DEFAULT `NOW()` | Thời điểm tạo bản ghi. |
| **UpdatedAt** | `timestamptz` | NULL | Thời điểm cập nhật gần nhất. |
| **IsDeleted** | `boolean` | NOT NULL, DEFAULT `false` | Soft delete flag (Global Query Filter). |
| **RowVersion** | `bytea` | NOT NULL, Concurrency Token | Xử lý Optimistic Concurrency Control. |

## 7.2. Recipe
Bảng chính lưu trữ thông tin công thức nấu ăn (`Recipes`).

| Column | Kiểu dữ liệu | Ràng buộc | Index | Mô tả |
| :--- | :--- | :--- | :--- | :--- |
| **Id** | `uuid` | PK | PK | (Kế thừa BaseEntity) |
| **Title** | `varchar(200)` | NOT NULL | - | Tiêu đề công thức. |
| **Slug** | `varchar(220)` | NOT NULL, UNIQUE | UNIQUE B-tree | URL-friendly slug. |
| **Description**| `text` | NOT NULL | - | Mô tả ngắn công thức. |
| **Instructions**| `text` | NOT NULL | - | Hướng dẫn chung. |
| **PrepTime** | `integer` | NOT NULL, CHECK > 0 | - | Thời gian chuẩn bị (phút). |
| **CookTime** | `integer` | NOT NULL, CHECK >= 0| - | Thời gian nấu (phút). |
| **Servings** | `integer` | NOT NULL, CHECK > 0 | - | Khẩu phần (người). |
| **Difficulty** | `smallint` | NOT NULL, DEFAULT 1 | B-tree | 1: Easy, 2: Medium, 3: Hard, 4: Expert. |
| **Status** | `smallint` | NOT NULL, DEFAULT 0 | B-tree | 0: Draft, 1: Published, 2: Archived. |
| **CategoryId** | `uuid` | FK -> Categories.Id | B-tree | Khóa ngoại danh mục. |
| **AuthorId** | `varchar(450)`| FK -> AspNetUsers.Id | B-tree | Khóa ngoại tác giả. |
| **SearchVector**| `tsvector` | NULL | GIN | Vector phục vụ Full-Text Search. |
| **PublishedAt** | `timestamptz` | NULL | B-tree | Thời điểm xuất bản công khai. |

### 7.2.1. Recipe Nutrition (Owned Entity)
Nhúng trực tiếp vào bảng `Recipes` với tiền tố `Nutrition_`.
- `Nutrition_Calories` (`decimal(8,2)`, Nullable)
- `Nutrition_Protein` (`decimal(8,2)`, Nullable)
- `Nutrition_Carbohydrates` (`decimal(8,2)`, Nullable)
- `Nutrition_Fat` (`decimal(8,2)`, Nullable)
- `Nutrition_Fiber` (`decimal(8,2)`, Nullable)
- `Nutrition_Sodium` (`decimal(8,2)`, Nullable)

## 7.3. RecipeStep
Bảng `RecipeSteps` lưu các bước thực hiện.

| Column | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | `uuid` | PK | Khóa chính UUID. |
| **RecipeId** | `uuid` | FK -> Recipes.Id (ON DELETE CASCADE) | Khóa ngoại công thức. |
| **StepNumber** | `integer` | NOT NULL, CHECK > 0 | Thứ tự bước thực hiện (1, 2, 3...). |
| **Title** | `varchar(200)` | NOT NULL | Tiêu đề từng bước. |
| **Description**| `text` | NOT NULL | Mô tả chi tiết bước thực hiện. |
| **TimerMinutes**| `integer` | NULL, CHECK >= 0 | Thời gian đếm ngược (nếu có). |
| **ImageUrl** | `varchar(500)` | NULL | Ảnh minh họa cho bước này. |

## 7.4. RecipeIngredient
Bảng `RecipeIngredients` lưu thành phần nguyên liệu.

| Column | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | `uuid` | PK | Khóa chính UUID. |
| **RecipeId** | `uuid` | FK -> Recipes.Id (ON DELETE CASCADE) | Khóa ngoại công thức. |
| **Name** | `varchar(200)` | NOT NULL | Tên nguyên liệu. |
| **Quantity** | `decimal(10,3)` | NULL | Định lượng nguyên liệu. |
| **Unit** | `varchar(50)` | NULL | Đơn vị tính (gam, ml, thìa...). |
| **Notes** | `varchar(500)` | NULL | Ghi chú nguyên liệu. |
| **OrderIndex** | `integer` | NOT NULL, DEFAULT 0 | Thứ tự hiển thị nguyên liệu. |

## 7.5. RecipeImage
Bảng `RecipeImages` lưu bộ sưu tập ảnh của công thức.

| Column | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | `uuid` | PK | Khóa chính UUID. |
| **RecipeId** | `uuid` | FK -> Recipes.Id (ON DELETE CASCADE) | Khóa ngoại công thức. |
| **OriginalUrl** | `varchar(500)` | NOT NULL | Đường dẫn ảnh gốc trên MinIO. |
| **MediumUrl** | `varchar(500)` | NULL | Đường dẫn ảnh vừa (800x600). |
| **ThumbnailUrl**| `varchar(500)` | NULL | Đường dẫn ảnh nhỏ (300x300). |
| **AltText** | `varchar(200)` | NULL | Mô tả ảnh cho SEO & Accessibility. |
| **IsPrimary** | `boolean` | NOT NULL, DEFAULT false | Đánh dấu ảnh đại diện chính. |
| **OrderIndex** | `integer` | NOT NULL, DEFAULT 0 | Thứ tự hiển thị trong bộ sưu tập. |

## 7.6. Category
Bảng `Categories` lưu trữ danh mục.

| Column | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | `uuid` | PK | Khóa chính UUID. |
| **Name** | `varchar(100)` | NOT NULL, UNIQUE | Tên danh mục. |
| **Slug** | `varchar(120)` | NOT NULL, UNIQUE | URL slug của danh mục. |
| **Description**| `text` | NULL | Mô tả danh mục. |
| **ImageUrl** | `varchar(500)` | NULL | Ảnh đại diện danh mục. |
| **OrderIndex** | `integer` | NOT NULL, DEFAULT 0 | Thứ tự hiển thị trên Menu. |

## 7.7. ApplicationUser (extends IdentityUser)
Mở rộng bảng `AspNetUsers` của ASP.NET Core Identity.
- **DisplayName** (`varchar(100)`, NOT NULL)
- **AvatarUrl** (`varchar(500)`, NULL)
- **Bio** (`text`, NULL)
- **IsActive** (`boolean`, NOT NULL, DEFAULT `true`)
- **CreatedAt** (`timestamptz`, NOT NULL, DEFAULT `NOW()`)

## 7.8. Refresh Token
Bảng `RefreshTokens` lưu trữ phiên làm việc của người dùng.

| Column | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| **Id** | `uuid` | PK | Khóa chính UUID. |
| **UserId** | `varchar(450)` | FK -> AspNetUsers.Id (CASCADE) | Khóa ngoại người dùng. |
| **TokenHash** | `varchar(64)` | NOT NULL, UNIQUE, INDEX | Mã hash SHA-256 của Refresh Token. |
| **ExpiresAt** | `timestamptz` | NOT NULL | Ngày hết hạn token (7 ngày). |
| **RevokedAt** | `timestamptz` | NULL | Thời điểm thu hồi token. |
| **ReplacedByTokenHash** | `varchar(64)` | NULL | Hash của token thay thế khi rotate. |
| **CreatedAt** | `timestamptz` | NOT NULL, DEFAULT `NOW()` | Thời điểm tạo token. |
| **CreatedByIp** | `varchar(45)` | NULL | Đỉa chỉ IP khi gửi yêu cầu. |

---

# CHƯƠNG 8. ĐẶC TẢ REST API

## 8.1. Authentication Module (/auth)

| Method | Endpoint | Mô tả | Auth Required | Request Body / Query | Response Code & Payload |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **POST** | `/auth/register` | Đăng ký tài khoản mới | Không | `{ email, password, fullName, userName }` | `201 Created` (AuthResponseDto), `409 Conflict`, `422 Unprocessable` |
| **POST** | `/auth/login` | Đăng nhập Local | Không | `{ email, password }` | `200 OK` (AuthResponseDto), `401 Unauthorized`, `423 Locked` |
| **POST** | `/auth/google` | Đăng nhập Google OAuth | Không | `{ idToken }` | `200 OK` (AuthResponseDto), `401 Unauthorized` |
| **POST** | `/auth/refresh` | Làm mới Access Token | Không | `{ refreshToken }` | `200 OK` (AuthResponseDto), `401 Unauthorized` |
| **POST** | `/auth/logout` | Đăng xuất hệ thống | Bearer JWT | `{ refreshToken }` | `204 No Content`, `401 Unauthorized` |
| **GET** | `/auth/me` | Lấy hồ sơ người dùng | Bearer JWT | None | `200 OK` (UserProfileDto), `401 Unauthorized` |
| **PATCH**| `/auth/me` | Cập nhật hồ sơ | Bearer JWT | `{ fullName?, avatarUrl? }` | `200 OK` (UserProfileDto), `422 Unprocessable` |

## 8.2. Categories Module (/categories)

| Method | Endpoint | Mô tả | Auth / Role | Request Body / Query | Response Code & Payload |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **GET** | `/categories` | Danh sách danh mục | Không | None | `200 OK` (`CategoryDto[]`) |
| **GET** | `/categories/{slug}` | Chi tiết danh mục | Không | `?page=1&pageSize=12` | `200 OK` (`CategoryDetailDto`), `404 Not Found` |
| **POST** | `/categories` | Tạo danh mục | Bearer (Admin) | `{ name, description?, imageUrl? }` | `201 Created` (`CategoryDto`), `403 Forbidden`, `409 Conflict` |
| **PUT** | `/categories/{id}` | Cập nhật danh mục | Bearer (Admin) | `{ name, description?, imageUrl? }` | `200 OK` (`CategoryDto`), `404 Not Found`, `409 Conflict` |
| **DELETE**| `/categories/{id}` | Xóa danh mục | Bearer (Admin) | None | `204 No Content`, `409 Conflict (Has recipes)` |

## 8.3. Recipes Module (/recipes)

| Method | Endpoint | Mô tả | Auth / Role | Request Body / Query | Response Code & Payload |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **GET** | `/recipes` | Lấy danh sách công thức | Không | `?page=1&pageSize=12&categoryId=&difficulty=&sort=` | `200 OK` (`PagedResult<RecipeSummaryDto>`) |
| **GET** | `/recipes/{slug}` | Xem chi tiết công thức | Không | None | `200 OK` (`RecipeDetailDto`), `404 Not Found` |
| **GET** | `/recipes/search` | Tìm kiếm FTS tiếng Việt | Không | `?q=keyword&page=1&pageSize=12` | `200 OK` (`PagedResult<RecipeSummaryDto>`) |
| **POST** | `/recipes` | Tạo công thức mới (Draft) | Bearer (Author) | `{ title, description, categoryId, prepTime, cookTime, servings, difficulty }` | `201 Created` (`RecipeDto`), `422 Unprocessable` |
| **PUT** | `/recipes/{id}` | Cập nhật công thức | Bearer (Owner/Admin) | `{ title, description, categoryId, prepTime, cookTime, servings, difficulty }` | `200 OK` (`RecipeDto`), `409 Concurrency Conflict` |
| **PATCH**| `/recipes/{id}/publish` | Xuất bản công thức | Bearer (Owner/Admin) | None | `200 OK` (`RecipeDto`), `422 (Missing steps)` |
| **PATCH**| `/recipes/{id}/unpublish`| Hủy xuất bản công thức | Bearer (Owner/Admin) | None | `200 OK` (`RecipeDto`) |
| **PATCH**| `/recipes/{id}/archive` | Lưu trữ công thức | Bearer (Owner/Admin) | None | `200 OK` (`RecipeDto`) |
| **DELETE**| `/recipes/{id}` | Xóa vĩnh viễn công thức | Bearer (Owner/Admin) | None | `204 No Content`, `403 Forbidden`, `404 Not Found` |

## 8.4. Recipe Images (/recipes/{id}/images)

| Method | Endpoint | Mô tả | Auth / Role | Request Body | Response Code |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **POST** | `/recipes/{id}/images` | Upload ảnh công thức | Bearer (Owner/Admin) | Multipart: `file`, `altText?` | `201 Created` (`RecipeImageDto`), `400 Bad MIME` |
| **PATCH**| `/recipes/{id}/images/{imgId}/primary` | Đặt ảnh đại diện chính | Bearer (Owner/Admin) | None | `200 OK` |
| **DELETE**| `/recipes/{id}/images/{imgId}` | Xóa ảnh công thức | Bearer (Owner/Admin) | None | `204 No Content` |

## 8.5. Recipe Steps (/recipes/{id}/steps)

| Method | Endpoint | Mô tả | Auth / Role | Request Body | Response Code |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **POST** | `/recipes/{id}/steps` | Thêm bước thực hiện | Bearer (Owner/Admin) | `{ title, description, timerMinutes?, imageUrl? }` | `201 Created` (`RecipeStepDto`) |
| **PUT** | `/recipes/{id}/steps/{stepId}` | Cập nhật bước thực hiện | Bearer (Owner/Admin) | `{ title, description, timerMinutes?, imageUrl? }` | `200 OK` (`RecipeStepDto`) |
| **DELETE**| `/recipes/{id}/steps/{stepId}` | Xóa bước (tự renumber) | Bearer (Owner/Admin) | None | `204 No Content` |

## 8.6. Recipe Ingredients (/recipes/{id}/ingredients)

| Method | Endpoint | Mô tả | Auth / Role | Request Body | Response Code |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **POST** | `/recipes/{id}/ingredients` | Thêm nguyên liệu | Bearer (Owner/Admin) | `{ name, quantity?, unit?, notes? }` | `201 Created` (`RecipeIngredientDto`) |
| **PUT** | `/recipes/{id}/ingredients/{ingId}` | Cập nhật nguyên liệu | Bearer (Owner/Admin) | `{ name, quantity?, unit?, notes? }` | `200 OK` (`RecipeIngredientDto`) |
| **DELETE**| `/recipes/{id}/ingredients/{ingId}` | Xóa nguyên liệu | Bearer (Owner/Admin) | None | `204 No Content` |

## 8.7. Health Check Endpoints

| Method | Endpoint | Mô tả | Auth | Response Payload |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/health` | Kiểm tra tổng hợp toàn bộ dependencies | Không | `200 OK` (Healthy) / `503 Service Unavailable` |
| **GET** | `/health/live` | Liveness Probe (Kiểm tra Process sống) | Không | `200 OK` (Healthy) |
| **GET** | `/health/ready` | Readiness Probe (Kiểm tra DB & Redis) | Không | `200 OK` (Healthy) / `503 Service Unavailable` |

---

# PHỤ LỤC A – HTTP STATUS CODES

| Code | Status Name | Ngữ cảnh sử dụng trong Culinary Blog API |
| :--- | :--- | :--- |
| **200** | OK | Truy vấn thành công; Cập nhật tài nguyên thành công; Đăng nhập thành công. |
| **201** | Created | Tạo tài nguyên mới thành công (Recipe, Category, Step, Ingredient, Image). |
| **204** | No Content | Xóa thành công; Đăng xuất thành công. Không trả về response body. |
| **400** | Bad Request | Dữ liệu đầu vào không hợp lệ syntax, sai MIME type file upload, vi phạm rule nghiệp vụ. |
| **401** | Unauthorized | Access Token hết hạn/không hợp lệ; Chưa đăng nhập; Refresh Token hết hạn. |
| **403** | Forbidden | Đã đăng nhập nhưng không có quyền (ví dụ: Author cố sửa Recipe của người khác). |
| **404** | Not Found | Đường dẫn hoặc tài nguyên không tồn tại trong hệ thống. |
| **409** | Conflict | Xung đột dữ liệu (Email đã tồn tại, Slug bị trùng, xóa Category đang chứa Recipe). |
| **422** | Unprocessable Entity | Dữ liệu đúng cú pháp nhưng sai ngữ nghĩa; Concurrency Check (RowVersion mismatch) thất bại. |
| **429** | Too Many Requests | Vượt quá giới hạn Rate Limit tối đa số lượng request cho phép. |
| **500** | Internal Server Error | Lỗi hệ thống nội bộ không lường trước (Unhandled Exception). |
| **503** | Service Unavailable | Health check bị lỗi (Database/Redis down) hoặc Server đang bảo trì. |

---

# PHỤ LỤC B – APPLICATION ERROR CODES

| Mã lỗi (Error Code) | HTTP Status | Mô tả chi tiết | Module |
| :--- | :--- | :--- | :--- |
| `AUTH_EMAIL_EXISTS` | 409 | Địa chỉ Email đã được sử dụng cho tài khoản khác. | Auth |
| `AUTH_INVALID_CREDENTIALS` | 401 | Email hoặc mật khẩu không chính xác. | Auth |
| `AUTH_TOKEN_EXPIRED` | 401 | JWT Access Token đã hết hạn sử dụng. | Auth |
| `AUTH_TOKEN_INVALID` | 401 | Token không hợp lệ hoặc bị sai chữ ký HMAC. | Auth |
| `AUTH_REFRESH_TOKEN_EXPIRED` | 401 | Refresh Token đã hết hạn, yêu cầu đăng nhập lại. | Auth |
| `AUTH_REFRESH_TOKEN_REVOKED` | 401 | Refresh Token đã bị thu hồi (nghi vấn Reuse Attack). | Auth |
| `AUTH_GOOGLE_TOKEN_INVALID` | 400 | Google Identity Token không hợp lệ hoặc đã bị hết hạn. | Auth |
| `AUTH_ACCOUNT_DISABLED` | 403 | Tài khoản của người dùng đã bị khóa bởi Admin. | Auth |
| `RECIPE_NOT_FOUND` | 404 | Công thức nấu ăn không tồn tại hoặc đã bị xóa. | Recipe |
| `RECIPE_SLUG_EXISTS` | 409 | URL Slug bị trùng lập. | Recipe |
| `RECIPE_PUBLISH_INCOMPLETE` | 400 | Công thức thiếu các thông tin bắt buộc (ví dụ: Steps) để Publish. | Recipe |
| `RECIPE_FORBIDDEN` | 403 | Người dùng không phải tác giả sở hữu công thức này. | Recipe |
| `RECIPE_CONCURRENCY_CONFLICT` | 422 | Dữ liệu công thức đã bị thay đổi bởi người dùng khác. | Recipe |
| `CATEGORY_NOT_FOUND` | 404 | Danh mục công thức không tồn tại. | Category |
| `CATEGORY_NAME_EXISTS` | 409 | Tên danh mục đã tồn tại trong hệ thống. | Category |
| `CATEGORY_DELETE_HAS_RECIPES` | 409 | Không thể xóa danh mục hiện đang chứa công thức nấu ăn. | Category |
| `FILE_SIZE_EXCEEDED` | 400 | Dung lượng tệp tải lên vượt quá hạn mức 5MB. | File |
| `FILE_MIME_INVALID` | 400 | Định dạng tệp không được hỗ trợ (chỉ nhận JPG, PNG, WebP, AVIF). | File |
| `VALIDATION_ERROR` | 400 | Dữ liệu đầu vào vi phạm các quy tắc FluentValidation. | Common |
| `RATE_LIMIT_EXCEEDED` | 429 | Gửi quá nhiều yêu cầu trong thời gian ngắn. | Common |

---

# PHỤ LỤC C – TỪ ĐIỂN THUẬT NGỮ

- **Access Token (AT):** Mã định danh dạng JWT dùng để xác thực các request gửi đến API. Có thời gian sống ngắn (15 phút).
- **Application Error Code (AEC):** Mã lỗi tùy chỉnh hiển thị dạng `SCREAMING_SNAKE_CASE` theo chuẩn RFC 7807.
- **Archive:** Trạng thái lưu trữ công thức, ẩn khỏi danh sách công khai nhưng vẫn bảo lưu dữ liệu.
- **Author:** Vai trò người dùng có quyền sáng tạo, cập nhật, xuất bản bài viết công thức cá nhân.
- **Background Job:** Tác vụ xử lý ngầm bất đồng bộ không gây nghẽn luồng xử lý HTTP request chính.
- **Clean Architecture:** Kiến trúc phần mềm phân tầng đề cao tính độc lập của Business Logic với các Framework bên ngoài.
- **CQRS (Command Query Responsibility Segregation):** Mô hình phân tách lệnh ghi dữ liệu (Command) và lệnh đọc dữ liệu (Query).
- **Draft:** Trạng thái bản nháp của công thức nấu ăn, chỉ tác giả bài viết hoặc Admin mới có thể truy cập xem.
- **Full-Text Search (FTS):** Tìm kiếm toàn văn bản hỗ trợ xử lý ngôn ngữ tự nhiên, loại bỏ dấu tiếng Việt và sắp xếp theo độ liên quan.
- **Hangfire:** Thư viện quản lý tác vụ nền bất đồng bộ cho ứng dụng .NET hỗ trợ cơ chế lưu trữ bền vững.
- **Incremental Static Regeneration (ISR):** Cơ chế sinh trang tĩnh lại theo chu kỳ thời gian của Next.js mà không cần Rebuild lại toàn bộ hệ thống Web.
- **JSON Web Token (JWT):** Chuẩn mở (RFC 7519) an toàn dùng để truyền tải thông tin xác thực dạng JSON giữa các bên.
- **MinIO:** Hệ thống lưu trữ đối tượng (Object Storage) mã nguồn mở tương thích với chuẩn AWS S3 API.
- **Optimistic Concurrency Control:** Cơ chế kiểm soát truy cập đồng thời bằng `RowVersion` giúp phát hiện xung đột dữ liệu khi SaveChanges.
- **Published:** Trạng thái công thức đã được xuất bản công khai cho tất cả mọi người cùng xem.
- **Refresh Token (RT):** Mã thông báo dùng để đổi lấy Access Token mới khi Access Token cũ hết hạn mà không cần gõ lại Mật khẩu.
- **Refresh Token Rotation:** Cơ chế bảo mật vô hiệu hóa Refresh Token cũ và cấp mới một Refresh Token khác sau mỗi lần làm mới thành công.
- **Slug:** Chuỗi chữ viết thường gạch nối không dấu tối ưu dùng để hiển thị đường dẫn URL thân thiện với SEO.
- **Soft Delete:** Đánh dấu xóa giả định (`IsDeleted = true`) trong DB để ẩn tài nguyên mà không xóa hẳn bản ghi vật lý.
- **Unit of Work:** Mẫu thiết kế gom nhóm nhiều thao tác vào CSDL thực thi đồng thời trong cùng một Transaction.
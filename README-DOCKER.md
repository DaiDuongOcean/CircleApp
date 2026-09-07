# Hướng Dẫn Chạy CircleApp Với Docker

Dự án đã được cấu hình sẵn sàng để khởi chạy toàn diện bao gồm:

1. **Ứng dụng Web** (ASP.NET Core 8.0 MVC + SignalR).
2. **Cơ sở dữ liệu** (Microsoft SQL Server 2022 trong container).
3. **Lưu trữ bền vững (Volumes)**: Dữ liệu database và hình ảnh uploads (`wwwroot/images`) không bị mất khi khởi động lại container.
4. **Tự động khởi tạo**: Tự động chạy Migration và Seed tài khoản / dữ liệu mẫu khi khởi động lần đầu.

---

## 1. Yêu Cầu Tiên Quyết

- Đã cài đặt **Docker Desktop** (trên Windows).
- Đảm bảo Docker Desktop đang bật và hoạt động (biểu tượng cá voi màu xanh lá cây hoặc trạng thái 'Engine running').

---

## 2. Khởi Chạy Ứng Dụng

Mở Terminal (PowerShell hoặc Command Prompt) tại thư mục gốc của dự án (`d:\CircleApp`), chạy lệnh:

```bash
docker compose up -d --build
```

> **Ghi chú**: Lần đầu tiên chạy, Docker sẽ tải image SQL Server và build image ứng dụng .NET nên có thể mất vài phút tuỳ tốc độ mạng.

---

## 3. Truy Cập Ứng Dụng

- **Địa chỉ Web**: [http://localhost:5000](http://localhost:5000)
- **Tài khoản đăng nhập có sẵn** (được tự động seed vào database):
  - **Admin**: `admin@trupja.com` / Mật khẩu: `Coding@1234?`
  - **User**: `ervis@trupja.com` / Mật khẩu: `Coding@1234?`

---

## 4. Kết Nối Trực Tiếp Đến Database (Tuỳ chọn)

Nếu bạn muốn dùng SQL Server Management Studio (SSMS), Azure Data Studio hoặc DBeaver để quản lý database:

- **Server**: `localhost,1433`
- **Server**: `localhost,14333`
- **Authentication**: `SQL Server Authentication`
- **Login / Username**: `sa`
- **Password**: `YourStrong@Password123`
- **Trust Server Certificate**: `True` (Tích chọn Encrypt / Trust Server Certificate)
- **Database Name**: `CircleAppDb`

---

## 5. Các Lệnh Quản Lý Thường Dùng

### Xem log hoạt động theo thời gian thực

```bash
# Xem log cả web và database
docker compose logs -f

# Chỉ xem log của web app
docker compose logs -f web

# Chỉ xem log của database
docker compose logs -f db
```

### Dừng toàn bộ hệ thống

```bash
docker compose down
```

### Khởi động lại sau khi đã dừng

```bash
docker compose up -d
```

### Reset toàn bộ dữ liệu (xoá database & volumes để làm mới từ đầu)

```bash
docker compose down -v
docker compose up -d --build
```

### Đồng bộ (Export) data từ SQL Server local lên Docker container

Bạn có thể dùng script tự động chỉ bằng 1 lệnh PowerShell:

```powershell
.\export-db-to-docker.ps1
```

Script sẽ tự động backup DB `CircleAppDbCopy` từ máy local, chuyển vào container `circleapp-db`, restore và tự động dọn dẹp file tạm.

---

## 6. Cấu Hình Biến Môi Trường (.env)

Hệ thống sử dụng file `.env` tại thư mục gốc để quản lý các thông tin nhạy cảm:

- `MSSQL_SA_PASSWORD`: Mật khẩu tài khoản `sa` của SQL Server.
- `AUTH_GOOGLE_CLIENT_ID` & `AUTH_GOOGLE_CLIENT_SECRET`: Khóa xác thực Google OAuth.
- `AUTH_GITHUB_CLIENT_ID` & `AUTH_GITHUB_CLIENT_SECRET`: Khóa xác thực GitHub OAuth.

File mẫu [`.env.example`](file:///d:/CircleApp/.env.example) cũng đã được tạo để tham khảo. Khi thay đổi các giá trị trong `.env`, chỉ cần chạy:

```bash
docker compose up -d
```

---

## 7. Tuỳ Biến Cấu Hình (Nếu Muốn)

Trong file `docker-compose.yml`:

- **Đổi cổng truy cập Web**: Thay đổi `"5000:8080"` thành cổng khác (ví dụ `"8080:8080"` hoặc `"5122:8080"`).
- **Môi trường**: Có thể chuyển `ASPNETCORE_ENVIRONMENT=Production` khi triển khai thực tế.

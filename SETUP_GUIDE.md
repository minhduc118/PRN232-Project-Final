# 📖 Setup Guide — Sports Court Management System
# PRN232 | ASP.NET Core Web API + ReactJS

**Version:** 1.0 | **Date:** 14/05/2026

---

## Mục lục

1. [Yêu cầu môi trường](#1-yêu-cầu-môi-trường)
2. [Cấu trúc thư mục](#2-cấu-trúc-thư-mục)
3. [Khởi tạo Backend](#3-khởi-tạo-backend)
4. [Khởi tạo Frontend](#4-khởi-tạo-frontend)
5. [Cấu hình Database](#5-cấu-hình-database)
6. [Chạy Local Development](#6-chạy-local-development)
7. [CI/CD với GitHub Actions](#7-cicd-với-github-actions)
8. [Deployment](#8-deployment)
9. [Environment Variables](#9-environment-variables)
10. [Checklist hoàn thành](#10-checklist-hoàn-thành)

---

## 1. Yêu cầu môi trường

### Công cụ cần cài đặt

| Công cụ | Phiên bản | Link tải |
|---------|-----------|----------|
| .NET SDK | 8.0+ | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Node.js | 20.x LTS | https://nodejs.org |
| SQL Server | 2019+ | https://www.microsoft.com/sql-server |
| Git | Latest | https://git-scm.com |
| VS Code | Latest | https://code.visualstudio.com |

### Extensions VS Code nên cài

```
- C# Dev Kit (Microsoft)
- ES7+ React/Redux/React-Native snippets
- Prettier - Code formatter
- GitLens
- Thunder Client (test API)
- SQL Server (mssql)
```

### Kiểm tra môi trường

```bash
dotnet --version    # >= 8.0.0
node --version      # >= 20.0.0
npm --version       # >= 10.0.0
git --version       # >= 2.0.0
```

---

## 2. Cấu trúc thư mục

```
PRN232-Project-Final/
├── .github/
│   └── workflows/
│       ├── backend-ci.yml        # CI/CD Backend
│       └── frontend-ci.yml       # CI/CD Frontend
├── backend/
│   ├── SportsCourt.sln
│   └── src/
│       ├── SportsCourt.API/
│       │   ├── Controllers/
│       │   │   ├── AuthController.cs
│       │   │   ├── BookingsController.cs
│       │   │   ├── CourtsController.cs
│       │   │   ├── PaymentsController.cs
│       │   │   ├── ServicesController.cs
│       │   │   ├── UsersController.cs
│       │   │   ├── WaitlistController.cs
│       │   │   └── ReportsController.cs
│       │   ├── Middleware/
│       │   │   └── ExceptionMiddleware.cs
│       │   ├── Hubs/
│       │   │   └── CourtStatusHub.cs
│       │   ├── appsettings.json
│       │   ├── appsettings.Development.json
│       │   ├── Dockerfile
│       │   └── Program.cs
│       ├── SportsCourt.Application/
│       │   ├── DTOs/
│       │   ├── Services/
│       │   │   ├── Interfaces/
│       │   │   └── Implementations/
│       │   ├── Validators/
│       │   └── Mappings/
│       ├── SportsCourt.Domain/
│       │   ├── Entities/
│       │   ├── Enums/
│       │   └── Common/
│       └── SportsCourt.Infrastructure/
│           ├── Data/
│           │   ├── AppDbContext.cs
│           │   ├── Configurations/
│           │   └── Migrations/
│           └── Repositories/
├── frontend/
│   ├── src/
│   │   ├── api/
│   │   │   ├── axiosInstance.ts
│   │   │   ├── authApi.ts
│   │   │   ├── courtApi.ts
│   │   │   └── bookingApi.ts
│   │   ├── components/
│   │   │   ├── common/
│   │   │   └── layout/
│   │   ├── pages/
│   │   │   ├── auth/
│   │   │   ├── customer/
│   │   │   └── admin/
│   │   ├── store/
│   │   ├── hooks/
│   │   ├── types/
│   │   ├── routes/
│   │   └── App.tsx
│   ├── .env
│   ├── .env.production
│   ├── vite.config.ts
│   └── package.json
├── database/
│   ├── schema.sql
│   └── DATABASE_DESIGN.md
├── SRS.md
├── PROJECT_ROADMAP.md
├── TECHNICAL_SPEC.md
└── README.md
```

---

## 3. Khởi tạo Backend

### Bước 3.1 — Tạo Solution và Projects

```bash
# Tạo thư mục backend
mkdir backend && cd backend

# Tạo solution
dotnet new sln -n SportsCourt

# Tạo 4 projects (Clean Architecture)
dotnet new webapi  -n SportsCourt.API            -o src/SportsCourt.API
dotnet new classlib -n SportsCourt.Application   -o src/SportsCourt.Application
dotnet new classlib -n SportsCourt.Domain        -o src/SportsCourt.Domain
dotnet new classlib -n SportsCourt.Infrastructure -o src/SportsCourt.Infrastructure
dotnet new xunit   -n SportsCourt.Tests          -o tests/SportsCourt.Tests

# Thêm vào solution
dotnet sln add src/SportsCourt.API/SportsCourt.API.csproj
dotnet sln add src/SportsCourt.Application/SportsCourt.Application.csproj
dotnet sln add src/SportsCourt.Domain/SportsCourt.Domain.csproj
dotnet sln add src/SportsCourt.Infrastructure/SportsCourt.Infrastructure.csproj
dotnet sln add tests/SportsCourt.Tests/SportsCourt.Tests.csproj
```

### Bước 3.2 — Cấu hình Project References

```bash
# API phụ thuộc Application + Infrastructure
dotnet add src/SportsCourt.API reference src/SportsCourt.Application
dotnet add src/SportsCourt.API reference src/SportsCourt.Infrastructure

# Application phụ thuộc Domain
dotnet add src/SportsCourt.Application reference src/SportsCourt.Domain

# Infrastructure phụ thuộc Domain
dotnet add src/SportsCourt.Infrastructure reference src/SportsCourt.Domain

# Tests phụ thuộc Application
dotnet add tests/SportsCourt.Tests reference src/SportsCourt.Application
```

### Bước 3.3 — Cài đặt NuGet Packages

```bash
# === Infrastructure ===
dotnet add src/SportsCourt.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/SportsCourt.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add src/SportsCourt.Infrastructure package Microsoft.EntityFrameworkCore.Design

# === API ===
dotnet add src/SportsCourt.API package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/SportsCourt.API package Swashbuckle.AspNetCore
dotnet add src/SportsCourt.API package Microsoft.AspNetCore.SignalR
dotnet add src/SportsCourt.API package Serilog.AspNetCore
dotnet add src/SportsCourt.API package Serilog.Sinks.Console

# === Application ===
dotnet add src/SportsCourt.Application package AutoMapper
dotnet add src/SportsCourt.Application package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add src/SportsCourt.Application package FluentValidation
dotnet add src/SportsCourt.Application package FluentValidation.AspNetCore
dotnet add src/SportsCourt.Application package BCrypt.Net-Next

# === Tests ===
dotnet add tests/SportsCourt.Tests package Moq
dotnet add tests/SportsCourt.Tests package FluentAssertions
```

### Bước 3.4 — Cấu hình appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SportsCourtDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-at-least-32-characters!!",
    "Issuer": "SportsCourt.API",
    "Audience": "SportsCourt.Client",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ClientUrl": "http://localhost:5173"
}
```

### Bước 3.5 — Cấu hình Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON
builder.Services.AddControllers().AddJsonOptions(opt => {
    opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Auth
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => {
        opt.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
        };
    });

// CORS
builder.Services.AddCors(opt => opt.AddPolicy("AllowFrontend", policy =>
    policy.WithOrigins(builder.Configuration["ClientUrl"]!)
          .AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// AutoMapper + FluentValidation
builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<CourtStatusHub>("/hubs/court-status");

app.Run();
```

### Bước 3.6 — EF Core Migration

```bash
# Tạo migration đầu tiên
dotnet ef migrations add InitialCreate \
  --project src/SportsCourt.Infrastructure \
  --startup-project src/SportsCourt.API

# Áp dụng lên database
dotnet ef database update \
  --project src/SportsCourt.Infrastructure \
  --startup-project src/SportsCourt.API
```

---

## 4. Khởi tạo Frontend

### Bước 4.1 — Tạo Project

```bash
# Từ thư mục gốc PRN232-Project-Final
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install
```

### Bước 4.2 — Cài đặt Dependencies

```bash
# Routing
npm install react-router-dom

# HTTP Client
npm install axios

# State Management
npm install zustand

# Data Fetching + Caching
npm install @tanstack/react-query

# Form + Validation
npm install react-hook-form zod @hookform/resolvers

# UI & Notification
npm install react-hot-toast lucide-react

# Charts (Dashboard)
npm install recharts

# Date utilities
npm install date-fns

# SignalR
npm install @microsoft/signalr

# Dev dependencies
npm install -D @types/node
```

### Bước 4.3 — Cấu hình vite.config.ts

```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') }
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
})
```

### Bước 4.4 — File .env

```env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_SIGNALR_URL=http://localhost:5000/hubs
VITE_APP_NAME=Sports Court Management
```

### Bước 4.5 — Cấu hình Axios Instance

```typescript
// src/api/axiosInstance.ts
import axios from 'axios';

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
});

axiosInstance.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosInstance.interceptors.response.use(
  (res) => res,
  async (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('accessToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;
```

### Bước 4.6 — App Router

```typescript
// src/routes/AppRouter.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';

export const AppRouter = () => (
  <BrowserRouter>
    <Routes>
      {/* Public */}
      <Route path="/login"    element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/"         element={<HomePage />} />
      <Route path="/courts"   element={<CourtListPage />} />
      <Route path="/courts/:id" element={<CourtDetailPage />} />

      {/* Customer Protected */}
      <Route element={<ProtectedRoute allowedRoles={['Customer','Admin','Staff']} />}>
        <Route path="/booking/:courtId" element={<BookingPage />} />
        <Route path="/my-bookings"      element={<MyBookingsPage />} />
        <Route path="/profile"          element={<ProfilePage />} />
      </Route>

      {/* Admin Protected */}
      <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
        <Route path="/admin"                element={<DashboardPage />} />
        <Route path="/admin/courts"         element={<ManageCourtsPage />} />
        <Route path="/admin/bookings"       element={<ManageBookingsPage />} />
        <Route path="/admin/users"          element={<ManageUsersPage />} />
        <Route path="/admin/reports"        element={<ReportsPage />} />
        <Route path="/admin/maintenance"    element={<MaintenancePage />} />
        <Route path="/admin/staff-shifts"   element={<StaffShiftsPage />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  </BrowserRouter>
);
```

---

## 5. Cấu hình Database cho Team 5 người

### Tổng quan: 2 giai đoạn quản lý database

```
Lần đầu setup:   schema.sql  → tạo DB từ đầu (26 bảng + seed data)
Cập nhật về sau: EF Migration → thêm/sửa bảng mà không mất data
```

---

### 5.1 Ba lựa chọn chạy SQL Server

#### Option A — SQL Server cài sẵn trên máy
> Phù hợp nếu máy đã có SQL Server Management Studio

```bash
# Mở SSMS → kết nối localhost → chạy file:
database/schema.sql
```

#### Option B — Docker SQL Server *(Khuyến nghị)*
> Không cần cài SQL Server, chỉ cần Docker Desktop

**Tạo `docker-compose.yml` ở thư mục gốc:**

```yaml
version: '3.8'
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sportscourtdb
    environment:
      SA_PASSWORD: "Dev@123456"
      ACCEPT_EULA: "Y"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
      - ./database:/database        # Mount thư mục database vào container
    restart: unless-stopped

volumes:
  sqldata:
```

**Khởi động và tạo DB:**

```bash
# Bước 1: Chạy container
docker compose up -d

# Bước 2: Chờ SQL Server sẵn sàng (~15 giây)
docker logs sportscourtdb --follow

# Bước 3: Chạy schema.sql vào container
docker exec -i sportscourtdb /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "Dev@123456" \
  -C -i /database/schema.sql

# ✅ Kết quả: DB có đủ 26 bảng + seed data
```

**Connection String khi dùng Docker:**
```
Server=localhost,1433;Database=SportsCourtDB;User Id=sa;Password=Dev@123456;TrustServerCertificate=True
```

#### Option C — Railway Cloud Database
> Không cần cài bất cứ gì, 5 người kết nối cùng 1 DB cloud

```bash
# 1. Đăng ký railway.app → New Project → Database → SQL Server
# 2. Copy Connection String từ Railway dashboard
# 3. Chia sẻ connection string cho cả team (qua nhóm chat bảo mật)
# 4. Chạy schema.sql qua Railway Query console
```

---

### 5.2 Phân biệt schema.sql vs EF Migration

| | `schema.sql` | EF Migration |
|---|---|---|
| **Dùng khi nào** | Khởi tạo DB lần đầu | Cập nhật DB về sau |
| **Tác động** | Xóa và tạo lại toàn bộ | Chỉ thêm/sửa phần mới |
| **Mất data** | ✅ Có (DROP DATABASE) | ❌ Không |
| **Commit lên Git** | 1 lần duy nhất | Mỗi lần thay đổi schema |

> [!WARNING]
> `schema.sql` có lệnh `DROP DATABASE` ở đầu — **chỉ chạy lần đầu** khi setup. Không chạy lại khi đã có data!

---

### 5.3 EF Migration — Quy trình cho team

#### Nguyên tắc bắt buộc

> [!CAUTION]
> **Không bao giờ** 2 người tạo migration cùng lúc → conflict rất khó resolve!

```
TRƯỚC khi tạo migration:
  1. Thông báo team: "Mình đang tạo migration X nhé!"
  2. Chờ xác nhận không ai đang làm
  3. git pull mới nhất
  4. Tạo migration → commit ngay → push
  5. Thông báo team: "Xong! Pull và update db nhé"
```

#### Lệnh tạo migration

```bash
# Khi thay đổi Entity C# → tạo migration
dotnet ef migrations add TenMigration \
  --project src/SportsCourt.Infrastructure \
  --startup-project src/SportsCourt.API

# Áp dụng lên DB local
dotnet ef database update \
  --project src/SportsCourt.Infrastructure \
  --startup-project src/SportsCourt.API

# Commit migration files ngay lập tức
git add .
git commit -m "db: add TenMigration - mô tả thay đổi"
git push
```

#### Khi pull code về (có migration mới)

```bash
git pull origin develop

# LUÔN LUÔN chạy sau khi pull
dotnet ef database update \
  --project src/SportsCourt.Infrastructure \
  --startup-project src/SportsCourt.API
```

---

### 5.4 Quy trình làm việc hàng ngày cho team

```bash
# === BẮT ĐẦU NGÀY LÀM VIỆC ===
git pull origin develop
dotnet ef database update    # Sync migration mới nhất
cd frontend && npm install   # Sync packages mới (nếu có)

# === KHI CẦN THAY ĐỔI DATABASE ===
# Thông báo trên nhóm chat trước!
git pull origin develop
# ... sửa Entity C# ...
dotnet ef migrations add ThemCotMoi \
  --project src/SportsCourt.Infrastructure \
  --startup-project src/SportsCourt.API
dotnet ef database update \
  --project src/SportsCourt.Infrastructure \
  --startup-project src/SportsCourt.API
git add . && git commit -m "db: add ThemCotMoi" && git push
# Thông báo xong để team pull về

# === KẾT THÚC NGÀY ===
git add . && git commit -m "feat: ..." && git push
```

---

### 5.5 Branching strategy cho 5 người

```
main      ──────●──────────────●──── (Production — deploy)
                │              │
develop   ──●───●──●──●──●─────●──── (Integration — merge vào đây)
                   │  │  │
feature/auth  ─────●──┘  │          (Dev 1 — Backend Auth)
feature/court ────────●──┘          (Dev 2 — Backend Court)
feature/booking ───────────●──      (Dev 3 — Backend Booking)
feature/payment ──────────────●─    (Dev 4 — Payment)
feature/frontend ─────────────────● (Dev 5 — ReactJS)
```

**Naming convention:**
```bash
feature/auth-jwt              # Dev 1
feature/court-crud            # Dev 2
feature/booking-workflow      # Dev 3
feature/payment-vnpay         # Dev 4
feature/frontend-ui           # Dev 5
hotfix/fix-booking-cancel     # Urgent fix
```

---

### 5.6 File .gitignore — Bảo vệ thông tin nhạy cảm

Thêm vào `.gitignore`:
```gitignore
# Config local — mỗi dev tự tạo, KHÔNG commit
backend/src/SportsCourt.API/appsettings.Development.json

# Build
**/bin/
**/obj/
**/node_modules/

# Env files
frontend/.env
frontend/.env.local
```

**Commit file mẫu thay thế:**
```bash
# appsettings.Development.json.example → COMMIT file này
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SportsCourtDB;User Id=sa;Password=Dev@123456;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "REPLACE_WITH_YOUR_SECRET_KEY_MIN_32_CHARS"
  }
}
```

---

### 5.7 Setup nhanh cho thành viên mới (clone về lần đầu)

```bash
# 1. Clone repo
git clone https://github.com/minhduc118/PRN232-Project-Final.git
cd PRN232-Project-Final

# 2. Chạy SQL Server (chọn 1 trong 2)
docker compose up -d                          # Option B: Docker
# hoặc mở SSMS kết nối localhost             # Option A: SQL Server sẵn

# 3. Tạo file config local
cp backend/src/SportsCourt.API/appsettings.Development.json.example \
   backend/src/SportsCourt.API/appsettings.Development.json
# Sửa connection string nếu cần

# 4. Tạo DB từ schema.sql (CHỈ LẦN ĐẦU)
docker exec -i sportscourtdb /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "Dev@123456" -C \
  -i /database/schema.sql

# 5. Restore & chạy backend
cd backend
dotnet restore
dotnet run --project src/SportsCourt.API

# 6. Chạy frontend
cd ../frontend
npm install
npm run dev

# ✅ Backend: http://localhost:5000/swagger
# ✅ Frontend: http://localhost:5173
```

---

### Connection String theo môi trường

```
# Local Docker
Server=localhost,1433;Database=SportsCourtDB;User Id=sa;Password=Dev@123456;TrustServerCertificate=True

# Local SQL Server (Windows Auth)
Server=localhost;Database=SportsCourtDB;Trusted_Connection=True;TrustServerCertificate=True

# Railway (Production)
Server=xxx.railway.app,PORT;Database=SportsCourtDB;User Id=sa;Password=xxx;TrustServerCertificate=True
```

---

## 6. Chạy Local Development

### Chạy Backend

```bash
cd backend
dotnet run --project src/SportsCourt.API

# API chạy tại: http://localhost:5000
# Swagger UI:   http://localhost:5000/swagger
```

### Chạy Frontend

```bash
cd frontend
npm run dev

# Frontend chạy tại: http://localhost:5173
```

### Chạy cả hai cùng lúc (terminal riêng)

```bash
# Terminal 1 - Backend
cd backend && dotnet watch run --project src/SportsCourt.API

# Terminal 2 - Frontend
cd frontend && npm run dev
```

---

## 7. CI/CD với GitHub Actions

### Backend CI — `.github/workflows/backend-ci.yml`

```yaml
name: Backend CI

on:
  push:
    branches: [main, develop]
    paths: ['backend/**']
  pull_request:
    branches: [main]

jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore backend/SportsCourt.sln

      - name: Build
        run: dotnet build backend/SportsCourt.sln --no-restore -c Release

      - name: Test
        run: dotnet test backend/SportsCourt.sln --no-build -v normal

  deploy:
    needs: build-test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Deploy to Render
        run: curl -X POST ${{ secrets.RENDER_DEPLOY_HOOK_URL }}
```

### Frontend CI — `.github/workflows/frontend-ci.yml`

```yaml
name: Frontend CI

on:
  push:
    branches: [main]
    paths: ['frontend/**']

jobs:
  build-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-node@v4
        with:
          node-version: '20.x'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install
        working-directory: ./frontend
        run: npm ci

      - name: Lint
        working-directory: ./frontend
        run: npm run lint

      - name: Build
        working-directory: ./frontend
        run: npm run build
        env:
          VITE_API_BASE_URL: ${{ secrets.VITE_API_BASE_URL }}

      - name: Deploy to Vercel
        uses: amondnet/vercel-action@v25
        with:
          vercel-token: ${{ secrets.VERCEL_TOKEN }}
          vercel-org-id: ${{ secrets.VERCEL_ORG_ID }}
          vercel-project-id: ${{ secrets.VERCEL_PROJECT_ID }}
          working-directory: ./frontend
          vercel-args: '--prod'
```

---

## 8. Deployment

### Stack đề xuất (Miễn phí)

```
Frontend  → Vercel
Backend   → Render
Database  → Railway
File/Ảnh → Cloudinary
```

### 8.1 Deploy Frontend lên Vercel

```bash
# Cài Vercel CLI
npm install -g vercel

# Login
vercel login

# Deploy từ thư mục frontend
cd frontend
vercel --prod
```

**Cấu hình Vercel:**
- Framework Preset: `Vite`
- Root Directory: `frontend`
- Build Command: `npm run build`
- Output Directory: `dist`

**Environment Variables trên Vercel:**
```
VITE_API_BASE_URL = https://your-backend.onrender.com/api
VITE_SIGNALR_URL  = https://your-backend.onrender.com/hubs
```

---

### 8.2 Deploy Backend lên Render

**Tạo `backend/src/SportsCourt.API/Dockerfile`:**

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/SportsCourt.API/SportsCourt.API.csproj", "src/SportsCourt.API/"]
COPY ["src/SportsCourt.Application/SportsCourt.Application.csproj", "src/SportsCourt.Application/"]
COPY ["src/SportsCourt.Domain/SportsCourt.Domain.csproj", "src/SportsCourt.Domain/"]
COPY ["src/SportsCourt.Infrastructure/SportsCourt.Infrastructure.csproj", "src/SportsCourt.Infrastructure/"]

RUN dotnet restore "src/SportsCourt.API/SportsCourt.API.csproj"
COPY . .
RUN dotnet publish "src/SportsCourt.API/SportsCourt.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SportsCourt.API.dll"]
```

**Các bước deploy Render:**
1. Vào https://render.com → New → Web Service
2. Connect GitHub repo
3. Root Directory: `backend`
4. Build Command: *(tự động dùng Dockerfile)*
5. Thêm Environment Variables:

```
ASPNETCORE_ENVIRONMENT     = Production
ConnectionStrings__DefaultConnection = Server=xxx.railway.app,...
JwtSettings__SecretKey     = your-production-secret-key
ClientUrl                  = https://your-frontend.vercel.app
```

---

### 8.3 Deploy Database lên Railway

1. Vào https://railway.app → New Project → Add Database → Microsoft SQL Server
2. Copy **Connection String** từ Railway
3. Dán vào Render Environment Variables
4. Chạy `schema.sql` qua Railway console hoặc Azure Data Studio

---

### 8.4 GitHub Secrets cần cấu hình

| Secret | Mô tả |
|--------|-------|
| `RENDER_DEPLOY_HOOK_URL` | Webhook từ Render dashboard |
| `VERCEL_TOKEN` | Token từ Vercel settings |
| `VERCEL_ORG_ID` | Org ID từ Vercel |
| `VERCEL_PROJECT_ID` | Project ID từ Vercel |
| `VITE_API_BASE_URL` | URL backend production |

---

## 9. Environment Variables

### Backend — `appsettings.Production.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=railway-host;Database=SportsCourtDB;User Id=sa;Password=xxx;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "production-secret-key-min-32-chars!!!",
    "Issuer": "SportsCourt.API",
    "Audience": "SportsCourt.Client",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "ClientUrl": "https://sports-court.vercel.app"
}
```

### Frontend — `.env.production`

```env
VITE_API_BASE_URL=https://sports-court-api.onrender.com/api
VITE_SIGNALR_URL=https://sports-court-api.onrender.com/hubs
VITE_APP_NAME=Sports Court Management
```

---

## 10. Checklist hoàn thành

### ✅ Backend Setup
- [ ] .NET 8 SDK đã cài
- [ ] Solution và 4 projects đã tạo
- [ ] Project references đúng thứ tự
- [ ] NuGet packages đã cài đầy đủ
- [ ] `appsettings.json` đã cấu hình
- [ ] `Program.cs` đã setup JWT, CORS, Swagger, SignalR
- [ ] EF Core migration chạy thành công
- [ ] `dotnet run` — API chạy tại port 5000
- [ ] Swagger UI hiển thị đầy đủ endpoints

### ✅ Frontend Setup
- [ ] Node.js 20+ đã cài
- [ ] Vite + React + TypeScript đã khởi tạo
- [ ] Tất cả npm packages đã cài
- [ ] `vite.config.ts` đã cấu hình proxy
- [ ] `.env` đã tạo
- [ ] `axiosInstance.ts` với interceptors
- [ ] Router với Protected Routes
- [ ] `npm run dev` — Frontend chạy tại port 5173

### ✅ Integration
- [ ] Frontend gọi được API backend
- [ ] Login trả về JWT token
- [ ] Token lưu vào localStorage
- [ ] Protected routes hoạt động
- [ ] SignalR kết nối thành công

### ✅ Deployment
- [ ] Dockerfile hoạt động (`docker build` thành công)
- [ ] GitHub Actions CI pass
- [ ] Backend deploy lên Render
- [ ] Frontend deploy lên Vercel
- [ ] Database trên Railway
- [ ] Public URL hoạt động end-to-end
- [ ] GitHub Secrets đã cấu hình đầy đủ

---

## Tài liệu tham khảo

| Tài liệu | Link |
|----------|------|
| SRS | [SRS.md](./SRS.md) |
| Database Design | [database/DATABASE_DESIGN.md](./database/DATABASE_DESIGN.md) |
| Technical Spec | [TECHNICAL_SPEC.md](./TECHNICAL_SPEC.md) |
| Project Roadmap | [PROJECT_ROADMAP.md](./PROJECT_ROADMAP.md) |
| ASP.NET Core Docs | https://docs.microsoft.com/aspnet/core |
| React Docs | https://react.dev |
| EF Core Docs | https://docs.microsoft.com/ef/core |

---

*Setup Guide — PRN232 Sports Court Management System — v1.0*

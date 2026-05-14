# 📐 Technical Specification Document
# Sports Court Management System — PRN232

**Version:** 1.0 | **Date:** 14/05/2026

---

## 1. Tổng quan kiến trúc

```
┌─────────────────────────────────────────────┐
│              CLIENT LAYER                   │
│         ReactJS (Vite + TypeScript)         │
└──────────────────┬──────────────────────────┘
                   │ HTTPS / REST API
┌──────────────────▼──────────────────────────┐
│              API GATEWAY LAYER              │
│        ASP.NET Core Web API (.NET 8)        │
│   JWT Auth │ Middleware │ Rate Limiting     │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│            APPLICATION LAYER                │
│    Services │ Business Logic │ Validators  │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│           INFRASTRUCTURE LAYER              │
│  EF Core │ SQL Server │ Redis │ SignalR     │
└─────────────────────────────────────────────┘
```

**Pattern áp dụng:** Clean Architecture + Repository/Service Pattern

---

## 2. Backend — ASP.NET Core Web API

### 2.1 Cấu trúc thư mục

```
SportsCourt.sln
├── src/
│   ├── SportsCourt.API/                  # Presentation Layer
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── CourtsController.cs
│   │   │   ├── BookingsController.cs
│   │   │   ├── PaymentsController.cs
│   │   │   ├── ServicesController.cs
│   │   │   ├── UsersController.cs
│   │   │   └── ReportsController.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionMiddleware.cs
│   │   │   └── RequestLoggingMiddleware.cs
│   │   ├── Hubs/
│   │   │   └── CourtStatusHub.cs        # SignalR
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── ApplicationBuilderExtensions.cs
│   │   └── Program.cs
│   │
│   ├── SportsCourt.Application/          # Business Layer
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   ├── Court/
│   │   │   ├── Booking/
│   │   │   └── Payment/
│   │   ├── Services/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IAuthService.cs
│   │   │   │   ├── ICourtService.cs
│   │   │   │   ├── IBookingService.cs
│   │   │   │   └── IPaymentService.cs
│   │   │   └── Implementations/
│   │   │       ├── AuthService.cs
│   │   │       ├── CourtService.cs
│   │   │       ├── BookingService.cs
│   │   │       └── PaymentService.cs
│   │   ├── Validators/                   # FluentValidation
│   │   └── Mappings/                     # AutoMapper Profiles
│   │
│   ├── SportsCourt.Domain/               # Core Entities
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Court.cs
│   │   │   ├── Booking.cs
│   │   │   ├── Payment.cs
│   │   │   └── ...
│   │   ├── Enums/
│   │   │   ├── BookingStatus.cs
│   │   │   └── CourtStatus.cs
│   │   └── Common/
│   │       └── BaseEntity.cs
│   │
│   └── SportsCourt.Infrastructure/       # Data Layer
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   ├── Configurations/           # EF Fluent API
│       │   └── Migrations/
│       ├── Repositories/
│       │   ├── Interfaces/
│       │   │   └── IGenericRepository.cs
│       │   └── Implementations/
│       │       └── GenericRepository.cs
│       └── External/
│           ├── VNPayService.cs
│           └── EmailService.cs
│
└── tests/
    ├── SportsCourt.UnitTests/
    └── SportsCourt.IntegrationTests/
```

---

### 2.2 Entity Framework Core — Migration Workflow

> **Lưu ý:** "EF Migrations" (không phải "gif") là công cụ đúng để quản lý database schema.

**Cài đặt packages:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
```

**Cấu hình DbContext:**
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Court> Courts { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
```

**Migration commands:**
```bash
# Tạo migration mới
dotnet ef migrations add InitialCreate --project SportsCourt.Infrastructure --startup-project SportsCourt.API

# Áp dụng lên database
dotnet ef database update --project SportsCourt.Infrastructure --startup-project SportsCourt.API

# Rollback migration
dotnet ef database update PreviousMigrationName

# Seed data
dotnet ef migrations add SeedData
```

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SportsCourtDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

---

### 2.3 JWT Authentication

**Cài đặt:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

**Cấu hình appsettings.json:**
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-min-32-chars!!",
    "Issuer": "SportsCourt.API",
    "Audience": "SportsCourt.Client",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Program.cs:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!))
        };
    });
```

---

### 2.4 API Endpoints

#### Auth
| Method | Endpoint | Mô tả | Auth |
|--------|----------|-------|------|
| POST | `/api/auth/register` | Đăng ký | ❌ |
| POST | `/api/auth/login` | Đăng nhập | ❌ |
| POST | `/api/auth/refresh-token` | Làm mới token | ❌ |
| POST | `/api/auth/logout` | Đăng xuất | ✅ |

#### Courts
| Method | Endpoint | Mô tả | Role |
|--------|----------|-------|------|
| GET | `/api/courts` | Danh sách sân + filter | Public |
| GET | `/api/courts/{id}` | Chi tiết sân | Public |
| POST | `/api/courts` | Tạo sân | Admin |
| PUT | `/api/courts/{id}` | Cập nhật sân | Admin |
| DELETE | `/api/courts/{id}` | Xóa sân | Admin |
| GET | `/api/courts/{id}/availability` | Kiểm tra lịch trống | Public |

#### Bookings
| Method | Endpoint | Mô tả | Role |
|--------|----------|-------|------|
| GET | `/api/bookings` | Danh sách booking | Admin/Staff |
| GET | `/api/bookings/my` | Booking của tôi | Customer |
| GET | `/api/bookings/{id}` | Chi tiết booking | Auth |
| POST | `/api/bookings` | Tạo booking | Customer |
| PUT | `/api/bookings/{id}/cancel` | Hủy booking | Auth |
| PUT | `/api/bookings/{id}/status` | Cập nhật trạng thái | Admin/Staff |

#### Payments
| Method | Endpoint | Mô tả | Role |
|--------|----------|-------|------|
| POST | `/api/payments/vnpay/create` | Tạo link thanh toán VNPay | Customer |
| GET | `/api/payments/vnpay/callback` | Callback từ VNPay | Public |
| GET | `/api/payments/{bookingId}` | Chi tiết thanh toán | Auth |

---

### 2.5 Response Format chuẩn

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; }
}
```

**Ví dụ response thành công:**
```json
{
  "success": true,
  "message": "Booking created successfully",
  "data": {
    "bookingId": 1,
    "courtName": "Sân Cầu Lông A1",
    "bookingDate": "2026-05-20",
    "totalAmount": 150000,
    "status": "Pending"
  },
  "statusCode": 201
}
```

---

### 2.6 Exception Middleware

```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status404NotFound);
        }
        catch (UnauthorizedException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status401Unauthorized);
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError);
        }
    }
}
```

---

### 2.7 Pagination & Filter

**Request model:**
```csharp
public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

public class CourtFilterRequest : PaginationRequest
{
    public int? CourtTypeId { get; set; }
    public string? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
```

**Response model:**
```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
```

---

### 2.8 SignalR — Realtime Court Status

```csharp
public class CourtStatusHub : Hub
{
    public async Task JoinCourtGroup(string courtId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"court-{courtId}");
    }

    public async Task LeaveCourtGroup(string courtId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"court-{courtId}");
    }
}

// Gọi từ BookingService khi tạo booking
await _hubContext.Clients.Group($"court-{courtId}")
    .SendAsync("CourtStatusChanged", new { courtId, status = "Booked" });
```

---

## 3. Frontend — ReactJS

### 3.1 Cấu trúc thư mục

```
frontend/
├── public/
├── src/
│   ├── api/                    # API calls
│   │   ├── axiosInstance.ts
│   │   ├── authApi.ts
│   │   ├── courtApi.ts
│   │   └── bookingApi.ts
│   ├── components/             # Reusable components
│   │   ├── common/
│   │   │   ├── Button/
│   │   │   ├── Input/
│   │   │   ├── Modal/
│   │   │   ├── Pagination/
│   │   │   └── Toast/
│   │   └── layout/
│   │       ├── Navbar.tsx
│   │       ├── Sidebar.tsx
│   │       └── Footer.tsx
│   ├── pages/
│   │   ├── auth/
│   │   │   ├── LoginPage.tsx
│   │   │   └── RegisterPage.tsx
│   │   ├── customer/
│   │   │   ├── HomePage.tsx
│   │   │   ├── CourtListPage.tsx
│   │   │   ├── CourtDetailPage.tsx
│   │   │   ├── BookingPage.tsx
│   │   │   └── MyBookingsPage.tsx
│   │   └── admin/
│   │       ├── DashboardPage.tsx
│   │       ├── ManageCourtsPage.tsx
│   │       ├── ManageBookingsPage.tsx
│   │       └── ReportsPage.tsx
│   ├── store/                  # Zustand state management
│   │   ├── authStore.ts
│   │   ├── courtStore.ts
│   │   └── bookingStore.ts
│   ├── hooks/                  # Custom hooks
│   │   ├── useAuth.ts
│   │   ├── useCourts.ts
│   │   └── useBooking.ts
│   ├── types/                  # TypeScript interfaces
│   │   ├── auth.types.ts
│   │   ├── court.types.ts
│   │   └── booking.types.ts
│   ├── utils/
│   │   ├── formatters.ts
│   │   └── validators.ts
│   ├── routes/
│   │   ├── AppRouter.tsx
│   │   └── ProtectedRoute.tsx
│   └── App.tsx
├── .env
├── .env.production
├── vite.config.ts
└── package.json
```

### 3.2 Tech Stack Frontend

| Package | Phiên bản | Mục đích |
|---------|-----------|----------|
| React | 18.x | UI Framework |
| TypeScript | 5.x | Type safety |
| Vite | 5.x | Build tool |
| React Router DOM | 6.x | Routing |
| Axios | 1.x | HTTP client |
| Zustand | 4.x | State management |
| React Hook Form | 7.x | Form handling |
| Zod | 3.x | Schema validation |
| TanStack Query | 5.x | Data fetching & cache |
| Recharts | 2.x | Dashboard charts |
| Shadcn/UI | latest | UI components |
| React Hot Toast | 2.x | Toast notifications |
| date-fns | 3.x | Date formatting |
| @microsoft/signalr | 8.x | Realtime connection |

### 3.3 Axios Instance & Token Handling

```typescript
// src/api/axiosInstance.ts
import axios from 'axios';
import { useAuthStore } from '../store/authStore';

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor — attach token
axiosInstance.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor — handle 401
axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        await useAuthStore.getState().refreshToken();
        return axiosInstance(originalRequest);
      } catch {
        useAuthStore.getState().logout();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;
```

### 3.4 Protected Routes

```typescript
// src/routes/ProtectedRoute.tsx
import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

interface ProtectedRouteProps {
  allowedRoles?: string[];
}

export const ProtectedRoute = ({ allowedRoles }: ProtectedRouteProps) => {
  const { isAuthenticated, user } = useAuthStore();

  if (!isAuthenticated) return <Navigate to="/login" replace />;

  if (allowedRoles && !allowedRoles.includes(user?.role ?? '')) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <Outlet />;
};
```

### 3.5 Environment Variables

```env
# .env
VITE_API_BASE_URL=http://localhost:5000/api
VITE_SIGNALR_URL=http://localhost:5000/hubs
VITE_VNPAY_RETURN_URL=http://localhost:3000/payment/result
```

---

## 4. Database — EF Core Migration Strategy

> ⚠️ **Làm rõ:** Bạn hỏi về "gif" — đây có thể là **EF Core Migrations**, đây là công cụ chính xác để quản lý database schema trong .NET. GIF (ảnh động) không liên quan đến database.

### 4.1 Migration Workflow

```
Code First Approach
┌──────────┐    dotnet ef migrations add    ┌──────────────┐
│ Entities │ ──────────────────────────────► │  Migration   │
│  (C#)    │                                │   Files      │
└──────────┘                                └──────┬───────┘
                                                   │ dotnet ef database update
                                            ┌──────▼───────┐
                                            │  SQL Server  │
                                            │  Database    │
                                            └──────────────┘
```

### 4.2 Seed Data

```csharp
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Staff" },
                new Role { Name = "Coach" },
                new Role { Name = "Customer" }
            );
        }

        if (!context.CourtTypes.Any())
        {
            context.CourtTypes.AddRange(
                new CourtType { Name = "Cầu lông", Description = "Sân cầu lông tiêu chuẩn" },
                new CourtType { Name = "Bóng đá", Description = "Sân bóng đá mini" },
                new CourtType { Name = "Pickleball", Description = "Sân pickleball" }
            );
        }

        await context.SaveChangesAsync();
    }
}
```

---

## 5. CI/CD Pipeline — GitHub Actions

### 5.1 Kiến trúc deployment đề xuất

```
Developer → GitHub → GitHub Actions → Docker Hub → Cloud Platform
                          │
                    ┌─────┴──────┐
                    │            │
                Backend      Frontend
                 (Render)    (Vercel)
                    │
                SQL Server
               (Railway / Azure)
```

### 5.2 Deployment Stack đề xuất

| Thành phần | Nền tảng | Ghi chú |
|------------|----------|---------|
| Backend API | **Render** | Free tier, Docker support |
| Frontend | **Vercel** | Auto-deploy từ GitHub |
| Database | **Railway** hoặc **Azure SQL** | Managed SQL Server |
| File Storage | **Cloudinary** | Upload ảnh sân |
| Email | **SendGrid** | Free 100 emails/ngày |

### 5.3 GitHub Actions — Backend CI/CD

```yaml
# .github/workflows/backend-ci-cd.yml
name: Backend CI/CD

on:
  push:
    branches: [main, develop]
    paths: ['backend/**']
  pull_request:
    branches: [main]
    paths: ['backend/**']

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore ./backend/SportsCourt.sln

      - name: Build
        run: dotnet build ./backend/SportsCourt.sln --no-restore --configuration Release

      - name: Run tests
        run: dotnet test ./backend/SportsCourt.sln --no-build --verbosity normal

  deploy:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Deploy to Render
        run: |
          curl -X POST ${{ secrets.RENDER_DEPLOY_HOOK_URL }}
```

### 5.4 GitHub Actions — Frontend CI/CD

```yaml
# .github/workflows/frontend-ci-cd.yml
name: Frontend CI/CD

on:
  push:
    branches: [main]
    paths: ['frontend/**']

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20.x'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Install dependencies
        working-directory: ./frontend
        run: npm ci

      - name: Run lint
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

### 5.5 Dockerfile — Backend

```dockerfile
# backend/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SportsCourt.API/SportsCourt.API.csproj", "SportsCourt.API/"]
COPY ["SportsCourt.Application/SportsCourt.Application.csproj", "SportsCourt.Application/"]
COPY ["SportsCourt.Domain/SportsCourt.Domain.csproj", "SportsCourt.Domain/"]
COPY ["SportsCourt.Infrastructure/SportsCourt.Infrastructure.csproj", "SportsCourt.Infrastructure/"]

RUN dotnet restore "SportsCourt.API/SportsCourt.API.csproj"
COPY . .
WORKDIR "/src/SportsCourt.API"
RUN dotnet build "SportsCourt.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportsCourt.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SportsCourt.API.dll"]
```

---

## 6. Git Branching Strategy

```
main          ──────●──────────────────●────────── (Production)
                    │                  │
develop       ──●───●──●───────────●───●────────── (Staging)
                       │           │
feature/      ─────────●───●───────┘               (Feature branches)
                           │
hotfix/       ─────────────●──────────────────      (Urgent fixes)
```

**Naming convention:**
```
feature/US-001-court-booking
feature/US-002-payment-vnpay
bugfix/US-010-fix-auth-token
hotfix/US-020-payment-callback
```

**Commit message format:**
```
feat: add court availability check endpoint
fix: resolve JWT token expiry issue
docs: update API documentation
test: add unit tests for BookingService
refactor: extract payment logic to separate service
```

---

## 7. GitHub Secrets cần cấu hình

| Secret | Mô tả |
|--------|-------|
| `RENDER_DEPLOY_HOOK_URL` | Webhook URL từ Render |
| `VERCEL_TOKEN` | Vercel API token |
| `VERCEL_ORG_ID` | Vercel Org ID |
| `VERCEL_PROJECT_ID` | Vercel Project ID |
| `VITE_API_BASE_URL` | URL backend production |
| `DB_CONNECTION_STRING` | Connection string production |
| `JWT_SECRET_KEY` | JWT secret key production |

---

## 8. Checklist kỹ thuật bắt buộc

### Backend
- [ ] Layer architecture (API / Application / Domain / Infrastructure)
- [ ] DTO với AutoMapper
- [ ] FluentValidation cho tất cả request
- [ ] JWT Authentication + Refresh Token
- [ ] EF Core Migration + Seed Data
- [ ] Exception Middleware global
- [ ] Pagination / Filter / Search chuẩn
- [ ] Swagger với JWT support
- [ ] Logging với Serilog
- [ ] SignalR cho realtime status

### Frontend
- [ ] TypeScript strict mode
- [ ] Protected Routes theo role
- [ ] Axios interceptor tự động refresh token
- [ ] React Hook Form + Zod validation
- [ ] Loading states cho mọi async action
- [ ] Error boundaries
- [ ] Toast notifications
- [ ] Responsive design (mobile-first)

### DevOps
- [ ] Dockerfile cho backend
- [ ] GitHub Actions CI/CD
- [ ] Environment variables tách biệt
- [ ] README hướng dẫn deploy

---

*Tài liệu kỹ thuật — PRN232 Sports Court Management System — v1.0*

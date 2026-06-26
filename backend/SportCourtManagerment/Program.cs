using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SportCourtManagerment.Data;
using SportCourtManagerment.Services;
using SportCourtManagerment.Services.Email;

namespace SportCourtManagerment;

/// <summary>Application entry point — configures DI, middleware, and startup tasks.</summary>
public class Program
{
  /// <summary>Main application entry.</summary>
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // ══════════════════════════════════════════
    //  Services
    // ══════════════════════════════════════════

    // Database (EF Core Code First)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
      options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("SportCourtManagerment")
      )
    );

    // ── Application Services ──────────────────
    builder.Services.AddScoped<TokenService>();
    builder.Services.AddScoped<IEmailService, EmailService>();

    // ── JWT Authentication ────────────────────
    var jwtSection = builder.Configuration.GetSection("Jwt");
    var secretKey  = jwtSection["Secret"]!;

    builder.Services
      .AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
          ValidateIssuer           = true,
          ValidIssuer              = jwtSection["Issuer"],
          ValidateAudience         = true,
          ValidAudience            = jwtSection["Audience"],
          ValidateLifetime         = true,
          ClockSkew                = TimeSpan.Zero, // no grace period — token expires exactly on time
        };
      });

    // ── Authorization ─────────────────────────
    builder.Services.AddAuthorization();

    // ── Controllers ───────────────────────────
    builder.Services.AddControllers();

    // ── CORS (allow React dev server) ─────────
    builder.Services.AddCors(options =>
    {
      options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
    });

    // ── Swagger / OpenAPI ─────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
      c.SwaggerDoc("v1", new()
      {
        Title       = "Sports Court Management API",
        Version     = "v1",
        Description = "API quản lý sân thể thao — PRN232",
      });

      // Add "Authorize" button in Swagger UI so we can test protected endpoints
      c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Nhập Access Token theo định dạng: Bearer {token}",
      });
      c.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
            {
              Type = ReferenceType.SecurityScheme,
              Id   = "Bearer",
            },
          },
          Array.Empty<string>()
        },
      });
    });

    var app = builder.Build();

    // ══════════════════════════════════════════
    //  Startup Tasks
    // ══════════════════════════════════════════

    // Auto-migrate and seed in development
    if (app.Environment.IsDevelopment())
    {
      using var scope     = app.Services.CreateScope();
      var       dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      await dbContext.Database.MigrateAsync();
      await DbSeeder.SeedAsync(dbContext);
    }

    // ══════════════════════════════════════════
    //  Middleware Pipeline
    // ══════════════════════════════════════════

    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sports Court API v1");
        //c.RoutePrefix = string.Empty; // Swagger at root "/"
      });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFrontend");

    // ORDER MATTERS: Authentication must come before Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    await app.RunAsync();
  }
}

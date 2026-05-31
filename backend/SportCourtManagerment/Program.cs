using Microsoft.EntityFrameworkCore;
using SportCourtManagerment.Data;

namespace SportCourtManagerment;

/// <summary>Application entry point — configures DI, middleware, and startup tasks.</summary>
public class Program
{
  /// <summary>Main application entry.</summary>
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // ==================== Services ====================

    // Database (EF Core Code First)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
      options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("SportCourtManagerment")
      )
    );

    // Controllers (Controller-based API pattern)
    builder.Services.AddControllers();

    // Authorization
    builder.Services.AddAuthorization();

    // Swagger / OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
      c.SwaggerDoc("v1", new()
      {
        Title = "Sports Court Management API",
        Version = "v1",
        Description = "API quản lý sân thể thao — PRN232"
      });
    });

    var app = builder.Build();

    // ==================== Startup Tasks ====================

    // Auto-migrate and seed in development
    if (app.Environment.IsDevelopment())
    {
      using var scope = app.Services.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      await dbContext.Database.MigrateAsync();
      await DbSeeder.SeedAsync(dbContext);
    }

    // ==================== Middleware ====================

    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sports Court API v1");
        c.RoutePrefix = string.Empty; // Swagger at root "/"
      });
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
  }
}

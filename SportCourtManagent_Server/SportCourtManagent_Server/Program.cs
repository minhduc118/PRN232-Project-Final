using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DataAccess.Implementation;
using SportCourtManagent_Server.Services.Interfaces;
using SportCourtManagent_Server.Services.Implements;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.OData;

// Disable inotify FileSystemWatcher to prevent IOException on Linux/Render containers (inotify limit 128)
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Repository DI registration
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionMatrixRepository, PermissionMatrixRepository>();
builder.Services.AddScoped<IMembershipTierRepository, MembershipTierRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<ICourtTypeRepository, CourtTypeRepository>();
builder.Services.AddScoped<ICourtComplexRepository, CourtComplexRepository>();
builder.Services.AddScoped<ICourtRepository, CourtRepository>();
builder.Services.AddScoped<ICourtImageRepository, CourtImageRepository>();
builder.Services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
builder.Services.AddScoped<ICourtPricingRepository, CourtPricingRepository>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddSingleton<IInMemoryBookingRepository, InMemoryBookingRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IBookingServiceRepository, BookingServiceRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ICoachScheduleRepository, CoachScheduleRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IRecurringBookingRepository, RecurringBookingRepository>();
builder.Services.AddScoped<IWaitlistRepository, WaitlistRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IEquipmentInventoryRepository, EquipmentInventoryRepository>();
builder.Services.AddScoped<IMaintenanceScheduleRepository, MaintenanceScheduleRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffShiftRepository, StaffShiftRepository>();
builder.Services.AddScoped<IPlayerRequestRepository, PlayerRequestRepository>();
builder.Services.AddScoped<IPlayerRequestMemberRepository, PlayerRequestMemberRepository>();
builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();

builder.Services.AddScoped<IComplexCourtTypeServiceRepository, ComplexCourtTypeServiceRepository>();

// Service DI registration
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<IComplexCourtTypeOfferingService, ComplexCourtTypeOfferingService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserAccessService, UserAccessService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IBookingManagementService, BookingManagementService>();
builder.Services.AddScoped<ICourtService, CourtService>();
builder.Services.AddScoped<ICourtComplexService, CourtComplexService>();
builder.Services.AddScoped<ICourtTypeService, CourtTypeService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICourtBookingService, CourtBookingService>();
builder.Services.AddScoped<ISePayService, SePayService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddSingleton<ITournamentLockManager, TournamentLockManager>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IMaintenanceScheduleService, MaintenanceScheduleService>();
builder.Services.AddScoped<ITaskItemService, TaskItemService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

}).AddOData(options => options
    .Select()
    .Filter()
    .OrderBy()
    .SetMaxTop(100)
    .Count()
    .Expand()
    .AddRouteComponents("odata", GetEdmModel()));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-min-32-chars-long-sports-court!!";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "SportCourtManagent_Server",
        ValidAudience = jwtSettings["Audience"] ?? "SportCourtClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SportCourtManagent_Server API", Version = "v1" });
    
    // Add JWT support to Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Nhập 'Bearer [token]' của bạn vào đây."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 128 * 1024; // 128 KB
    options.Limits.MaxRequestHeaderCount = 200;
});

var app = builder.Build();

if (args.Contains("--reset-db"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.ExecuteSqlRaw(@"
            DECLARE @sql NVARCHAR(MAX) = N'';
            SELECT @sql += 'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' DROP CONSTRAINT ' + QUOTENAME(f.name) + ';' + CHAR(13)
            FROM sys.foreign_keys f
            INNER JOIN sys.tables t ON f.parent_object_id = t.object_id
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;

            SELECT @sql += 'DROP TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ';' + CHAR(13)
            FROM sys.tables t
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id;

            EXEC sp_executesql @sql;
        ");
        Console.WriteLine("[DB Reset] All tables dropped successfully from db60780.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB Reset Error] {ex.Message}");
    }
    return;
}

app.Use(async (context, next) =>
{
    foreach (var cookieKey in context.Request.Cookies.Keys)
    {
        if (cookieKey.StartsWith(".AspNetCore.TempData", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Cookies.Delete(cookieKey);
        }
    }
    await next();
});

// Auto-migrate and seed database
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    try
//    {
//        await dbContext.Database.MigrateAsync();
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"[Migrate Notice] {ex.Message}");
//    }

//    try
//    {
//        await DbSeeder.SeedAsync(dbContext);
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"[Seed Notice] {ex.Message}");
//    }
//}


// Configure the HTTP request pipeline. Always enable Swagger for checking
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SportCourtManagent_Server API v1");
    c.RoutePrefix = "swagger";
});

// Redirect root URL directly to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SportCourtManagent_Server.Hubs.SlotStatusHub>("/hubs/slot-status");

app.Run();


Microsoft.OData.Edm.IEdmModel GetEdmModel()
{
    var builder = new Microsoft.OData.ModelBuilder.ODataConventionModelBuilder();
    builder.EntitySet<SportCourtManagent_Server.DTOs.Court.CourtListDto>("Courts").EntityType.HasKey(c => c.CourtId);
    builder.EntitySet<SportCourtManagent_Server.DTOs.Review.ReviewDto>("Reviews").EntityType.HasKey(r => r.ReviewId);
    return builder.GetEdmModel();
}

using Microsoft.EntityFrameworkCore;
using SportCourtManagent_Server.Models;
using SportCourtManagent_Server.DataAccess.Interfaces;
using SportCourtManagent_Server.DataAccess.Implementation;
using System.Text.Json.Serialization;
using SportCourtManagent_Server.Services.Interfaces;
using SportCourtManagent_Server.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();
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
builder.Services.AddScoped<IStaffShiftRepository, StaffShiftRepository>();
builder.Services.AddScoped<IPlayerRequestRepository, PlayerRequestRepository>();
builder.Services.AddScoped<IPlayerRequestMemberRepository, PlayerRequestMemberRepository>();
builder.Services.AddScoped<ITaskItemRepository, TaskItemRepository>();

// Court Booking Service
builder.Services.AddScoped<ICourtBookingService, CourtBookingService>();
builder.Services.AddScoped<ISePayService, SePayService>();
builder.Services.AddScoped<IServiceService, ServiceService>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler =
        ReferenceHandler.IgnoreCycles;
}); ;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

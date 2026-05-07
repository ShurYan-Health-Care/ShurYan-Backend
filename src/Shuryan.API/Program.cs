using Shuryan.API.Extensions;
using Shuryan.API.Hubs;
using Shuryan.Shared.Extensions;
using SwaggerThemes;
using Hangfire;
using Hangfire.SqlServer;
using Shuryan.Application.Interfaces;
using Shuryan.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Development.Local.json", optional: true, reloadOnChange: true);

#region Infrastructure Configuration 
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);
#endregion

#region Identity & Authentication 
builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
#endregion

#region Application Configuration
builder.Services.AddApplicationSettings(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddUnitOfWork();
builder.Services.AddApplicationServices();
builder.Services.AddAutoMapperProfiles();
builder.Services.AddValidation();
#endregion

#region API Configuration
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddSignalR();
builder.Services.AddRateLimiterConfiguration();
builder.Services.AddGlobalExceptionHandler();

// Hangfire — Background Job Processing
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<IPaymentExpiryJob, PaymentExpiryJob>();
#endregion


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(Theme.UniversalDark);
    await app.SeedDatabaseAsync();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(Theme.UniversalDark);
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors("ShuryanCorsPolicy");
app.UseRateLimiter();
app.UseMiddleware<Shuryan.API.Middleware.SecurityHeadersMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<VideoNotificationHub>("/hubs/video-notify");
// Hangfire Dashboard (Development only) + Recurring Jobs
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}


try
{
    // Register recurring job: expire stale pending payments every 30 minutes
    RecurringJob.AddOrUpdate<IPaymentExpiryJob>(
        "expire-pending-payments",
        job => job.ExpirePendingPaymentsAsync(),
        "*/1 * * * *"); // Every 30 minutes
}
catch (Exception ex)
{
    // Hangfire not ready yet
}

app.Run();

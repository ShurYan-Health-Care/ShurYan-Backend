using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Shuryan.API.Services;
using System.Text.Json;
using System.Threading.RateLimiting;
using Shuryan.Application.Interfaces;
using Shuryan.Application.Services;
using Shuryan.Application.Services.Auth;
using Shuryan.Application.Services.Email;
using Shuryan.Application.Services.Token;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.Repositories.LaboratoryRepositories;
using Shuryan.Core.Interfaces.Repositories.Pharmacies;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Infrastructure.Repositories.Doctors;
using Shuryan.Infrastructure.Repositories.Laboratories;
using Shuryan.Infrastructure.Repositories.Medical;
using Shuryan.Infrastructure.Repositories.Patients;
using Shuryan.Infrastructure.Repositories.Pharmacies;
using Shuryan.Infrastructure.UnitOfWork;
using Shuryan.Shared.Configurations;

namespace Shuryan.API.Extensions
{
    public static class ServiceExtensions
    {
        #region Register all application configuration settings
        public static IServiceCollection AddApplicationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<OAuthSettings>(configuration.GetSection("OAuthSettings"));
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.Configure<Shuryan.Core.Settings.PaymobSettings>(configuration.GetSection("Paymob"));
            services.Configure<Shuryan.Core.Settings.FrontendSettings>(configuration.GetSection("FrontendSettings"));

            return services;
        }
        #endregion

        #region Register all repositories
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IPharmacyRepository, PharmacyRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IDoctorConsultationRepository, DoctorConsultationRepository>();
            services.AddScoped<IConsultationRecordRepository, ConsultationRecordRepository>();
            services.AddScoped<ILabPrescriptionRepository, LabPrescriptionRepository>();

            return services;
        }
        #endregion

        #region Register Unit of Work pattern
        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
        #endregion

        #region Register all application services
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Authentication & Authorization Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();

            // Email Service
            services.AddScoped<IEmailService, EmailService>();

            // File Upload Service
            services.AddScoped<IFileUploadService, CloudinaryService>();

            // Business Services
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IVerifierService, VerifierService>();

            // Doctor Profile Services
            services.AddScoped<IClinicService, ClinicService>();
            services.AddScoped<IDoctorServicePricingService, DoctorServicePricingService>();
            services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
            services.AddScoped<IDoctorPartnerService, DoctorPartnerService>();

            // Laboratory Services
            //services.AddScoped<ILaboratoryService, LaboratoryService>();
            services.AddScoped<ILaboratoryDocumentService, LaboratoryDocumentService>();
            services.AddScoped<ILabPrescriptionService, LabPrescriptionService>();
            services.AddScoped<ILabOrderService, LabOrderService>();
            services.AddScoped<IPatientLabService, PatientLabService>();

            // Prescription Service
            services.AddScoped<IPrescriptionService, PrescriptionService>();

            // Session Management Services
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IDocumentationService, DocumentationService>();
            services.AddScoped<ILabTestService, LabTestService>();

            // Payment Services
            services.AddHttpClient<IPaymobService, PaymobService>();
            services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();

            // Pharmacy Profile Service
            services.AddScoped<IPharmacyProfileService, PharmacyProfileService>();

            // Laboratory Profile Service
            services.AddScoped<ILaboratoryProfileService, LaboratoryProfileService>();

            // Review Services
            services.AddScoped<IDoctorReviewService, DoctorReviewService>();

            // Notification Services
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationHubService, NotificationHubService>();

            return services;
        }
        #endregion

        #region Register FluentValidation validators
        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<Program>();

            return services;
        }
        #endregion

        #region Register AutoMapper profiles
        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Shuryan.Application.Mappers.MappingProfile));

            return services;
        }
        #endregion

        #region Configure Global Exception Handler
        public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
        {
            services.AddExceptionHandler<Shuryan.API.Middleware.GlobalExceptionHandler>();
            services.AddProblemDetails();

            return services;
        }
        #endregion

        #region Configure Rate Limiting
        public static IServiceCollection AddRateLimiterConfiguration(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

               options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    // 1. نحاول نجيب الـ ID بتاع المستخدم لو مسجل دخول
                    var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    
                    // 2. نعمل المفتاح (Key) اللي هنقيس عليه الكوتة
                    // لو مسجل دخول هنستخدم الـ ID بتاعه، ولو زائر خارجي هنستخدم الـ IP
                    var partitionKey = !string.IsNullOrEmpty(userId) 
                        ? $"user_{userId}" 
                        : $"ip_{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 200, // رفعنا الحد لـ 200 لتلائم الـ Production
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                options.AddPolicy("auth", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                // Payment endpoints: 5 requests/minute per user (prevents payment spam)
                options.AddPolicy("payment", context =>
                {
                    var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                        ?? context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter($"payment_{userId}", _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 5,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                // Webhook endpoints: 30 requests/minute per IP (Paymob callbacks)
                options.AddPolicy("webhook", context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter($"webhook_{ip}", _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = 30,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";
                    var response = new
                    {
                        isSuccess = false,
                        message = "Too many requests. Please slow down and try again later.",
                        statusCode = 429,
                        errors = Array.Empty<string>()
                    };
                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(response), cancellationToken);
                };
            });

            return services;
        }
        #endregion

        #region Configure Swagger/OpenAPI documentation
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Shuryan Healthcare API",
                    Version = "v1",
                    Description = "Healthcare Management System API",
                });

                // Custom Schema ID to avoid conflicts
                options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

                // Add JWT Authentication to Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid JWT token.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }
        #endregion
    }
}

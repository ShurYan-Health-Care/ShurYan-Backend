using FluentValidation;
using Microsoft.OpenApi.Models;
using Shuryan.API.Services;
using Shuryan.Application.Interfaces;
using Shuryan.Application.Services;
using Shuryan.Application.Services.AI;
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

            // AI Chat Services
            services.AddHttpClient<IGeminiAIService, GeminiAIService>();
            services.AddScoped<IChatService, ChatService>();

            // Payment Services
            services.AddHttpClient<IPaymobService, PaymobService>();
            services.AddScoped<IPaymentService, PaymentService>();
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

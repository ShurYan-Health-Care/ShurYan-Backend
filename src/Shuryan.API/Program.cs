using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shuryan.Application.Extensions;
using Shuryan.Application.Interfaces;
using Shuryan.Application.Services;
using Shuryan.Application.Services.Auth;
using Shuryan.Application.Services.Email;
using Shuryan.Application.Services.Token;
using Shuryan.Core.Interfaces.Repositories;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Infrastructure.Data;
using Shuryan.Infrastructure.Repositories.Patients;
using Shuryan.Infrastructure.Repositories.Pharmacies;
using Shuryan.Infrastructure.UnitOfWork;
using Shuryan.Shared.Configurations;
using Shuryan.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load local configuration file (not committed to Git)
builder.Configuration.AddJsonFile("appsettings.Development.Local.json", optional: true, reloadOnChange: true);

// ==================== Database Configuration ====================
builder.Services.AddDatabaseConfiguration(builder.Configuration);

// ==================== CORS Configuration ====================
builder.Services.AddCorsConfiguration(builder.Configuration);

// ==================== Identity Configuration ====================
builder.Services.AddIdentityConfiguration();

// ==================== JWT Authentication ====================
builder.Services.AddJwtAuthentication(builder.Configuration);

// ==================== Authorization Policies ====================
builder.Services.AddAuthorizationPolicies();

// ==================== Repositories ====================
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();

// ==================== Unit of Work ====================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(typeof(Shuryan.Application.Mappers.MappingProfile));


// ==================== Application Services ====================
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<ILaboratoryService, LaboratoryService>();
builder.Services.AddScoped<ILabOrderService, LabOrderService>();
builder.Services.AddScoped<ILaboratoryDocumentService, LaboratoryDocumentService>();
builder.Services.AddScoped<ILabPrescriptionService, LabPrescriptionService>();


// Register FluentValidation from Application assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
//builder.Services.AddValidatorsFromAssemblyContaining<Shuryan.Application.Services.DoctorService>();

// ==================== Application Services ====================
// Configure settings from appsettings.json
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<OAuthSettings>(builder.Configuration.GetSection("OAuthSettings"));

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();

// ==================== FluentValidation ====================
builder.Services.AddValidatorsFromAssemblyContaining<Program>();


// ==================== Controllers ====================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ==================== API Documentation ====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();

    // ==================== DATABASE SEEDING ====================
     await app.SeedDatabaseAsync();
    
    // To clear the database (USE WITH CAUTION!)
     //await app.ClearDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseCors("ShuryanCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

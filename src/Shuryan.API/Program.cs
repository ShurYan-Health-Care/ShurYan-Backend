using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Shuryan.Application.Extensions;
using Shuryan.Application.Services.Auth;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Infrastructure.Data;
using Shuryan.Infrastructure.UnitOfWork;
using Shuryan.Shared.Configurations;
using Shuryan.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

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

// ==================== Unit of Work ====================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ==================== Application Services ====================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ==================== FluentValidation ====================
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ==================== Controllers ====================
builder.Services.AddControllers();

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

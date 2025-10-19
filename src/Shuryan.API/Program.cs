using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Shuryan.Infrastructure.Data;
using Shuryan.Shared.Configurations;
using Shuryan.Shared.Extensions;
using Shuryan.Application.Extensions;
using FluentValidation.AspNetCore;
using FluentValidation;
using Shuryan.Core.Interfaces.UnitOfWork;
using Shuryan.Infrastructure.UnitOfWork;
using Shuryan.Core.Interfaces.Services;
using Shuryan.Infrastructure.Services;
using Shuryan.Application.Services;
using Shuryan.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IDoctorApplicationService, DoctorApplicationService>();

// Register FluentValidation from Application assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<DoctorApplicationService>();



builder.Services.AddControllers();
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
app.UseAuthorization();

app.MapControllers();

app.Run();

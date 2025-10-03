using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Shuryan.Infrastructure.Data;
using Shuryan.Shared.Configurations;
using Shuryan.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddCorsConfiguration(builder.Configuration);

builder.Services.AddControllers();
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
app.UseCors("ShuryanCorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();

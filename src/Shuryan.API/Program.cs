using Shuryan.API.Extensions;
using Shuryan.API.Hubs;
using Shuryan.Shared.Extensions;
using SwaggerThemes;

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
#endregion


var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
	app.UseSwagger();
    app.UseSwaggerUI(Theme.UniversalDark);

    await app.SeedDatabaseAsync();

	//await app.ClearDatabaseAsync();
//}

app.UseHttpsRedirection();
app.UseCors("ShuryanCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();

using Stross.Application.Extensions;
using Stross.Infrastructure.Extensions;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.RegisterApplication();
builder.Services.RegisterInfrastructure();

// Add OpenAPI services
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000");
            policy.AllowCredentials();
            policy.AllowAnyMethod();
            policy.WithHeaders("Authorization");
            policy.AllowAnyHeader();
        });
});

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment()) // development
{
    app.UseDeveloperExceptionPage();

    // Map OpenAPI endpoint
    app.MapOpenApi();

    // Add Scalar API documentation
    app.MapScalarApiReference(options =>
    {
        options.Title = "Stross API";
        options.Theme = ScalarTheme.Purple;
        options.ShowSidebar = true;
        options.DarkMode = true;
    });
}
else // production
{
}

app.UseRouting();
        
app.UseCors();
app.UseAuthentication();
       
app.UseAuthorization();

app.MapControllers();

app.Run();
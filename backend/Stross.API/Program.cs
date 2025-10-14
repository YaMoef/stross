using System.Text.Json.Serialization;
using Stross.Application.Extensions;
using Stross.Infrastructure.Extensions;
using Microsoft.OpenApi.Models;
using Stross.Config;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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
}
else // production
{
}

app.UseRouting();
        
app.UseCors();
app.UseAuthentication();
       
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
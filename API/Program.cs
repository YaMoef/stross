using System.Text.Json.Serialization;
using Application.Extensions;
using Infrastructure.Extensions;
using Microsoft.OpenApi.Models;
using Config;

var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, "stack.env");
Console.Out.WriteLine("Using env file: "+dotenv.ToString());
DotNetEnv.Env.Load(dotenv);

var builder = WebApplication.CreateBuilder(args);
var CorsConfig = "_corsConfig";
var connectionStringConfig = builder.Configuration.GetSection("ConnectionString").Get<ConnectionStringConfig>();

builder.Services.RegisterApplication();
builder.Services.RegisterInfrastructure(connectionStringConfig.Database);
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
        
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo() { Title = "Template", Version = "v1" });
});
        
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsConfig,
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
var app = builder.Build();
if (app.Environment.IsDevelopment()) // development
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Template v1"));
}
else // production
{
}

app.UseRouting();
        
app.UseCors(CorsConfig);
app.UseAuthentication();
       
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
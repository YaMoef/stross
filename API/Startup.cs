using System.Text.Json.Serialization;
using Application.Extensions;
using Infrastructure.Extensions;
using Microsoft.OpenApi.Models;

namespace API;

public class Startup
{
    private const string CorsConfig = "_corsConfig";
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.RegisterApplication();
        services.RegisterInfrastructure(_configuration.GetConnectionString("Database")!);
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Template", Version = "v1" });
        });
        
        services.AddCors(options =>
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
        
        services.AddAuthorization();
        
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) // development
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Template v1"));
        }
        else // production
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, "stack.env");
            Console.Out.WriteLine("Using env file: "+dotenv.ToString());
            DotEnv.Load(dotenv);
        }

        app.UseRouting();
        
        app.UseCors(CorsConfig);
        app.UseAuthentication();
       
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
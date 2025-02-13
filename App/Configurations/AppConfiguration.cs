using App.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Repository.Data;
using Serilog;
using Service;

namespace App.Configurations
{
    public static class AppConfiguration
    {
        public static void ConfigureLogging(WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            builder.Host.UseSerilog();
        }

        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            // Connecting the service layer
            builder.Services.AddServiceLayer();

            // Database connection
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Connecting controllers and Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "File Format Control", Version = "v1" });
                c.UseInlineDefinitionsForEnums();
            });
        }

        public static void ConfigureMiddleware(WebApplication app)
        {
            // Swagger Settings
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Setting up Middleware
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseAuthorization();

            // Controller routing
            app.MapControllers();
        }
    }
}

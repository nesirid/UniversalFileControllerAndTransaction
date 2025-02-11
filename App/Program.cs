using Microsoft.EntityFrameworkCore;
using Serilog;
using Repository.Data;
using App.Mappings;
using App.Middlewares;
using Service.Services.Interfaces;
using Service.Services;
using Service.Parsers;
using Microsoft.OpenApi.Models;



var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// Register services
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);


builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileTypeRecognizer, FileTypeRecognizer>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IFileParser, CsvFileParser>();
builder.Services.AddScoped<IFileParser, TxtFileParser>();
builder.Services.AddScoped<IFileParser, XmlFileParser>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "File Format Control", Version = "v1" });

    c.UseInlineDefinitionsForEnums();
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

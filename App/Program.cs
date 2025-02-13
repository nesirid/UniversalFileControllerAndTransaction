using Microsoft.EntityFrameworkCore;
using Serilog;
using Repository.Data;
using App.Middlewares;
using Service.Services.Interfaces;
using Service.Services;
using Service.Parsers;
using Microsoft.OpenApi.Models;
using Service;
using App.Configurations;


var builder = WebApplication.CreateBuilder(args);

// Logging configuration
AppConfiguration.ConfigureLogging(builder);

// Registration of services
AppConfiguration.ConfigureServices(builder);

var app = builder.Build();

// Setting up Middleware and endpoints
AppConfiguration.ConfigureMiddleware(app);

// Launching the application
app.Run();

using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace App.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            // Log error to Serilog
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            var errorResponse = new
            {
                Success = false,
                Message = "An error occurred while processing the request.",
                Errors = new List<string>()
            };

            switch (exception)
            {
                //case ValidationException ex:
                //    response.StatusCode = (int)HttpStatusCode.BadRequest;
                //    errorResponse = new
                //    {
                //        Success = false,
                //        Message = "Validation failed",
                //        Errors = ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList()
                //    };
                //    break;

                case DbUpdateException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse = new
                    {
                        Success = false,
                        Message = "Database update error",
                        Errors = new List<string> { exception.Message }
                    };
                    break;

                case AuthenticationException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse = new
                    {
                        Success = false,
                        Message = "Authentication failed",
                        Errors = new List<string> { exception.Message }
                    };
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse = new
                    {
                        Success = false,
                        Message = "Internal server error",
                        Errors = new List<string> { exception.Message }
                    };
                    break;
            }

            // Convert to JSON
            var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true });

            // Send response
            await context.Response.WriteAsync(result);
        }
    }
}

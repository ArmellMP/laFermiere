// PresentationAPI/Middlewares/ExceptionMiddleware.cs
using System.Net;
using System.Text.Json;
using Application.Common.Exceptions;
using FluentValidation;

namespace PresentationAPI.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Une erreur est survenue: {Message}", exception.Message);

            var response = context.Response;
            response.ContentType = "application/json";

            var result = new
            {
                Success = false,
                Message = GetUserFriendlyMessage(exception),
                Errors = GetErrors(exception),
                TraceId = context.TraceIdentifier,
                StackTrace = _env.IsDevelopment() ? exception.StackTrace : null
            };

            response.StatusCode = GetStatusCode(exception);
            await response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                NotFoundException => (int)HttpStatusCode.NotFound,
                BusinessRuleException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private static string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                ValidationException => "Les données envoyées ne sont pas valides.",
                NotFoundException => exception.Message,
                BusinessRuleException => exception.Message,
                UnauthorizedAccessException => "Vous n'êtes pas autorisé à effectuer cette action.",
                _ => "Une erreur technique est survenue. Veuillez réessayer plus tard."
            };
        }

        private static List<string>? GetErrors(Exception exception)
        {
            if (exception is ValidationException validationException)
            {
                return validationException.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
            }

            return null;
        }
    }
}
using System.Net;
using System.Text.Json;

namespace Warehouse.API.Middleware
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

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Бизнес-ошибка. Путь: {Path}, Метод: {Method}", context.Request.Path, context.Request.Method);
                await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Данные не найдены. Путь: {Path}, Метод: {Method}", context.Request.Path, context.Request.Method);
                await HandleExceptionAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Внутренняя ошибка сервера. Путь: {Path}, Метод: {Method}", context.Request.Path, context.Request.Method);
                await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Произошла непредвиденная ошибка.");
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                error = message,
                statusCode = (int)statusCode
            });

            return context.Response.WriteAsync(result);
        }
    }
}
using User.Exceptions;
using User.Middlewares.Models;

namespace User.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception was thrown during the request");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            BadHttpRequestException badRequest => StatusCodes.Status400BadRequest,
            ApiException apiException => apiException.HttpStatusCode,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(new ErrorDetailsModel
        {
            StatusCode = context.Response.StatusCode,
            Message = !string.IsNullOrWhiteSpace(exception.Message)
                ? exception.Message.Replace('"', '\'')
                : "There was an error during your request"
        }.ToString());
    }
}
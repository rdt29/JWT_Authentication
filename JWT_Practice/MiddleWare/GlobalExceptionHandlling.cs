using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using JWT_Practice.Model;
using System.Runtime.InteropServices;
using Serilog;

namespace JWT_Practice.MiddleWare
{
    public class GlobalExceptionHandlling
    {
        private readonly RequestDelegate _next;
        //private readonly ILogger<GlobalExceptionHandlling> _logger;

        public GlobalExceptionHandlling(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exe)
            {
                //_logger.LogError(exe, exe.Message);
                Log.Error(exe, exe.Message);
                //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await HandleExceptionAsync(context, exe);
            }
            Log.CloseAndFlush();
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            int statusCode;
            //await context.Response.WriteAsync(new ErrorDetails()
            //{
            //    StatusCode = context.Response.StatusCode,
            //    Message = "Internal Server Error from the custom middleware."
            //}.ToString());

            //context.Response.ContentType = "application/json";
            //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            statusCode = exception switch
            {
                AccessViolationException => StatusCodes.Status404NotFound,
                DivideByZeroException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError,
            };

            //Exception e => "Exe",

            //switch (exception)
            //{
            //    case Exception:
            //        message = "Global Exception in  middleware";
            //        // handle other exceptions
            //        break;

            //    default:
            //        // unhandled exception
            //        break;
            //}

            //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //return context.Response.WriteAsync("An unhandled exception occurred.");
            //}
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = statusCode,
                Message = exception.Message
            }.ToString());
        }
    }
}
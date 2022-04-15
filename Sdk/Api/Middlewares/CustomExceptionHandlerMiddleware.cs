using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Sdk.Api.Dtos;
using Sdk.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sdk.Api.Middlewares
{
    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
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

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var validationFailures = new List<ValidationFailure>();
            var code = HttpStatusCode.InternalServerError;

            switch (ex)
            {
                case CustomException customException:
                    {
                        code = customException.StatusCode;
                        validationFailures.Add(new ValidationFailure
                        {
                            ErrorMessage = customException.Message,
                        });
                        break;
                    }
                case ValidationException validationException:
                    {
                        code = HttpStatusCode.BadRequest;
                        validationFailures = validationException.Errors.ToList();
                        break;
                    }
                case Exception exception:
                    {
                        validationFailures.Add(new ValidationFailure
                        {
                            ErrorMessage = exception.Message,
                        });
                        break;
                    }
            }

            await BuildApiResponseDto(context, new ApiResponseDto
            {
                Errors = validationFailures
            }, (int)code);
        }

        private static Task BuildApiResponseDto(HttpContext context, ApiResponseDto apiResponseDto,
            int statusCode)
        {
            string response = JsonConvert.SerializeObject(apiResponseDto);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(response, Encoding.UTF8);
        }
    }

    public static class CustomExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
        }
    }
}

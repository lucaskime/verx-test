using Microsoft.AspNetCore.Mvc;
using Transaction.Application.Validation;

namespace Transaction.WebApi.Responses;

// Wires every failure path outside the handlers (model binding, unmatched routes, exceptions)
// to the same envelope the controllers return.
internal static class ApiResponseRegistration
{
    private const string InvalidValueMessage = "The value is invalid.";

    public static IMvcBuilder AddApiResponses(this IMvcBuilder mvc) => mvc
        // Contracts declare [Required] explicitly; the implicit one only adds "The request field is required." noise.
        .AddMvcOptions(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
        // Serializer messages expose CLR type names and byte positions; clients get the field and a neutral message.
        .AddJsonOptions(options => options.AllowInputFormatterExceptionMessages = false)
        .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.SelectMany(entry => entry.Value!.Errors.Select(error => new ApiError(
                ValidationErrors.InvalidCode,
                string.IsNullOrEmpty(error.ErrorMessage) ? InvalidValueMessage : error.ErrorMessage,
                ApiResponses.ToJsonPath(entry.Key))));
            return new BadRequestObjectResult(ApiResponses.Failure<object>(errors, context.HttpContext));
        });

    public static WebApplication UseApiResponses(this WebApplication app)
    {
        // The middleware logs the exception and answers aborted requests itself; details never leak.
        app.UseExceptionHandler(new ExceptionHandlerOptions
        {
            ExceptionHandler = context => ApiResponses.WriteFailureAsync(context,
                StatusCodes.Status500InternalServerError,
                new ApiError("Server.Unexpected", "An unexpected error occurred.")),
        });
        app.UseStatusCodePages(context => ApiResponses.WriteFailureAsync(context.HttpContext,
            context.HttpContext.Response.StatusCode, ApiResponses.HttpError(context.HttpContext.Response.StatusCode)));
        return app;
    }
}

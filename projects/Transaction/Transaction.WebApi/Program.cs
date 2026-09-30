using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Transaction.IoC;
using Transaction.WebApi.Responses;
using Transaction.WebApi.Security;

namespace Transaction.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddTransaction();

        builder.Services.AddControllers()
            // Enums travel by name ("Debit"), never by number: the contract does not depend on member order.
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(allowIntegerValues: false)))
            .AddApiResponses();
        builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerOpenApiTransformer>());
        builder.Services.AddJwtAuthentication();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/");
        }

        app.UseApiResponses();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }
}

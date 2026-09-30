using System.Text.Json.Serialization;
using BalanceProjection.IoC;
using BalanceProjection.WebApi.Responses;
using BalanceProjection.WebApi.Security;
using Scalar.AspNetCore;

namespace BalanceProjection.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddBalanceProjection();

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

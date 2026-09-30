using Identity.WebApi.Features.Auth.Login;
using Identity.WebApi.Features.Auth.Register;
using Identity.WebApi.Features.Users.Me;

namespace Identity.WebApi.Features;

public static class FeaturesServiceCollectionExtensions
{
    /// <summary>Casos de uso (vertical slices).</summary>
    public static IServiceCollection AddFeatures(this IServiceCollection services)
    {
        services.AddScoped<IRegisterUserHandler, RegisterUserHandler>();
        services.AddScoped<ILoginUserHandler, LoginUserHandler>();
        services.AddScoped<IGetCurrentUserHandler, GetCurrentUserHandler>();

        return services;
    }
}

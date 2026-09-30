namespace Identity.WebApi.Configuration;

public static class OptionsConfiguration
{
    /// <summary>
    /// Ponto único de registro das options da aplicação. Cada uma é ligada à sua seção do
    /// appsettings e validada por DataAnnotations já no startup: configuração inválida
    /// derruba a aplicação ao subir, e não na primeira requisição que precisar dela.
    /// </summary>
    public static IServiceCollection AddAppOptions(this IServiceCollection services)
    {
        services.AddValidatedOptions<DatabaseOptions>(DatabaseOptions.SectionName);
        services.AddValidatedOptions<JwtOptions>(JwtOptions.SectionName);

        return services;
    }

    private static void AddValidatedOptions<TOptions>(this IServiceCollection services, string sectionName)
        where TOptions : class =>
        services.AddOptions<TOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
}

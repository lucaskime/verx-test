using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace BalanceProjection.WebApi.Security;

// Identity publishes only a JWKS (no OIDC discovery document), so the key set is fetched straight from its URL.
// ConfigurationManager caches it and refreshes on unknown "kid", which covers key rotation.
internal sealed class JwksConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
{
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
        string address, IDocumentRetriever retriever, CancellationToken cancel)
    {
        var json = await retriever.GetDocumentAsync(address, cancel);
        var keySet = new JsonWebKeySet(json);

        var configuration = new OpenIdConnectConfiguration { JwksUri = address };
        foreach (var key in keySet.GetSigningKeys())
            configuration.SigningKeys.Add(key);

        return configuration;
    }
}

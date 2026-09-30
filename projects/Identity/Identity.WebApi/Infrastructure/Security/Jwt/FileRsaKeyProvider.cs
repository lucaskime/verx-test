using System.Security.Cryptography;
using System.Text.Json;
using Identity.WebApi.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.WebApi.Infrastructure.Security.Jwt;

/// <summary>
/// Gera um par de chaves RSA na primeira execução e o persiste em disco, para que o
/// mesmo par (e o mesmo "kid") sobreviva a reinícios do serviço e permita que os demais
/// microserviços validem tokens via JWKS de forma consistente.
/// </summary>
public sealed class FileRsaKeyProvider : IRsaKeyProvider, IDisposable
{
    private readonly RSA _rsa;

    public string KeyId { get; }

    public FileRsaKeyProvider(IOptions<JwtOptions> jwtOptions, IWebHostEnvironment environment)
    {
        var configuredPath = jwtOptions.Value.SigningKeyPath;
        var fullPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);

        if (File.Exists(fullPath))
        {
            var stored = JsonSerializer.Deserialize<StoredKey>(File.ReadAllText(fullPath))
                ?? throw new InvalidOperationException($"Não foi possível ler a chave de assinatura em '{fullPath}'.");

            _rsa = RSA.Create();
            _rsa.ImportFromPem(stored.PrivateKeyPem);
            KeyId = stored.KeyId;
        }
        else
        {
            _rsa = RSA.Create(2048);
            KeyId = Guid.NewGuid().ToString("N");

            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var stored = new StoredKey(KeyId, _rsa.ExportPkcs8PrivateKeyPem());
            File.WriteAllText(fullPath, JsonSerializer.Serialize(stored));
        }
    }

    public RsaSecurityKey GetSigningKey() => new(_rsa) { KeyId = KeyId };

    public JsonWebKeySet GetPublicJwks()
    {
        var parameters = _rsa.ExportParameters(includePrivateParameters: false);

        var jwk = new JsonWebKey
        {
            Kty = JsonWebAlgorithmsKeyTypes.RSA,
            Use = "sig",
            Alg = SecurityAlgorithms.RsaSha256,
            Kid = KeyId,
            N = Base64UrlEncoder.Encode(parameters.Modulus),
            E = Base64UrlEncoder.Encode(parameters.Exponent),
        };

        var keySet = new JsonWebKeySet();
        keySet.Keys.Add(jwk);
        return keySet;
    }

    public void Dispose() => _rsa.Dispose();

    private sealed record StoredKey(string KeyId, string PrivateKeyPem);
}

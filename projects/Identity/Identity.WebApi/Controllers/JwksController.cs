using Identity.WebApi.Infrastructure.Security.Jwt;
using Microsoft.AspNetCore.Mvc;

namespace Identity.WebApi.Controllers;

/// <summary>
/// Expõe o conjunto de chaves públicas (JWKS) para que outros microsserviços
/// validem a assinatura dos tokens emitidos por este serviço, sem compartilhar segredos.
/// </summary>
[ApiController]
[Route(".well-known")]
public sealed class JwksController(IRsaKeyProvider keyProvider) : ControllerBase
{
    [HttpGet("jwks.json")]
    public IActionResult GetJwks() => Ok(keyProvider.GetPublicJwks());
}

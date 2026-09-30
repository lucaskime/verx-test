namespace Identity.WebApi.Infrastructure.Security.Cryptography;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}

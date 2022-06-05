using Test_Builder.Models;

namespace Test_Builder.Services
{
    public interface ITokenService
    {
        string BuildToken(string customerId);
        bool ValidateToken(string token);
    }
}

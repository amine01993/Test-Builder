using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Test_Builder.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private const int EXPIRY_DURATION_MINUTES = 60 * 24;

        public TokenService(IConfiguration _configuration)
        {
            this._configuration = _configuration;
        }

        public string BuildToken(string customerId)
        {
            string key = _configuration["Jwt:Key"], issuer = _configuration["Jwt:Issuer"], audience = _configuration["Jwt:Audience"];

            var claims = new[] { 
                new Claim(ClaimTypes.Name, customerId), // user_id
                new Claim(ClaimTypes.Role, "Customer"), // role
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(issuer, audience, claims,
                expires: DateTime.Now.AddMinutes(EXPIRY_DURATION_MINUTES), signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public bool ValidateToken(string token)
        {
            string key = _configuration["Jwt:Key"], issuer = _configuration["Jwt:Issuer"], audience = _configuration["Jwt:Audience"];

            var secret = Encoding.UTF8.GetBytes(key);
            var securityKey = new SymmetricSecurityKey(secret);
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = securityKey
                }, out SecurityToken validatedToken);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

using Test_Builder.Services;
using Test_Builder.Models;

namespace Test_Builder.Controllers
{
    [ApiController]
    [Route("api/token")]
    [Produces("application/json")]
    public class TokenController : ControllerBase
    {
        private readonly IDBHelper _DBHelper;
        private readonly ITokenService _token;

        public TokenController(IDBHelper _DBHelper, ITokenService _token)
        {
            this._DBHelper = _DBHelper;
            this._token = _token;
        }

        // api/token/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] Customer customer)
        {
            if (ModelState.IsValid)
            {
                var query = @"INSERT INTO customer(name, email, password)
                    OUTPUT INSERTED.id
                    VALUES(@name, @email, @password)";

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(customer.Password);

                var parameters = new Dictionary<string, object>();
                parameters.Add("name", customer.Name);
                parameters.Add("email", customer.Email);
                parameters.Add("password", hashedPassword);
                
                // return Id
                var id = _DBHelper.Write(query, parameters);

                var token = _token.BuildToken(id.ToString());

                return new JsonResult(new { result = 1, token });
            }

            var errorsDict = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(error => error.ErrorMessage)
            );

            return new JsonResult(new { result = 0, errors = errorsDict });
        }

        // api/token
        [HttpPost]
        public IActionResult Login([FromBody] Customer customer)
        {
            string query = $@"SELECT id AS Id, password AS Password
                FROM customer
                WHERE email = @email";

            var parameters = new Dictionary<string, object>() { {"email", customer.Email } };

            var res = _DBHelper.Query2<Customer>(query, parameters);

            if(res != null)
            {
                var validPass = BCrypt.Net.BCrypt.Verify(customer.Password, res.Password);

                if (validPass)
                {
                    var generatedToken = _token.BuildToken(res.Id.ToString());
                    return new JsonResult(new { result = 1, token = generatedToken });
                }

                return new JsonResult(new { result = 0,
                    errors = new Dictionary<string, string[]>()
                    {{ "Password", new string[] { "Incorrect password" } }}
                });
            }

            return new JsonResult(new { result = 0,
                errors = new Dictionary<string, string[]>()
                    {{ "Email", new string[] { "Email not found" } }}
            });
        }

        // api/token/auth
        [HttpGet("auth")]
        [Authorize]
        public IActionResult Get()
        {
            var customer = _DBHelper.Query2<Customer>(
                @"SELECT name AS Name FROM customer WHERE id = @id",
                new Dictionary<string, object>() { { "id", User.Identity.Name } }
            );

            return new JsonResult(new { name = customer.Name, loggedIn = true });
        }
    }
}

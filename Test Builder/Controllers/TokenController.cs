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
        private readonly ITokenService _token;

        public TokenController(ITokenService _token)
        {
            this._token = _token;
        }

        // api/token/register
        [HttpPost("register")]
        public IActionResult Register([FromServices] ICustomerService customerService, [FromBody] Customer customer)
        {
            if (ModelState.IsValid)
            {
                var id = customerService.Insert(customer);

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
        public IActionResult Login([FromServices] ICustomerService customerService, [FromBody] Customer customer)
        {
            var res = customerService.GetByEmail(customer.Email);

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
        public IActionResult Get([FromServices] ICustomerService customerService)
        {
            var customer = customerService.Get(int.Parse(User.Identity.Name));

            return new JsonResult(new { name = customer.Name, loggedIn = true });
        }
    }
}

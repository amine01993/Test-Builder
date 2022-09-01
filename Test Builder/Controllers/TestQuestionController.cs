using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Test_Builder.Services;
using Test_Builder.Models;

namespace Test_Builder.Controllers
{
    [ApiController]
    [Route("api/test-question")]
    [Produces("application/json")]
    public class TestQuestionController : ControllerBase
    {
        private readonly IDBHelper _DBHelper;

        public TestQuestionController(IDBHelper DBHelper)
        {
            _DBHelper = DBHelper;
        }

        //// Get: api/test-question/get/1
        //[HttpGet]
        //[Authorize]
        //public IActionResult Get()

        // Post: api/test-question/positions
        [HttpPost("positions")]
        [Authorize]
        public IActionResult Positions(IEnumerable<Position> positions)
        {
            foreach(var position in positions)
            {
                _DBHelper.Write(
                    @"UPDATE test_question
                    SET position = @position
                    WHERE id = @id AND customer_id = @customer_id",
                    new Dictionary<string, object> { { "position", position._Position },
                        { "id", position.Id }, { "customer_id", User.Identity.Name }}
                );
            }

            return new JsonResult(null);
        }

        // Delete: api/test-question/delete/5
        [HttpDelete("delete/{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            _DBHelper.Write(
                @"DELETE FROM test_question WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object>()
                { {"id", id}, {"customer_id", User.Identity.Name} }
            );

            return new JsonResult(null);
        }
    }
}

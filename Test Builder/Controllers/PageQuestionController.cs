using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Test_Builder.Services;
using Test_Builder.Models;

namespace Test_Builder.Controllers
{
    [ApiController]
    [Route("api/page-question")]
    [Produces("application/json")]
    public class PageQuestionController : ControllerBase
    {
        public PageQuestionController()
        {
        }

        // Post: api/page-question/positions
        [HttpPost("positions")]
        [Authorize]
        public IActionResult Positions([FromServices] IPageQuestionService pageQuestionService, IEnumerable<Position> positions)
        {
            pageQuestionService.SetPositions(positions);

            return new JsonResult(null);
        }

        // Delete: api/page-question/delete/5
        [HttpDelete("delete/{id}")]
        [Authorize]
        public IActionResult Delete([FromServices] IPageQuestionService pageQuestionService, int id)
        {
            pageQuestionService.Delete(id);

            return new JsonResult(null);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Test_Builder.Services;
using Test_Builder.Models;

namespace Test_Builder.Controllers
{
    [ApiController]
    [Route("api/page")]
    [Produces("application/json")]
    public class PageController : ControllerBase
    {
        public PageController(IDBHelper DBHelper)
        {
        }

        // Get: api/page/get/5
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get([FromServices] IPageService pageService, int id)
        {
            var page = pageService.Get(id);

            return new JsonResult(page);
        }

        // Post: api/page/add
        [HttpPost("add")]
        [Authorize]
        public IActionResult Add([FromServices] IPageService pageService, [FromBody] Page page)
        {
            if (ModelState.IsValid)
            {
                pageService.InsertOrUpdate(page);

                return new JsonResult(page);
            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage)
            );

            return new JsonResult(errors) { StatusCode = 422 };
        }

        // Post: api/page/positions
        [HttpPost("positions")]
        [Authorize]
        public IActionResult Positions(
            [FromServices] IPageService pageService, 
            [FromBody] IEnumerable<Position> positions, [FromQuery] int testId
        )
        {
            pageService.SetPositions(positions, testId);
            return new JsonResult(null);
        }

        // Delete: api/page/delete/5
        [HttpDelete("delete/{id}")]
        [Authorize]
        public IActionResult Delete([FromServices] IPageService pageService, int id, [FromQuery] int testId)
        {
            pageService.Delete(id, testId);
            return new JsonResult(null);
        }
    }
}

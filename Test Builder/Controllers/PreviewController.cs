using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Test_Builder.Services;
using Test_Builder.Models;

namespace Test_Builder.Controllers
{
    [ApiController]
    [Route("api/preview")]
    [Produces("application/json")]
    public class PreviewController : ControllerBase
    {
        public PreviewController()
        {
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get([FromServices] IPreviewService previewService, int id)
        {
            var test = previewService.Get(id);

            return new JsonResult(test);
        }

        //// Post: api/test/add
        //[HttpPost("add")]
        //[Authorize]
        //public IActionResult Add([FromServices] ITestService testService, [FromBody] Test test)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if(test.Id == 0)
        //        {
        //            test.Id = testService.Add(test);
        //        }

        //        return new JsonResult(new { id = test.Id });
        //    }

        //    var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(
        //        kvp => kvp.Key,
        //        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage)
        //    );

        //    return new JsonResult(errors) { StatusCode = 422 };
        //}
    }
}

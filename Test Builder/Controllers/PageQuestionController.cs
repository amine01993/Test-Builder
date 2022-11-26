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

        // Post: api/page-question/add
        [HttpPost("add/{pageId}")]
        [Authorize]
        public IActionResult Add(
            [FromServices] IPageQuestionService pageQuestionService,
            IEnumerable<int> questionIds, int pageId
        )
        {
            var existantQuestionIds = pageQuestionService.GetUsedQuestions(questionIds, pageId);
            var position = pageQuestionService.MaxPosition(pageId);
            // add these questions to the page
            foreach(var questionId in questionIds)
            {
                if (existantQuestionIds.Contains(questionId))
                    continue;
                pageQuestionService.Insert(new PageQuestion() { 
                    PageId = pageId,
                    QuestionId = questionId,
                    Position = position++,
                });
            }
            
            return new JsonResult(null);
        }
    }
}

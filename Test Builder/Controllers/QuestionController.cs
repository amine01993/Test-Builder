using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Test_Builder.Services;
using Test_Builder.Models;
using Newtonsoft.Json;

namespace Test_Builder.Controllers
{
    [ApiController]
    [Route("api/question")]
    [Produces("application/json")]
    public class QuestionController : ControllerBase
    {
        private readonly IDBHelper dBHelper;

        public QuestionController(IDBHelper dBHelper)
        {
            this.dBHelper = dBHelper;
        }

        // Get: api/question/types
        [HttpGet("types")]
        public IActionResult GetTypes([FromServices] IQuestionTypeService questionTypeService)
        {
            var questionTypes = questionTypeService.List();

            return new JsonResult(questionTypes);
        }

        // Get: api/question/search
        [HttpGet("search")]
        [Authorize]
        public IActionResult Search([FromServices] IQuestionService questionService, [FromQuery] DataParameters parameters)
        {
            var result = questionService.Search(parameters);

            return new JsonResult(result);
        }

        // Post: api/question/add/1
        [HttpPost("add/{pageId?}")]
        [Authorize]
        public IActionResult Add(
            [FromServices] IQuestionService questionService,
            [FromServices] IPageQuestionService pageQuestionService,
            [FromBody] Question question, int? pageId
        )
        {
            if (ModelState.IsValid)
            {
                var questionId = questionService.Insert(question);

                if(pageId.HasValue)
                {
                    var pageQuestion = new PageQuestion { PageId = pageId.Value, Random = false, QuestionId = questionId };

                    if(TryValidateModel(pageQuestion))
                    {
                        var maxPosition = pageQuestionService.MaxPosition(pageId.Value);
                        pageQuestion.Position = maxPosition;

                        pageQuestionService.Insert(pageQuestion);
                    }
                    else
                    {
                        return new JsonResult(new { page = "Can't add question to this page" }) { StatusCode = 422 };
                    }
                }

                return new JsonResult(new { });
            }

            var errorsDict = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(error => error.ErrorMessage)
            );

            return new JsonResult(errorsDict) { StatusCode = 422 };
        }

        // Post: api/question/edit
        [HttpPost("edit")]
        [Authorize]
        public IActionResult Edit([FromServices] IQuestionService questionService, [FromBody] Question question)
        {
            if (ModelState.IsValid)
            {
                questionService.Update(question);

                return new JsonResult(new { });
            }

            var errorsDict = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(error => error.ErrorMessage)
            );

            return new JsonResult(errorsDict) { StatusCode = 422 };
        }

        // Post: api/question/duplicate/1
        [HttpPost("duplicate/{id}")]
        [Authorize]
        public IActionResult Duplicate([FromServices] IQuestionService questionService, int id)
        {
            var questionId = questionService.Duplicate(id);

            if (questionId == null)
                return new JsonResult(null) { StatusCode = 404 };

            return new JsonResult(new { questionId });
        }

        // Delete: api/question/delete/1
        [HttpDelete("delete/{id}")]
        [Authorize]
        public IActionResult Delete([FromServices] IQuestionService questionService, int id)
        {
            var result = questionService.Delete(id);

            if (result == 1)
                return new JsonResult(new { message = "Can't delete this question, it is used in other tests." }) { StatusCode = 409 };

            return new JsonResult(null);
        }

        // Get: api/question/used-in/1
        [HttpGet("used-in/{id}")]
        [Authorize]
        public IActionResult UsedIn([FromServices] IQuestionService questionService, int id)
        {
            var tests = questionService.UsedIn(id);

            return new JsonResult(tests, new JsonSerializerSettings() { 
                Converters = { new UsedInTestConverter() }
            });
        }

        // Get: api/question/1
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get([FromServices] IQuestionService questionService, int id)
        {
            var question = questionService.Get(id);

            if (question == null)
                return new JsonResult(null) { StatusCode = 404 };

            return new JsonResult(question);
        }
    }
}

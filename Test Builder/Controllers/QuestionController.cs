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
        public IActionResult GetTypes()
        {
            var questionTypes = dBHelper.QueryList2<QuestionType>(
                @"SELECT id AS Id, name AS Name, icon AS Icon, link AS Link 
                FROM question_type"
            );

            return new JsonResult(questionTypes);
        }

        // Get: api/question/search
        [HttpGet("search")]
        [Authorize]
        public IActionResult Search([FromQuery] DataParameters parameters)
        {
            var query =
                @"SELECT ISNULL(tq.id, 0) AS Id, ISNULL(tq.position, 0) AS Position,
                    q.id AS QuestionId, q.question AS Question, q.selection AS Selection,
                    qt.id AS TypeId, qt.name AS TypeName,
                    0 AS Random, NULL AS QuestionIds, NULL AS Number
                FROM question q
                INNER JOIN category sc ON sc.id = q.category_id 
                                AND (sc.customer_id = @customer_id OR sc.customer_id IS NULL) #subCategory
                INNER JOIN category c ON c.id = sc.parent_id 
                                AND (c.customer_id = @customer_id OR c.customer_id IS NULL) #category
                INNER JOIN question_type qt ON qt.id = q.type_id #questionType
                
                LEFT JOIN test_question tq ON tq.question_id = q.id AND tq.customer_id = @customer_id #status
                WHERE q.customer_id = @customer_id #searchTerm";

            var result = dBHelper.GetDataResult(query, parameters);

            return new JsonResult(result);
        }

        // Post: api/question/add/1
        [HttpPost("add/{pageId?}")]
        [Authorize]
        public IActionResult Add([FromServices] IQuestionService questionService, [FromBody] Question question, int? pageId)
        {
            if (ModelState.IsValid)
            {
                var questionId = questionService.Insert(question);

                if(pageId.HasValue)
                {
                    var testQuestion = new TestQuestion { PageId = pageId.Value, Random = false, QuestionId = questionId };

                    if(TryValidateModel(testQuestion))
                    {
                        var maxPosition = dBHelper.Query<int>(
                            @"SELECT ISNULL(MAX(position) + 1, 0) AS Nbr
                            FROM test_question
                            WHERE page_id = @page_id AND customer_id = @customer_id",
                            new Dictionary<string, object>() { { "page_id", pageId }, { "customer_id", User.Identity.Name } }
                        );

                        dBHelper.Write(@"
                            INSERT INTO test_question(page_id, position, random, question_id, 
                                category_id, subcategory_id, type_id, number, question_ids, customer_id)
                            OUTPUT INSERTED.id
                            VALUES(@page_id, @position, @random, @question_id, 
                                @category_id, @subcategory_id, @type_id, @number, @question_ids, @customer_id)",
                            new Dictionary<string, object> { { "page_id", testQuestion.PageId }, 
                                { "random", testQuestion.Random },
                                { "question_id", testQuestion.QuestionId },
                                { "category_id", DBNull.Value },
                                { "subcategory_id", DBNull.Value },
                                { "type_id", DBNull.Value },
                                { "number", DBNull.Value },
                                { "question_ids", DBNull.Value },
                                { "customer_id", User.Identity.Name }, { "position", maxPosition }
                            }
                        );
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
                return new JsonResult(new { }) { StatusCode = 404 };

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

            return new JsonResult(new { });
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
                return new JsonResult(new { }) { StatusCode = 404 };

            return new JsonResult(question);
        }
    }
}

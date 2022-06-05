using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Test_Builder.Services;
using Test_Builder.Models;

namespace Test_Builder.Controllers
{
    [ApiController]
    [Route("api/test")]
    [Produces("application/json")]
    public class TestController : ControllerBase
    {
        private readonly IDBHelper _DBHelper;

        public TestController(IDBHelper DBHelper)
        {
            _DBHelper = DBHelper;
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(int id)
        {
            var test = _DBHelper.Query2<Test>(
                @"SELECT id AS Id, name AS Name
                FROM test
                WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object> { { "id", id }, { "customer_id", User.Identity.Name } }
            );

            var pages = _DBHelper.QueryList2<Page>(
               @"SELECT id AS Id, name AS Name, limit AS Limit, position AS Position, 
                        shuffle AS Shuffle, test_id AS TestId
                FROM page
                WHERE test_id = @test_id AND customer_id = @customer_id
                ORDER BY position",
               new Dictionary<string, object> { { "test_id", id }, { "customer_id", User.Identity.Name } }
           );

            return new JsonResult(new { test, pages });
        }

        // Post: api/test/add
        [HttpPost("add")]
        [Authorize]
        public IActionResult Add([FromBody] Test test)
        {
            if (ModelState.IsValid)
            {
                var customer_id = User.Identity.Name;
                int pageId = 0;

                if(test.Id == 0)
                {
                    var id = _DBHelper.Write(
                        @"INSERT INTO test(name, introduction, category_id, customer_id)
                        OUTPUT INSERTED.id
                        VALUES(@name, @introduction, @category_id, @customer_id)",
                        new Dictionary<string, object>() { { "name", test.Name },
                            {"introduction", test.Introduction != null ? test.Introduction : DBNull.Value },
                            {"category_id", test.CategoryId },{ "customer_id", customer_id} }
                    );

                    test.Id = (int)id;

                    pageId = (int)_DBHelper.Write(
                        @"INSERT INTO page(name, position, test_id, customer_id)
                        OUTPUT INSERTED.id
                        VALUES(@name, @position, @test_id, @customer_id)",
                        new Dictionary<string, object>() { { "name", "Page 1" },
                            {"position", 1 }, {"test_id", test.Id},{ "customer_id", customer_id} }
                    );
                }

                return new JsonResult(new { result = 1, id = test.Id });
            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage)
            );

            return new JsonResult(errors) { StatusCode = 422 };
        }

        [HttpPost("add-question")]
        [Authorize]
        public IActionResult AddQuestion([FromBody] TestQuestion testQuestion)
        {
            if (ModelState.IsValid)
            {
                testQuestion.Id = (int)_DBHelper.Write(@"
                    INSERT INTO test_question(page_id, position, random, question_id, 
                        category_id, subcategory_id, type_id, number, question_ids, customer_id)
                    OUTPUT INSERTED.id
                    VALUES(@page_id, @position, @random, @question_id, 
                        @category_id, @subcategory_id, @type_id, @number, @question_ids, @customer_id)",
                    new Dictionary<string, object> { { "page_id", testQuestion.PageId }, { "random", testQuestion.Random }, 
                        { "question_id", testQuestion.QuestionId.HasValue ? testQuestion.QuestionId : DBNull.Value },
                        { "category_id", testQuestion.CategoryId.HasValue ? testQuestion.CategoryId : DBNull.Value }, 
                        { "subcategory_id", testQuestion.SubCategoryId.HasValue ? testQuestion.SubCategoryId : DBNull.Value }, 
                        { "type_id", testQuestion.TypeId.HasValue ? testQuestion.TypeId : DBNull.Value },
                        { "number", testQuestion.Number.HasValue ? testQuestion.Number : DBNull.Value }, 
                        { "question_ids", testQuestion.QuestionIds != null ? testQuestion.QuestionIds : DBNull.Value }, 
                        { "customer_id", User.Identity.Name }, { "position", testQuestion.Position }
                    }        
                );

                return new JsonResult(testQuestion);
            }

            var errors = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage)
            );

            return new JsonResult(errors) { StatusCode = 422 };
        }
    }
}

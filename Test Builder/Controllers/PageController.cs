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
        private readonly IDBHelper _DBHelper;

        public PageController(IDBHelper DBHelper)
        {
            _DBHelper = DBHelper;
        }

        // Get: api/page/get/5
        [HttpGet("get/{id}")]
        [Authorize]
        public IActionResult Get(int id)
        {
            long t = DateTime.Now.Ticks, delta;

            var page = _DBHelper.Query2<Page>(
               @"SELECT id AS Id, name AS Name, limit AS Limit, position AS Position, 
                        shuffle AS Shuffle, test_id AS TestId
                FROM page
                WHERE id = @page_id AND customer_id = @customer_id
                ORDER BY position",
               new Dictionary<string, object> { { "page_id", id }, { "customer_id", User.Identity.Name } }
            );
            delta = (DateTime.Now.Ticks - t) / TimeSpan.TicksPerMillisecond;

            t = DateTime.Now.Ticks;
            var questions = _DBHelper.QueryList2<TestQuestion>(
                @"SELECT tq.id AS Id, tq.position AS Position, tq.random AS Random, tq.question_id AS QuestionId,
                    tq.question_ids AS QuestionIds, tq.number AS Number,
                    q.question AS Question, q.selection AS Selection,
                    qt.id AS TypeId, qt.name AS TypeName
                FROM test_question tq
                LEFT JOIN question q ON q.id = tq.question_id AND q.customer_id = @customer_id
                LEFT JOIN question_type qt ON qt.id = q.type_id
                WHERE tq.page_id = @page_id AND tq.customer_id = @customer_id
                ORDER BY tq.question_id",
                new Dictionary<string, object> { { "page_id", id }, { "customer_id", User.Identity.Name } }
            );
            delta = (DateTime.Now.Ticks - t) / TimeSpan.TicksPerMillisecond;

            var paramDict = new Dictionary<string, object> { { "customer_id", User.Identity.Name } };
            var inList = new List<string>() { "0" };
            var index = 0;

            t = DateTime.Now.Ticks;
            foreach (var question in questions)
            {
                if (question.QuestionId.HasValue)
                {
                    paramDict.Add("question_" + index, question.QuestionId);
                    inList.Add("@question_" + index++);
                }
            }
            delta = (DateTime.Now.Ticks - t) / TimeSpan.TicksPerMillisecond;

            t = DateTime.Now.Ticks;
            var answers = _DBHelper.QueryList2<Answer>(
                $@"SELECT a.id AS Id, a.answer AS _Answer, a.correct AS Correct, a.match AS Match,
                    a.points AS Points, a.penalty AS Penalty, a.question_id AS QuestionId
                FROM answer a
                WHERE a.question_id IN ({string.Join(',', inList)}) AND a.customer_id = @customer_id
                ORDER BY a.question_id", paramDict
            );
            delta = (DateTime.Now.Ticks - t) / TimeSpan.TicksPerMillisecond;

            t = DateTime.Now.Ticks;
            index = 0;
            foreach (var question in questions)
            {
                while (index < answers.Count && answers[index].QuestionId == question.QuestionId)
                {
                    if(question.Answers == null)
                        question.Answers = new List<Answer>();
                    question.Answers.Add(answers[index++]);
                }
            }
            delta = (DateTime.Now.Ticks - t) / TimeSpan.TicksPerMillisecond;

            t = DateTime.Now.Ticks;
            questions.Sort((t1, t2) =>
            {
                return t1.Position < t2.Position ? -1 : 1;
            });
            delta = (DateTime.Now.Ticks - t) / TimeSpan.TicksPerMillisecond;

            return new JsonResult(new { page, questions });
        }

        // Post: api/page/add
        [HttpPost("add")]
        [Authorize]
        public IActionResult Add([FromBody] Page page)
        {
            if (ModelState.IsValid)
            {
                var customer_id = User.Identity.Name;

                if(page.Id == 0)
                {
                    page.Id = (int)_DBHelper.Write(
                        @"INSERT INTO page(name, limit, test_id, position, shuffle, customer_id)
                        OUTPUT INSERTED.id
                        VALUES(@name, @limit, @test_id, @position, @shuffle, @customer_id)",
                        new Dictionary<string, object>() { { "name", page.Name },
                            { "limit", page.Limit.HasValue ? page.Limit : DBNull.Value },
                            { "test_id", page.TestId },{ "position", page.Position },{ "shuffle", page.Shuffle },
                            { "customer_id", customer_id} }
                    );
                }
                else
                {
                    _DBHelper.Write(
                        @"UPDATE page
                        SET name = @name, limit = @limit, shuffle = @shuffle
                        WHERE id = @id AND customer_id = @customer_id AND test_id = @test_id",
                        new Dictionary<string, object>() { { "name", page.Name },
                            { "limit", page.Limit.HasValue ? page.Limit : DBNull.Value },
                            { "shuffle", page.Shuffle }, { "id", page.Id },
                            { "customer_id", customer_id },{ "test_id", page.TestId } }
                    );
                }

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
        public IActionResult Positions([FromBody] IEnumerable<Position> positions, [FromQuery] int testId)
        {
            foreach(var position in positions)
            {
                _DBHelper.Write(
                    @"UPDATE page 
                    SET position = @position 
                    WHERE id = @id AND customer_id = @customer_id AND test_id = @test_id",
                    new Dictionary<string, object> { { "position", position._Position}, { "id", position.Id },
                        { "customer_id", User.Identity.Name }, { "test_id", testId } }
                );
            }
            return new JsonResult(new { });
        }

        // Delete: api/page/delete/5
        [HttpDelete("delete/{id}")]
        [Authorize]
        public IActionResult Delete(int id, [FromQuery] int testId)
        {
            //Delete page related entities
            _DBHelper.Write(
                @"DELETE FROM test_question 
                WHERE page_id = @page_id AND customer_id = @customer_id",
                new Dictionary<string, object> { { "page_id", id }, { "customer_id", User.Identity.Name } }
            );

            //Delete page
            _DBHelper.Write(
                @"DELETE FROM page 
                WHERE id = @id AND customer_id = @customer_id AND test_id = @test_id",
                new Dictionary<string, object> { { "id", id },
                    { "customer_id", User.Identity.Name }, { "test_id", testId } }
            );

            return new JsonResult(new { });
        }
    }
}

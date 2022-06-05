using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Test_Builder.Services;
using Test_Builder.Models;

namespace Test_Builder.Controllers
{
    [ApiController]
    [Route("api/question")]
    [Produces("application/json")]
    public class QuestionController : ControllerBase
    {
        private readonly IDBHelper _DBHelper;

        public QuestionController(IDBHelper DBHelper)
        {
            _DBHelper = DBHelper;
        }

        // Get: api/question/types
        [HttpGet("types")]
        public IActionResult GetTypes()
        {
            var questionTypes = _DBHelper.QueryList2<QuestionType>(
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

            var result = _DBHelper.GetDataResult(query, parameters);

            return new JsonResult(result);
        }

        // Post: api/question/add/1
        [HttpPost("add/{pageId?}")]
        [Authorize]
        public IActionResult Add([FromBody] Question question, int? pageId)
        {
            if (ModelState.IsValid)
            {
                var questionId = (int)_DBHelper.Write(
                    @"INSERT INTO question(type_id, category_id, points, penalty, shuffle, selection, question, customer_id)
                    OUTPUT INSERTED.id
                    VALUES(@type_id, @category_id, @points, @penalty, @shuffle, @selection, @question, @customer_id)",
                    new Dictionary<string, object> { { "type_id", question.TypeId }, { "category_id", question.CategoryId },
                        { "points", question.Points},
                        { "penalty", question.Penalty.HasValue ? question.Penalty : DBNull.Value },
                        { "shuffle", question.Shuffle.HasValue ? question.Shuffle : DBNull.Value},
                        { "selection", question.Selection.HasValue ? question.Selection : DBNull.Value},
                        { "question", question._Question},
                        { "customer_id", User.Identity.Name} }
                );

                if(question.Answers != null)
                {
                    foreach(var answer in question.Answers)
                    {
                        _DBHelper.Write(
                            @"INSERT INTO answer(answer, match, question_id, points, penalty, correct, customer_id)
                            VALUES(@answer, @match, @question_id, @points, @penalty, @correct, @customer_id)",
                            new Dictionary<string, object> { { "answer", answer._Answer }, 
                                { "match", answer.Match != null ? answer.Match : DBNull.Value },
                                { "question_id", questionId },
                                { "points", answer.Points.HasValue ? answer.Points : DBNull.Value},
                                { "penalty", answer.Penalty.HasValue ? answer.Penalty : DBNull.Value },
                                { "correct", answer.Correct },
                                { "customer_id", User.Identity.Name} }
                        );
                    }
                }

                if(pageId.HasValue)
                {
                    var testQuestion = new TestQuestion { PageId = pageId.Value, Random = false, QuestionId = questionId };

                    if(TryValidateModel(testQuestion))
                    {
                        var maxPosition = _DBHelper.Query<int>(
                            @"SELECT ISNULL(MAX(position) + 1, 0) AS Nbr
                            FROM test_question
                            WHERE page_id = @page_id AND customer_id = @customer_id",
                            new Dictionary<string, object>() { { "page_id", pageId }, { "customer_id", User.Identity.Name } }
                        );

                        _DBHelper.Write(@"
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
        public IActionResult Edit([FromBody] Question question)
        {
            if (ModelState.IsValid)
            {
                _DBHelper.Write(
                    @"UPDATE question
                    SET type_id = @type_id, category_id = @category_id, points = @points, penalty = @penalty, 
                        shuffle = @shuffle, selection = @selection, question = @question
                    WHERE id = @id AND customer_id = @customer_id",
                    new Dictionary<string, object> { { "type_id", question.TypeId }, { "category_id", question.CategoryId },
                        { "points", question.Points},
                        { "penalty", question.Penalty.HasValue ? question.Penalty : DBNull.Value },
                        { "shuffle", question.Shuffle.HasValue ? question.Shuffle : DBNull.Value },
                        { "selection", question.Selection.HasValue ? question.Selection : DBNull.Value },
                        { "question", question._Question },
                        { "customer_id", User.Identity.Name }, { "id", question.Id } }
                );

                // deleting all answers and adding the new ones
                _DBHelper.Write(
                    @"DELETE FROM answer 
                    WHERE question_id = @question_id AND customer_id = @customer_id",
                    new Dictionary<string, object> { 
                        { "question_id", question.Id }, { "customer_id", User.Identity.Name } }
                );

                if (question.Answers != null)
                {
                    foreach (var answer in question.Answers)
                    {
                        _DBHelper.Write(
                            @"INSERT INTO answer(answer, match, question_id, points, penalty, correct, customer_id)
                            VALUES(@answer, @match, @question_id, @points, @penalty, @correct, @customer_id)",
                            new Dictionary<string, object> { { "answer", answer._Answer },
                                { "match", answer.Match != null ? answer.Match : DBNull.Value },
                                { "question_id", question.Id },
                                { "points", answer.Points.HasValue ? answer.Points : DBNull.Value},
                                { "penalty", answer.Penalty.HasValue ? answer.Penalty : DBNull.Value },
                                { "correct", answer.Correct },
                                { "customer_id", User.Identity.Name} }
                        );
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

        // Get: api/question/1
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var question = _DBHelper.Query2<Question>(
                @"SELECT type_id AS TypeId, category_id AS CategoryId, points AS Points, penalty AS Penalty, 
                    shuffle AS Shuffle, selection AS Selection, question AS _Question
                FROM question q
                WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object> { { "id", id }, { "customer_id", User.Identity.Name } }
            );

            if (question == null)
            {
                return new JsonResult(new { }) { StatusCode = 404 };
            }

            var answers = _DBHelper.QueryList2<Answer>(
                @"SELECT id AS Id, answer AS _Answer, match AS Match, points AS Points, penalty AS Penalty, correct AS Correct
                FROM answer a
                WHERE question_id = @question_id AND customer_id = @customer_id",
                new Dictionary<string, object> { { "question_id", id }, { "customer_id", User.Identity.Name } }
            );
            
            question.Answers = answers.ToArray();

            return new JsonResult(question);
        }

    }
}

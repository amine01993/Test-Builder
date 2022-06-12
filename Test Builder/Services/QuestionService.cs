using Test_Builder.Models;

namespace Test_Builder.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IDBHelper dBHelper;
        private readonly IHttpContextAccessor httpContextAccessor;

        public QuestionService(IDBHelper dBHelper, IHttpContextAccessor httpContextAccessor)
        {
            this.dBHelper = dBHelper;
            this.httpContextAccessor = httpContextAccessor;
        }
        public Question? Get(int id)
        {
            var question = dBHelper.Query2<Question>(
                @"SELECT type_id AS TypeId, category_id AS CategoryId, points AS Points, penalty AS Penalty, 
                    shuffle AS Shuffle, selection AS Selection, question AS _Question
                FROM question q
                WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object> 
                { { "id", id }, { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name } }
            );

            if (question == null)
                return null;

            var answers = dBHelper.QueryList2<Answer>(
                @"SELECT id AS Id, answer AS _Answer, match AS Match, points AS Points, penalty AS Penalty, 
                    correct AS Correct
                FROM answer a
                WHERE question_id = @question_id AND customer_id = @customer_id",
                new Dictionary<string, object> 
                { { "question_id", id }, { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name } }
            );

            question.Answers = answers.ToArray();

            return question;
        }
        public int Insert(Question question)
        {
            var questionId = (int) dBHelper.Write(
                @"INSERT INTO question(type_id, category_id, points, penalty, shuffle, selection, question, customer_id)
                OUTPUT INSERTED.id
                VALUES(@type_id, @category_id, @points, @penalty, @shuffle, @selection, @question, @customer_id)",
                new Dictionary<string, object> { { "type_id", question.TypeId }, { "category_id", question.CategoryId },
                    { "points", question.Points},
                    { "penalty", question.Penalty.HasValue ? question.Penalty : DBNull.Value },
                    { "shuffle", question.Shuffle.HasValue ? question.Shuffle : DBNull.Value},
                    { "selection", question.Selection.HasValue ? question.Selection : DBNull.Value},
                    { "question", question._Question},
                    { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name} }
            );

            if (question.Answers != null)
            {
                foreach (var answer in question.Answers)
                {
                    dBHelper.Write(
                        @"INSERT INTO answer(answer, match, question_id, points, penalty, correct, customer_id)
                            VALUES(@answer, @match, @question_id, @points, @penalty, @correct, @customer_id)",
                        new Dictionary<string, object> { { "answer", answer._Answer },
                            { "match", answer.Match != null ? answer.Match : DBNull.Value },
                            { "question_id", questionId },
                            { "points", answer.Points.HasValue ? answer.Points : DBNull.Value},
                            { "penalty", answer.Penalty.HasValue ? answer.Penalty : DBNull.Value },
                            { "correct", answer.Correct },
                            { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name} }
                    );
                }
            }

            return questionId;
        }
        public void Update(Question question)
        {
            dBHelper.Write(
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
                    { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name }, 
                    { "id", question.Id } }
            );

            // deleting all answers and adding the new ones
            dBHelper.Write(
                @"DELETE FROM answer 
                WHERE question_id = @question_id AND customer_id = @customer_id",
                new Dictionary<string, object> 
                { { "question_id", question.Id }, { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name } }
            );

            if (question.Answers != null)
            {
                foreach (var answer in question.Answers)
                {
                    dBHelper.Write(
                        @"INSERT INTO answer(answer, match, question_id, points, penalty, correct, customer_id)
                        VALUES(@answer, @match, @question_id, @points, @penalty, @correct, @customer_id)",
                        new Dictionary<string, object> { { "answer", answer._Answer },
                        { "match", answer.Match != null ? answer.Match : DBNull.Value },
                        { "question_id", question.Id },
                        { "points", answer.Points.HasValue ? answer.Points : DBNull.Value},
                        { "penalty", answer.Penalty.HasValue ? answer.Penalty : DBNull.Value },
                        { "correct", answer.Correct },
                        { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name} }
                    );
                }
            }

        }
        public int? Duplicate(int id)
        {
            var question = Get(id);

            if (question == null)
                return null;

            var questionId = Insert(question);

            return questionId;
        }
        public IEnumerable<Test> UsedIn(int id)
        {
            var tests = dBHelper.QueryList2<Test>(
                @"SELECT t.id AS Id, t.name AS Name
                FROM test_question tq
                INNER JOIN page p ON p.id = tq.page_id AND p.customer_id = @customer_id
                INNER JOIN test t ON t.id = p.test_id AND t.customer_id = @customer_id
                WHERE tq.question_id = @question_id AND tq.customer_id = @customer_id",
                new Dictionary<string, object>() 
                { { "question_id", id }, { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name } }
            );

            return tests;
        }
        public int Delete(int id)
        {
            var tests = UsedIn(id);

            if(tests.Count() == 0)
            {
                dBHelper.Write(
                    @"DELETE FROM answer 
                    WHERE question_id = @question_id AND customer_id = @customer_id",
                    new Dictionary<string, object>() 
                    { { "question_id", id }, { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name } }
                );

                dBHelper.Write(
                    @"DELETE FROM question 
                    WHERE id = @question_id AND customer_id = @customer_id",
                    new Dictionary<string, object>() 
                    { { "question_id", id }, { "customer_id", httpContextAccessor.HttpContext.User.Identity.Name } }
                );

                return 0;
            }

            return 1;
        }

    }
}

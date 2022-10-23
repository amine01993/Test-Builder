using Test_Builder.Models;
using Dapper;

namespace Test_Builder.Services
{
    public class PageService : IPageService
    {
        private readonly IDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly int customer_id;

        public PageService(IDBContext dBContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dBContext = dBContext;
            this.httpContextAccessor = httpContextAccessor;
            customer_id = int.Parse(httpContextAccessor.HttpContext.User.Identity.Name);
        }

        public Page Get(int id)
        {
            var page = dBContext.Get<Page>(
               @"SELECT id AS Id, name AS Name, limit AS Limit, position AS Position, 
                        shuffle AS Shuffle, test_id AS TestId
                FROM page
                WHERE id = @page_id AND customer_id = @customer_id
                ORDER BY position",
               new Dictionary<string, object> { { "page_id", id }, { "customer_id", customer_id } }
            );
            if (page == null) return null;

            List<PageQuestion> pageQuestions;
            using(var connection = dBContext.Connection())
            {
                pageQuestions = connection.Query<PageQuestion, Question, QuestionType, PageQuestion>(
                    @"SELECT pq.id AS Id, pq.position AS Position, pq.random AS Random, pq.question_id AS QuestionId,
                        pq.question_ids AS QuestionIds, pq.number AS Number,
                        q.id AS Id, q.question AS _Question, q.selection AS Selection,
                        qt.id AS Id, qt.name AS Name
                    FROM page_question pq
                    INNER JOIN question q ON q.id = pq.question_id AND q.customer_id = @customer_id
                    INNER JOIN question_type qt ON qt.id = q.type_id
                    WHERE pq.page_id = @page_id AND pq.customer_id = @customer_id
                    ORDER BY pq.question_id",
                    (pageQuestion, question, questionType) =>
                    {
                        question.QuestionType = questionType;
                        pageQuestion.Question = question;
                        return pageQuestion;
                    },
                    new Dictionary<string, object> { { "page_id", id }, { "customer_id", customer_id } }
                ).ToList();
            }
            page.PageQuestions = pageQuestions;

            var paramDict = new Dictionary<string, object> { { "customer_id", customer_id } };
            var inList = new List<string>() { "0" };
            var index = 0;

            foreach (var pageQuestion in pageQuestions)
            {
                if (pageQuestion.QuestionId.HasValue)
                {
                    paramDict.Add("question_" + index, pageQuestion.QuestionId);
                    inList.Add("@question_" + index++);
                }
            }

            var answers = dBContext.List<Answer>(
                $@"SELECT a.id AS Id, a.answer AS _Answer, a.correct AS Correct, a.match AS Match,
                    a.points AS Points, a.penalty AS Penalty, a.question_id AS QuestionId
                FROM answer a
                WHERE a.question_id IN ({string.Join(',', inList)}) AND a.customer_id = @customer_id
                ORDER BY a.question_id", paramDict
            );

            index = 0;
            foreach (var pageQuestion in pageQuestions)
            {
                while (index < answers.Count() && answers.ElementAt(index).QuestionId == pageQuestion.QuestionId)
                {
                    if (pageQuestion.Question?.Answers == null)
                        pageQuestion.Question.Answers = new List<Answer>();
                    pageQuestion.Question.Answers.Add(answers.ElementAt(index++));
                }
            }

            pageQuestions.Sort((t1, t2) =>
            {
                return t1.Position < t2.Position ? -1 : 1;
            });

            return page;
        }

        public void Insert(Page page)
        {
            page.Id = (int)dBContext.Write(
                @"INSERT INTO page(name, limit, test_id, position, shuffle, customer_id)
                OUTPUT INSERTED.id
                VALUES(@name, @limit, @test_id, @position, @shuffle, @customer_id)",
                new Dictionary<string, object>() { { "name", page.Name },
                    { "limit", page.Limit.HasValue ? page.Limit : DBNull.Value },
                    { "test_id", page.TestId },{ "position", page.Position },{ "shuffle", page.Shuffle },
                    { "customer_id", customer_id} }
            );
        }

        public void Update(Page page)
        {
            dBContext.Write(
                @"UPDATE page
                SET name = @name, limit = @limit, shuffle = @shuffle
                WHERE id = @id AND customer_id = @customer_id AND test_id = @test_id",
                new Dictionary<string, object>() { { "name", page.Name },
                    { "limit", page.Limit.HasValue ? page.Limit : DBNull.Value },
                    { "shuffle", page.Shuffle }, { "id", page.Id },
                    { "customer_id", customer_id },{ "test_id", page.TestId } }
            );
        }

        public void InsertOrUpdate(Page page)
        {
            if (page.Id == 0)
            {
                Insert(page);
            }
            else
            {
                Update(page);
            }
        }
        
        public void Delete(int id, int testId)
        {
            //Delete page questions
            dBContext.Write(
                @"DELETE FROM page_question 
                WHERE page_id = @page_id AND customer_id = @customer_id",
                new Dictionary<string, object> { { "page_id", id }, { "customer_id", customer_id } }
            );

            //Delete page
            dBContext.Write(
                @"DELETE FROM page 
                WHERE id = @id AND customer_id = @customer_id AND test_id = @test_id",
                new Dictionary<string, object> { { "id", id },
                    { "customer_id", customer_id }, { "test_id", testId } }
            );
        }

        public void SetPositions(IEnumerable<Position> positions, int testId)
        {
            foreach (var position in positions)
            {
                dBContext.Write(
                    @"UPDATE page 
                    SET position = @position 
                    WHERE id = @id AND customer_id = @customer_id AND test_id = @test_id",
                    new Dictionary<string, object> { { "position", position._Position}, { "id", position.Id },
                        { "customer_id", customer_id }, { "test_id", testId } }
                );
            }
        }
    }
}

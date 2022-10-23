using Test_Builder.Models;
using Dapper;

namespace Test_Builder.Services
{
    public class PreviewService: IPreviewService
    {
        private readonly IDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly int customer_id;

        public PreviewService(IDBContext dBContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dBContext = dBContext;
            this.httpContextAccessor = httpContextAccessor;
            customer_id = int.Parse(httpContextAccessor.HttpContext.User.Identity.Name);
        }

        public Test Get(int id)
        {
            var test = dBContext.Get<Test>(
                @"SELECT id AS Id, name AS Name
                FROM test
                WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object> { { "id", id }, { "customer_id", customer_id } }
            );
            if (test == null)
                return null;

            test.Pages = dBContext.List<Page>(
               @"SELECT id AS Id, name AS Name, limit AS Limit, position AS Position, 
                        shuffle AS Shuffle, test_id AS TestId
                FROM page
                WHERE test_id = @test_id AND customer_id = @customer_id
                ORDER BY id",
               new Dictionary<string, object> { { "test_id", id }, { "customer_id", customer_id } }
            );
            // ORDER BY position

            var pageQuestionDict = new Dictionary<string, object>
                { { "customer_id", customer_id } };
            var pagesIdsParams = new List<string>();
            foreach(var page in test.Pages)
            {
                pagesIdsParams.Add("page_" + page.Id);
                pageQuestionDict.Add("page_" + page.Id, page.Id);
            }

            IEnumerable<PageQuestion> pageQuestions;
            using(var connection = dBContext.Connection())
            {
                pageQuestions = connection.Query<PageQuestion, Question, QuestionType, PageQuestion>(
                    $@"SELECT pq.id AS Id, pq.position AS Position, pq.random AS Random, pq.question_id AS QuestionId,
                        pq.question_ids AS QuestionIds, pq.number AS Number, pq.page_id AS PageId,
                        q.question AS _Question, q.selection AS Selection,
                        qt.id AS QuestionType.Id, qt.name AS Name
                    FROM page_question pq
                    INNER JOIN question q ON q.id = pq.question_id AND q.customer_id = @customer_id
                    INNER JOIN question_type qt ON qt.id = q.type_id
                    WHERE pq.page_id IN ({string.Join(',', pagesIdsParams)}) AND pq.customer_id = @customer_id
                    ORDER BY pq.question_id",
                    (pageQuestion, question, questionType) => {
                        question.QuestionType = questionType;
                        pageQuestion.Question = question;
                        return pageQuestion;
                    },
                    pageQuestionDict
                );
            }

            var answerDict = new Dictionary<string, object> { { "customer_id", customer_id } };
            var questionsIdsParams = new List<string>();
            foreach (var pageQuestion in pageQuestions)
            {
                questionsIdsParams.Add("question_" + pageQuestion.QuestionId);
                answerDict.Add("question_" + pageQuestion.QuestionId, pageQuestion.QuestionId);
            }
            var answers = dBContext.List<Answer>(
                $@"SELECT id AS Id, answer AS _Answer, match AS Match, points AS Points, penalty AS Penalty, 
                    correct AS Correct
                FROM answer a
                WHERE question_id IN ({string.Join(',', questionsIdsParams)}) AND customer_id = @customer_id
                ORDER BY question_id",
                answerDict
            );

            AttachAnswersToPageQuestions(answers, pageQuestions);

            pageQuestions.OrderBy(pq => pq.PageId);

            AttachPageQuestionsToPages(pageQuestions, test.Pages);

            return test;
        }

        /**
         * @param IEnumerable<Answer> answers - Ordered by QuestionId
         * @param IEnumerable<Answer> pageQuestions - Ordered by QuestionId
         */
        private void AttachAnswersToPageQuestions(IEnumerable<Answer> answers, IEnumerable<PageQuestion> pageQuestions)
        {
            var ai = 0;
            foreach(var pageQuestion in pageQuestions)
            {
                while(ai < answers.Count() && pageQuestion.QuestionId == answers.ElementAt(ai).QuestionId)
                {
                    if (pageQuestion.Question.Answers == null)
                        pageQuestion.Question.Answers = new List<Answer> { };
                    pageQuestion.Question.Answers.Add(answers.ElementAt(ai++));
                }
            }
        }

        /**
         * @param IEnumerable<Answer> pageQuestions - Ordered by PageId
         * @param IEnumerable<Answer> pages - Ordered by PageId
         */
        private void AttachPageQuestionsToPages(IEnumerable<PageQuestion> pageQuestions, IEnumerable<Page> pages)
        {
            var pqi = 0;
            foreach (var page in pages)
            {
                while (pqi < pageQuestions.Count() && page.Id == pageQuestions.ElementAt(pqi).PageId)
                {
                    if (page.PageQuestions == null)
                        page.PageQuestions = new List<PageQuestion> { };
                    page.PageQuestions.Add(pageQuestions.ElementAt(pqi++));
                }
                if (page.PageQuestions != null)
                    page.PageQuestions.OrderBy(pq => pq.Position);
            }
            pages.OrderBy(p => p.Position);
        }
    }
}

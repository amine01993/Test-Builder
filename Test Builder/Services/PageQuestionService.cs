﻿using Test_Builder.Models;

namespace Test_Builder.Services
{
    public class PageQuestionService : IPageQuestionService
    {
        private readonly IDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly int customer_id;

        public PageQuestionService(IDBContext dBContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dBContext = dBContext;
            this.httpContextAccessor = httpContextAccessor;
            customer_id = int.Parse(httpContextAccessor.HttpContext.User.Identity.Name);
        }

        public IEnumerable<int> GetUsedQuestions(IEnumerable<int> questionIds, int pageId)
        {
            var testId = dBContext.GetScalar<int>(
                "SELECT test_id FROM page WHERE id = @page_id AND customer_id = @customer_id",
                new Dictionary<string, object> { { "page_id", pageId }, { "customer_id", customer_id } }
            );
            // list all questions in questionIds that are in the test
            var parameters = new Dictionary<string, object> { { "test_id", testId }, { "customer_id", customer_id } };
            var parmertersList = new List<string>();
            foreach (var questionId in questionIds)
            {
                parameters.Add("question_" + questionId, questionId);
                parmertersList.Add("@question_" + questionId);
            }
            var existantQuestionIds = dBContext.List<int>(
                $@"SELECT pq.question_id
                FROM page_question pq
                INNER JOIN page p on p.id = pq.page_id AND p.test_id = @test_id AND p.customer_id = @customer_id
                WHERE pq.question_id in ({string.Join(',', parmertersList)}) AND pq.customer_id = @customer_id",
                parameters
            );

            return existantQuestionIds;
        }

        public int Insert(PageQuestion pageQuestion)
        {
            var id = (int)dBContext.Write(@"
                INSERT INTO page_question(page_id, position, random, question_id, 
                    category_id, subcategory_id, type_id, number, question_ids, customer_id)
                OUTPUT INSERTED.id
                VALUES(@page_id, @position, @random, @question_id, 
                    @category_id, @subcategory_id, @type_id, @number, @question_ids, @customer_id)",
                new Dictionary<string, object> { { "page_id", pageQuestion.PageId },
                    { "random", pageQuestion.Random },
                    { "question_id", pageQuestion.QuestionId },
                    { "category_id", DBNull.Value },
                    { "subcategory_id", DBNull.Value },
                    { "type_id", DBNull.Value },
                    { "number", DBNull.Value },
                    { "question_ids", DBNull.Value },
                    { "customer_id", customer_id }, { "position", pageQuestion.Position }
                }
            );

            return id;
        }

        public int MaxPosition(int pageId)
        {
            var maxPosition = dBContext.GetScalar<int>(
                @"SELECT ISNULL(MAX(position) + 1, 0)
                FROM page_question
                WHERE page_id = @page_id AND customer_id = @customer_id",
                new Dictionary<string, object>() { { "page_id", pageId }, { "customer_id", customer_id } }
            );

            return maxPosition;
        }

        public void SetPositions(IEnumerable<Position> positions)
        {
            foreach (var position in positions)
            {
                dBContext.Write(
                    @"UPDATE page_question
                    SET position = @position
                    WHERE id = @id AND customer_id = @customer_id",
                    new Dictionary<string, object> { { "position", position._Position },
                        { "id", position.Id }, { "customer_id", customer_id }}
                );
            }
        }

        public void Delete(int id)
        {
            dBContext.Write(
                @"DELETE FROM page_question 
                WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object>()
                { {"id", id}, {"customer_id", customer_id} }
            );
        }
    }
}

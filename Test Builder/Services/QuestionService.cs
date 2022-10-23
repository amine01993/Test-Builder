using Test_Builder.Models;
using Dapper;

namespace Test_Builder.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly int customer_id;

        public QuestionService(IDBContext dBContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dBContext = dBContext;
            this.httpContextAccessor = httpContextAccessor;
            customer_id = int.Parse(httpContextAccessor.HttpContext.User.Identity.Name);
        }

        public Question? Get(int id)
        {
            var question = dBContext.Get<Question>(
                @"SELECT type_id AS TypeId, category_id AS CategoryId, points AS Points, penalty AS Penalty, 
                    shuffle AS Shuffle, selection AS Selection, question AS _Question
                FROM question q
                WHERE id = @id AND customer_id = @customer_id",
                 new Dictionary<string, object>
                { { "id", id }, { "customer_id", customer_id } }
            );

            if (question == null)
                return null;

            var answers = dBContext.List<Answer>(
                @"SELECT id AS Id, answer AS _Answer, match AS Match, points AS Points, penalty AS Penalty, 
                    correct AS Correct
                FROM answer a
                WHERE question_id = @question_id AND customer_id = @customer_id",
                new Dictionary<string, object>
                { { "question_id", id }, { "customer_id", customer_id } }
            );

            question.Answers = answers.ToList();

            return question;
        }

        public IEnumerable<Question> Get(IEnumerable<int> ids)
        {
            var questionDict = new Dictionary<string, object>() { { "customer_id", customer_id} };
            var questionIdsParam = new List<string>();
            foreach (var id in ids)
            {
                questionDict["id_" + id] = id;
                questionIdsParam.Add("@id_" + id);
            }

            IEnumerable<Question> questions;
            using(var connection = dBContext.Connection())
            {
                questions = connection.Query<Question, Category, Category, Question>(
                    $@"SELECT q.id AS Id, q.type_id AS TypeId, q.shuffle AS Shuffle, q.selection AS Selection, 
                        q.question AS _Question, q.points AS Points, q.penalty AS Penalty,
                        c.id AS Id, c.name AS Name, c.parent_id AS ParentId,
                        cp.id AS Id, cp.name AS Name
                    FROM question q
                    INNER JOIN category c ON c.id = q.category_id AND (c.customer_id = @customer_id OR c.customer_id IS NULL)
                    LEFT JOIN category cp ON cp.id = c.parent_id AND (cp.customer_id = @customer_id OR cp.customer_id IS NULL)
                    WHERE q.id IN ({string.Join(',', questionIdsParam)}) AND q.customer_id = @customer_id
                    ORDER BY q.id",
                    (question, category, parent) =>
                    {
                        category.Parent = parent;
                        question.Category = category;
                        return question;
                    },
                    //splitOn: "Category.Id, Category.Parent.Id",
                    param: questionDict
                );
            }

            var answers = dBContext.List<Answer>(
                $@"SELECT a.id AS Id, a.answer AS _Answer, a.correct AS Correct, a.match AS Match,
                    a.points AS Points, a.penalty AS Penalty, a.question_id AS QuestionId
                FROM answer a
                WHERE a.question_id IN ({string.Join(',', questionIdsParam)}) AND a.customer_id = @customer_id
                ORDER BY a.question_id", questionDict
            );

            var index = 0;
            foreach (var question in questions)
            {
                while (index < answers.Count() && answers.ElementAt(index).QuestionId == question.Id)
                {
                    if (question.Answers == null)
                        question.Answers = new List<Answer>();
                    question.Answers.Add(answers.ElementAt(index++));
                }
            }

            return questions;
        }

        public DataResult<Question> Search(DataParameters parameters)
        {
            var sql =
                @"SELECT q.id AS Id, q.question AS _Question, q.selection AS Selection,
                    q.type_id AS TypeId

                FROM question q
                INNER JOIN category sc ON sc.id = q.category_id 
                                AND (sc.customer_id = @customer_id OR sc.customer_id IS NULL) #subCategory
                INNER JOIN category c ON c.id = sc.parent_id 
                                AND (c.customer_id = @customer_id OR c.customer_id IS NULL) #category
                INNER JOIN question_type qt ON qt.id = q.type_id #questionType
                
                LEFT JOIN page_question pq ON pq.question_id = q.id AND pq.customer_id = @customer_id #status
                WHERE q.customer_id = @customer_id #searchTerm";


            var sqlParameters = new Dictionary<string, object>()
            { { "customer_id", customer_id } };

            IDictionary<string, string> filter = parameters.decodeFilter();
            //IDictionary<string, string> orderBy = parameters.decodeParam(parameters._orderBy);

            foreach (var item in filter)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    //string[] values;
                    //IList<string> paramList;
                    switch (item.Key)
                    {
                        case "status":
                            if (item.Value == "0")
                            {
                                sql = sql.Replace("#status", "");
                            }
                            else if (item.Value == "1")
                            {
                                sql = sql.Replace("#status", "AND pq.id IS NOT NULL");
                            }
                            else if (item.Value == "2")
                            {
                                sql = sql.Replace("#status", "AND pq.id IS NULL");
                            }
                            break;
                        case "type":
                            if (item.Value == "0")
                                sql = sql.Replace("#questionType", "");
                            else
                            {
                                sqlParameters.Add("type", item.Value);
                                sql = sql.Replace("#questionType", "AND qt.id = @type");
                            }
                            break;
                        case "category":
                            if (item.Value == "0")
                                sql = sql.Replace("#category", "");
                            else
                            {
                                sqlParameters.Add("category", item.Value);
                                sql = sql.Replace("#category", "AND c.id = @category");
                            }
                            break;
                        case "subCategory":
                            if (item.Value == "0")
                                sql = sql.Replace("#subCategory", "");
                            else
                            {
                                sqlParameters.Add("subCategory", item.Value);
                                sql = sql.Replace("#subCategory", "AND sc.id = @subCategory");
                            }
                            break;
                        case "term":
                            sqlParameters.Add("term", "%" + item.Value + "%");
                            sql = sql.Replace("#searchTerm", "AND q.question LIKE @term");
                            break;
                        default:
                            break;
                    }
                }
            }

            sql = sql.Replace("#status", "").Replace("#questionType", "").Replace("#category", "")
                .Replace("#subCategory", "").Replace("#searchTerm", "");

            //var orderList = new List<string>();
            //foreach(var item in orderBy)
            //{
            //    orderList.Add($"{item.Key} {item.Value}");
            //}
            //var orderStr = orderList.Count == 0 ? "" : "ORDER BY " + string.Join(", ", orderList);

            var totalQuery = $@"SELECT COUNT(*) AS Value FROM ({sql}) as t";
            var total = dBContext.GetScalar<int>(totalQuery, sqlParameters);

            var query = $@"{sql} 
                ORDER BY Id
                OFFSET {(parameters.page - 1) * parameters.pageSize} ROWS 
                FETCH NEXT {parameters.pageSize} ROWS ONLY";
            // questions results
            var list = dBContext.List<Question>(query, sqlParameters); 

            var paramDict = new Dictionary<string, object>
                { { "customer_id", customer_id } };
            var inList = new List<string>() { "0" };
            var index = 0;

            foreach (var question in list)
            {
                paramDict.Add("question_" + index, question.Id);
                inList.Add("@question_" + index++);
            }

            var answers = dBContext.List<Answer>(
                $@"SELECT a.id AS Id, a.answer AS _Answer, a.correct AS Correct, a.match AS Match,
                    a.points AS Points, a.penalty AS Penalty, a.question_id AS QuestionId
                FROM answer a
                WHERE a.question_id IN ({string.Join(',', inList)}) AND a.customer_id = @customer_id
                ORDER BY a.question_id", paramDict
            );

            index = 0;
            foreach (var question in list)
            {
                while (index < answers.Count() && answers.ElementAt(index).QuestionId == question.Id)
                {
                    if (question.Answers == null)
                        question.Answers = new List<Answer>();
                    question.Answers.Add(answers.ElementAt(index++));
                }
            }

            var result = new DataResult<Question>() { Total = total, Count = list.Count(), Data = list };

            return result;
        }

        public int Insert(Question question)
        {
            var questionId = (int) dBContext.Write(
                @"INSERT INTO question(type_id, category_id, points, penalty, shuffle, selection, question, customer_id)
                OUTPUT INSERTED.id
                VALUES(@type_id, @category_id, @points, @penalty, @shuffle, @selection, @question, @customer_id)",
                new Dictionary<string, object> { { "type_id", question.TypeId }, { "category_id", question.CategoryId },
                    { "points", question.Points},
                    { "penalty", question.Penalty.HasValue ? question.Penalty : DBNull.Value },
                    { "shuffle", question.Shuffle.HasValue ? question.Shuffle : DBNull.Value},
                    { "selection", question.Selection.HasValue ? question.Selection : DBNull.Value},
                    { "question", question._Question},
                    { "customer_id", customer_id} }
            );

            if (question.Answers != null)
            {
                foreach (var answer in question.Answers)
                {
                    dBContext.Write(
                        @"INSERT INTO answer(answer, match, question_id, points, penalty, correct, customer_id)
                            VALUES(@answer, @match, @question_id, @points, @penalty, @correct, @customer_id)",
                        new Dictionary<string, object> { { "answer", answer._Answer },
                            { "match", answer.Match != null ? answer.Match : DBNull.Value },
                            { "question_id", questionId },
                            { "points", answer.Points.HasValue ? answer.Points : DBNull.Value},
                            { "penalty", answer.Penalty.HasValue ? answer.Penalty : DBNull.Value },
                            { "correct", answer.Correct },
                            { "customer_id", customer_id} }
                    );
                }
            }

            return questionId;
        }
        
        public void Update(Question question)
        {
            dBContext.Write(
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
                    { "customer_id", customer_id }, 
                    { "id", question.Id } }
            );

            // deleting all answers and adding the new ones
            dBContext.Write(
                @"DELETE FROM answer 
                WHERE question_id = @question_id AND customer_id = @customer_id",
                new Dictionary<string, object> 
                { { "question_id", question.Id }, { "customer_id", customer_id } }
            );

            if (question.Answers != null)
            {
                foreach (var answer in question.Answers)
                {
                    dBContext.Write(
                        @"INSERT INTO answer(answer, match, question_id, points, penalty, correct, customer_id)
                        VALUES(@answer, @match, @question_id, @points, @penalty, @correct, @customer_id)",
                        new Dictionary<string, object> { { "answer", answer._Answer },
                        { "match", answer.Match != null ? answer.Match : DBNull.Value },
                        { "question_id", question.Id },
                        { "points", answer.Points.HasValue ? answer.Points : DBNull.Value},
                        { "penalty", answer.Penalty.HasValue ? answer.Penalty : DBNull.Value },
                        { "correct", answer.Correct },
                        { "customer_id", customer_id } }
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
            var tests = dBContext.List<Test>(
                @"SELECT t.id AS Id, t.name AS Name
                FROM page_question pq
                INNER JOIN page p ON p.id = pq.page_id AND p.customer_id = @customer_id
                INNER JOIN test t ON t.id = p.test_id AND t.customer_id = @customer_id
                WHERE pq.question_id = @question_id AND pq.customer_id = @customer_id",
                new Dictionary<string, object>() 
                { { "question_id", id }, { "customer_id", customer_id } }
            );

            return tests;
        }
        
        public int Delete(int id)
        {
            var tests = UsedIn(id);

            if(tests.Count() == 0)
            {
                dBContext.Write(
                    @"DELETE FROM answer 
                    WHERE question_id = @question_id AND customer_id = @customer_id",
                    new Dictionary<string, object>() 
                    { { "question_id", id }, { "customer_id", customer_id } }
                );

                dBContext.Write(
                    @"DELETE FROM question 
                    WHERE id = @question_id AND customer_id = @customer_id",
                    new Dictionary<string, object>() 
                    { { "question_id", id }, { "customer_id", customer_id } }
                );

                return 0;
            }

            return 1;
        }

    }
}

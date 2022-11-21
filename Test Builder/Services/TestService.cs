using Dapper;
using Test_Builder.Models;

namespace Test_Builder.Services
{
    public class TestService: ITestService
    {
        private readonly IDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly int customer_id;

        public TestService(IDBContext dBContext, IHttpContextAccessor httpContextAccessor)
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
                ORDER BY position",
               new Dictionary<string, object> { { "test_id", id }, { "customer_id", customer_id } }
            );

            return test;
        }

        public IEnumerable<Test> List()
        {
            IEnumerable<Test> tests;
            using(var connection = dBContext.Connection())
            {
                tests = connection.Query<Test, Category, Category, Test>(
                    @"SELECT t.id AS Id, t.name AS Name, t.created_at AS CreatedAt,
                        c.id AS Id, c.name AS Name,
                        sc.id AS Id, sc.name AS Name
                    FROM test t
                    INNER JOIN category sc ON sc.id = t.category_id AND (sc.customer_id IS NULL OR sc.customer_id = @customer_id)
                    INNER JOIN category c ON c.id = sc.parent_id AND (c.customer_id IS NULL OR c.customer_id = @customer_id)
                    WHERE t.customer_id = @customer_id",
                    (test, category, subCategory) =>
                    {
                        subCategory.Parent = category;
                        test.Category = subCategory;
                        return test;
                    },
                    new Dictionary<string, object> { { "customer_id", customer_id } }
                );
            }
            return tests;
        }

        public int Add(Test test)
        {
            var id = dBContext.Write(
                @"INSERT INTO test(name, introduction, category_id, customer_id)
                OUTPUT INSERTED.id
                VALUES(@name, @introduction, @category_id, @customer_id)",
                new Dictionary<string, object>() { { "name", test.Name },
                    {"introduction", test.Introduction != null ? test.Introduction : DBNull.Value },
                    {"category_id", test.CategoryId },{ "customer_id", customer_id} }
            );

            test.Id = (int)id;

            dBContext.Write(
                @"INSERT INTO page(name, position, test_id, customer_id)
                OUTPUT INSERTED.id
                VALUES(@name, @position, @test_id, @customer_id)",
                new Dictionary<string, object>() { { "name", "Page 1" },
                    {"position", 1 }, {"test_id", test.Id},{ "customer_id", customer_id} }
            );

            return test.Id;
        }
    }
}

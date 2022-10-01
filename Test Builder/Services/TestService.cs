using Test_Builder.Models;

namespace Test_Builder.Services
{
    public class TestService: ITestService
    {
        private readonly IDBHelper dBHelper;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly int customer_id;

        public TestService(IDBHelper dBHelper, IHttpContextAccessor httpContextAccessor)
        {
            this.dBHelper = dBHelper;
            this.httpContextAccessor = httpContextAccessor;
            customer_id = int.Parse(httpContextAccessor.HttpContext.User.Identity.Name);
        }

        public Test Get(int id)
        {
            var test = dBHelper.Query2<Test>(
                @"SELECT id AS Id, name AS Name
                FROM test
                WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object> { { "id", id }, { "customer_id", customer_id } }
            );
            if (test == null)
                return null;

            test.Pages = dBHelper.QueryList2<Page>(
               @"SELECT id AS Id, name AS Name, limit AS Limit, position AS Position, 
                        shuffle AS Shuffle, test_id AS TestId
                FROM page
                WHERE test_id = @test_id AND customer_id = @customer_id
                ORDER BY position",
               new Dictionary<string, object> { { "test_id", id }, { "customer_id", customer_id } }
            );

            return test;
        }

        public int Add(Test test)
        {
            var id = dBHelper.Write(
                @"INSERT INTO test(name, introduction, category_id, customer_id)
                OUTPUT INSERTED.id
                VALUES(@name, @introduction, @category_id, @customer_id)",
                new Dictionary<string, object>() { { "name", test.Name },
                    {"introduction", test.Introduction != null ? test.Introduction : DBNull.Value },
                    {"category_id", test.CategoryId },{ "customer_id", customer_id} }
            );

            test.Id = (int)id;

            dBHelper.Write(
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

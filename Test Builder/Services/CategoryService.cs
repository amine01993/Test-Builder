using Test_Builder.Models;

namespace Test_Builder.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IDBContext dBContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly int customer_id;

        public CategoryService(IDBContext dBContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dBContext = dBContext;
            this.httpContextAccessor = httpContextAccessor;
            customer_id = int.Parse(httpContextAccessor.HttpContext.User.Identity.Name);
        }

        public IEnumerable<Category> Get()
        {
            var categories = dBContext.List<Category>(
               @"SELECT id AS Id, name AS Name, parent_id AS ParentId 
               FROM category
               WHERE customer_id = @customer_id OR customer_id IS NULL",
               new Dictionary<string, object>()
               { {"customer_id", customer_id} }
            );
            return categories;
        }
        
        public void Insert(Category category)
        {
            dBContext.Write(
                @"INSERT INTO category(name, parent_id, customer_id) 
                VALUES (@name, @parent_id, @customer_id)",
                new Dictionary<string, object>()
                { {"name", category.Name},
                    {"parent_id", category.ParentId.HasValue ? category.ParentId : DBNull.Value},
                    {"customer_id", customer_id} }
            );
        }

        public void Update(Category category)
        {
            dBContext.Write(
                @"UPDATE category SET name = @name, parent_id = @parent_id
                WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object>()
                { {"name", category.Name},
                    {"parent_id", category.ParentId.HasValue ? category.ParentId : DBNull.Value},
                    {"id", category.Id}, {"customer_id", customer_id} }
            );
        }

        public void InsertOrUpdate(Category category)
        {
            if (category.Id == 0)
            {
                Insert(category);
            }
            else
            {
                Update(category);
            }
        }

        public void Delete(int id, bool hasSubCategories)
        {
            if (hasSubCategories)
            {
                dBContext.Write(
                    @"DELETE FROM category WHERE parent_id = @id AND customer_id = @customer_id",
                    new Dictionary<string, object>()
                    { {"id", id}, {"customer_id", customer_id} }
                );
            }

            dBContext.Write(
                @"DELETE FROM category WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object>()
                { {"id", id}, {"customer_id", customer_id} }
            );
        }

    }
}

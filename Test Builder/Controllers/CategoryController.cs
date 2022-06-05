using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Test_Builder.Services;
using Test_Builder.Models;

namespace Test_Builder.Controllers
{
    [ApiController]
    [Route("api/category")]
    [Produces("application/json")]
    public class CategoryController : ControllerBase
    {
        private readonly IDBHelper _DBHelper;

        public CategoryController(IDBHelper DBHelper)
        {
            _DBHelper = DBHelper;
        }

        // Get: api/category
        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            var categories = _DBHelper.QueryList2<Category>(
                @"SELECT id AS Id, name AS Name, parent_id AS ParentId 
                FROM category
                WHERE customer_id = @customer_id OR customer_id IS NULL",
                new Dictionary<string, object>()
                { {"customer_id", User.Identity.Name} }
            );

            var result = new JsonResult(categories);
            
            return result;
        }

        // Post: api/category/add
        [HttpPost("add")]
        [Authorize]
        public IActionResult Add([FromBody] Category category)
        {
            if (ModelState.IsValid)
            {
                if(category.Id == 0)
                {
                    _DBHelper.Write(
                        @"INSERT INTO category(name, parent_id, customer_id) 
                        VALUES (@name, @parent_id, @customer_id)",
                        new Dictionary<string, object>()
                        { {"name", category.Name}, 
                            {"parent_id", category.ParentId.HasValue ? category.ParentId : DBNull.Value}, 
                            {"customer_id", User.Identity.Name} }
                    );
                }
                else
                {
                    _DBHelper.Write(
                        @"UPDATE category SET name = @name, parent_id = @parent_id
                        WHERE id = @id AND customer_id = @customer_id",
                        new Dictionary<string, object>()
                        { {"name", category.Name}, 
                            {"parent_id", category.ParentId.HasValue ? category.ParentId : DBNull.Value},
                            {"id", category.Id}, {"customer_id", User.Identity.Name} }
                    );
                }

                return Get();
            }

            var errorsDict = ModelState.Where(x => x.Value.Errors.Count > 0).ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(error => error.ErrorMessage)
            );

            return new JsonResult(errorsDict) { StatusCode = 422 };
        }

        // Delete: api/category/delete
        [HttpDelete("delete")]
        [Authorize]
        public IActionResult Delete(int id, bool hasSubCategories = false)
        {
            if (hasSubCategories)
            {
                _DBHelper.Write(
                    @"DELETE FROM category WHERE parent_id = @id AND customer_id = @customer_id",
                    new Dictionary<string, object>()
                    { {"id", id}, {"customer_id", User.Identity.Name} }
                );
            }
            
            _DBHelper.Write(
                @"DELETE FROM category WHERE id = @id AND customer_id = @customer_id",
                new Dictionary<string, object>()
                { {"id", id}, {"customer_id", User.Identity.Name} }
            );

            return Get();
        }
    }
}

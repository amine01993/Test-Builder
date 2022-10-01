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
        public CategoryController(IDBHelper DBHelper)
        {
        }

        // Get: api/category
        [HttpGet]
        [Authorize]
        public IActionResult Get([FromServices] ICategoryService categoryService)
        {
            var categories = categoryService.Get();
            
            return new JsonResult(categories);
        }

        // Post: api/category/add
        [HttpPost("add")]
        [Authorize]
        public IActionResult Add([FromServices] ICategoryService categoryService, [FromBody] Category category)
        {
            if (ModelState.IsValid)
            {
                categoryService.InsertOrUpdate(category);

                return Get(categoryService);
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
        public IActionResult Delete([FromServices] ICategoryService categoryService, int id, bool hasSubCategories = false)
        {
            categoryService.Delete(id, hasSubCategories);

            return Get(categoryService);
        }
    }
}

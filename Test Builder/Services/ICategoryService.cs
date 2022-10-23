using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Test_Builder.Models;

namespace Test_Builder.Services
{
    public interface ICategoryService
    {
        public IEnumerable<Category> Get();
        public void Insert(Category category);
        public void Update(Category category);
        public void InsertOrUpdate(Category category);
        public void Delete(int id, bool hasSubCategories);
    }
}

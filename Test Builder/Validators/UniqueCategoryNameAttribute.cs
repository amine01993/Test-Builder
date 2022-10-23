using System.ComponentModel.DataAnnotations;
using Test_Builder.Models;

using Test_Builder.Services;

namespace Test_Builder.Validators
{
    public class UniqueCategoryNameAttribute: ValidationAttribute
    {
        public UniqueCategoryNameAttribute()
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dBContext = validationContext.GetService<IDBContext>();
            var contextAccessor = validationContext.GetService<IHttpContextAccessor>();
            var category = (Category) validationContext.ObjectInstance;

            var customer_id = contextAccessor.HttpContext.User.Identity.Name;

            int count;
            // IF Subcategory
            if (category.ParentId.HasValue)
            {
                count = dBContext.GetScalar<int>(
                    @"SELECT COUNT(*) 
                    FROM category 
                    WHERE name = @name AND parent_id = @parent_id 
                        AND (customer_id = @customer_id OR customer_id IS NULL)",
                    new Dictionary<string, object> {
                        { "name", value}, { "parent_id", category.ParentId }, { "customer_id", customer_id } }
                );
            }
            else
            {   // IF Category
                count = dBContext.GetScalar<int>(
                    @"SELECT COUNT(*) 
                    FROM category 
                    WHERE name = @name AND parent_id IS NULL 
                        AND (customer_id = @customer_id OR customer_id IS NULL)",
                    new Dictionary<string, object> {
                        { "name", value}, { "customer_id", customer_id } }
                );
            }

            if (count > 0)
                return new ValidationResult($"The name '{value}' is already used");

            return ValidationResult.Success;
        }
    }
}

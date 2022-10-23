using System.ComponentModel.DataAnnotations;
using Test_Builder.Models;

using Test_Builder.Services;

namespace Test_Builder.Validators
{
    /**
     *  Check if Foreign keys belong to the current Customer 
     */
    public class BelongToCustomerAttribute: ValidationAttribute
    {
        private string table, id;
        public BelongToCustomerAttribute(string table, string id)
        {
            this.table = table;
            this.id = id;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var dBContext = validationContext.GetService<IDBContext>();
            var contextAccessor = validationContext.GetService<IHttpContextAccessor>();

            var customer_id = contextAccessor.HttpContext.User.Identity.Name;

            int count;
            count = dBContext.GetScalar<int>(
                $@"SELECT COUNT(*) 
                FROM {table} 
                WHERE {id} = @id AND (customer_id = @customer_id OR customer_id IS NULL)",
                new Dictionary<string, object> {
                    { "id", value}, { "customer_id", customer_id } }
            );

            if (count == 0)
                return new ValidationResult(ErrorMessage ?? "This entity doesn't exist");

            return ValidationResult.Success;
        }
    }
}

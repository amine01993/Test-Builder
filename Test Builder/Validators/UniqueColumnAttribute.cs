using System.ComponentModel.DataAnnotations;
//using Microsoft.Extensions.DependencyInjection;

using Test_Builder.Services;

namespace Test_Builder.Validators
{
    public class UniqueColumnAttribute: ValidationAttribute
    {
        public string columnName { get; set; }
        public string table { get; set; }
        public UniqueColumnAttribute(string columnName, string table)
        {
            this.columnName = columnName;
            this.table = table;
        }

        public string GetErrorMessage() => $"This {columnName} is already used.";

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var dBContext = validationContext.GetService<IDBContext>();
            var query = $@"SELECT COUNT(*) FROM {table} WHERE {columnName} = @param0";
            var param0 = value?.ToString() ?? "";
            var count = dBContext.GetScalar<int>(query, new Dictionary<string, object>() { { "param0", param0 } });
            if (count > 0)
                return new ValidationResult(GetErrorMessage());
            return ValidationResult.Success;
        }
    }
}

using System.ComponentModel.DataAnnotations;

using Test_Builder.Services;

namespace Test_Builder.Validators
{
    /**
     *  Check if Foreign keys belong to the current Customer 
     */
    public class BelongToAttribute: ValidationAttribute
    {
        private string table, id;
        public BelongToAttribute(string table, string id)
        {
            this.table = table;
            this.id = id;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var _DBHelper = validationContext.GetService<IDBHelper>();

            int count;
            count = _DBHelper.Query<int>(
                $@"SELECT COUNT(*) 
                FROM {table} 
                WHERE {id} = @id",
                new Dictionary<string, object> {
                    { "id", value}}
            );

            if (count == 0)
                return new ValidationResult(ErrorMessage ?? "This entity doesn't exist");

            return ValidationResult.Success;
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Test_Builder.Validators
{
    public class GreaterThanOrEqualToPropertyAttribute : ValidationAttribute
    {
        private string property; 
        public GreaterThanOrEqualToPropertyAttribute(string property)
        {
            this.property = property;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var obj = validationContext.ObjectInstance;

            var number = obj.GetType().GetProperty(property).GetValue(obj, null);

            if (number == null || value == null || (double)value >= (double)number)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"Value {value} must be greater than or equal to {number}");
        }
    }
}

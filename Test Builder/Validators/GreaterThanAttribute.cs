using System.ComponentModel.DataAnnotations;

namespace Test_Builder.Validators
{
    public class GreaterThanAttribute: ValidationAttribute
    {
        private double number; 
        public GreaterThanAttribute(double number)
        {
            this.number = number;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value == null || (double)value > number)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"Value {value} must be greater than {number}");
        }
    }

    public class GreaterThanOrEqualToAttribute : ValidationAttribute
    {
        private double number;
        public GreaterThanOrEqualToAttribute(double number)
        {
            this.number = number;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || (double)value >= number)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? $"Value {value} must be greater than or equal to {number}");
        }
    }
}

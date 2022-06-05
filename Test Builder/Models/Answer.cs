using System.ComponentModel.DataAnnotations;
using Test_Builder.Validators;

namespace Test_Builder.Models
{
    public class Answer
    {
        public int Id { get; set; }
        [Required]
        public string _Answer { get; set; }
        public string? Match { get; set; }
        //[BelongToCustomer("question", "id", ErrorMessage = "This Question doesn't exist")]
        public int QuestionId { get; set; }
        public bool Correct { get; set; }
        [GreaterThan(0)]
        [GreaterThanOrEqualToProperty("Penalty")]
        public double? Points { get; set; }
        [GreaterThanOrEqualTo(0)]
        public double? Penalty { get; set; }
    }
}

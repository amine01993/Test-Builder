using System.ComponentModel.DataAnnotations;
using Test_Builder.Validators;

namespace Test_Builder.Models
{
    public class Question
    {
        public int Id { get; set; }
        [BelongTo("question_type", "id", ErrorMessage = "This Question type doesn't exist")]
        public int TypeId { get; set; }
        [BelongToCustomer("category", "id", ErrorMessage = "This Category doesn't exist")]
        public QuestionType? QuestionType { get; set; }
        public int CategoryId { get; set; }
        [GreaterThan(0)]
        [GreaterThanOrEqualToProperty("Penalty")]
        public double Points { get; set; }
        [GreaterThanOrEqualTo(0)]
        public double? Penalty { get; set; }
        [Range(0, 3)]
        public int? Shuffle { get; set; }
        [Range(0, 1)]
        public int? Selection { get; set; }
        [Required]
        [StringLength(100000, MinimumLength = 5)]
        public string _Question { get; set; }
        public List<Answer>? Answers { get; set; }
    }

    //public class QuestionResult
    //{
    //    public int Id { get; set; }
    //    public string Question { get; set; }
    //    public double Points { get; set; }
    //    public string CategoryName { get; set; }
    //    public string SubCategoryName { get; set; }
    //    public string QuestionTypeName { get; set; }
    //}
}

using System.ComponentModel.DataAnnotations;
using Test_Builder.Validators;

namespace Test_Builder.Models
{
    public class PageQuestion
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        [BelongToCustomer("question", "id", ErrorMessage = "This Question doesn't exist")]
        public int? QuestionId { get; set; }
        public Question? Question { get; set; }
        public int Position { get; set; }
        
        public bool Random { get; set; }
        public int? Number { get; set; }
        [MaxLength(200)]
        public string? QuestionIds { get; set; }


        //public string? Question { get; set; }
        //public byte? Selection { get; set; }

        //[BelongToCustomer("category", "id", ErrorMessage = "This Category doesn't exist")]
        //public int? CategoryId { get; set; }
        //[BelongToCustomer("category", "id", ErrorMessage = "This SubCategory doesn't exist")]
        //public int? SubCategoryId { get; set; }
        //[BelongToCustomer("question_type", "id", ErrorMessage = "This Question type doesn't exist")]
        //public int? TypeId { get; set; }
        //public string? TypeName { get; set; }

        //public List<Answer>? Answers { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;
using Test_Builder.Validators;

namespace Test_Builder.Models
{
    public class Test
    {
        public int Id { get; set; }
        [Required]
        [MinLength(5)]
        [MaxLength(30)]
        public string Name { get; set; }
        public string? Introduction { get; set; }
        [BelongToCustomer("category", "id", ErrorMessage = "This Category doesn't exist")]
        public int CategoryId { get; set; }
        public int? Limit { get; set; }
    }
}

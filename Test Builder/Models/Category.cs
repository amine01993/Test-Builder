using System.ComponentModel.DataAnnotations;
using Test_Builder.Validators;

namespace Test_Builder.Models
{
    public class Category
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [UniqueCategoryName]
        public string Name { get; set; }
        [BelongToCustomer("category", "id", ErrorMessage = "This Category doesn't exist")]
        public int? ParentId { get; set; }
    }
}

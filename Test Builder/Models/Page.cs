using System.ComponentModel.DataAnnotations;
using Test_Builder.Validators;

namespace Test_Builder.Models
{
    public class Page
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public string Name { get; set; }
        public int? Limit { get; set; }
        public bool Shuffle { get; set; }
        public int Position { get; set; }
        [BelongToCustomer("test", "id", ErrorMessage = "This Test doesn't exist")]
        public int TestId { get; set; }
        public IList<PageQuestion>? PageQuestions { get; set; }
    }

    public class Position
    {
        public int Id { get; set; }
        public int _Position { get; set; }
    }
}

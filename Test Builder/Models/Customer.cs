using System.ComponentModel.DataAnnotations;

using Test_Builder.Validators;

namespace Test_Builder.Models
{
    public class Customer
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        [UniqueColumn("email", "customer")]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        public string Password { get; set; }
        //public DateTime CreateAt { get; set; }
        //public DateTime ModifiedAt { get; set; }
    }
}

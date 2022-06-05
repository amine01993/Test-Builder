using System.ComponentModel.DataAnnotations;

namespace Test_Builder.Models
{
    public class QuestionType
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Icon { get; set; }
        [MaxLength(50)]
        public string Link { get; set; }
    }
}

using Newtonsoft.Json;
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

    public class UsedInTestConverter : JsonConverter<Test>
    {
        public override Test? ReadJson(JsonReader reader, Type objectType, Test? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, Test? value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(Test.Id));
            writer.WriteValue(value?.Id);
            writer.WritePropertyName(nameof(Test.Name));
            writer.WriteValue(value?.Name);
            writer.WriteEndObject();
        }
    }
}

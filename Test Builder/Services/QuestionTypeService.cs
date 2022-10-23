using Test_Builder.Models;

namespace Test_Builder.Services
{
    public class QuestionTypeService : IQuestionTypeService
    {
        private readonly IDBContext dBContext;

        public QuestionTypeService(IDBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        public IEnumerable<QuestionType> List()
        {
            var questionTypes = dBContext.List<QuestionType>(
                @"SELECT id AS Id, name AS Name, icon AS Icon, link AS Link 
                FROM question_type"
            );

            return questionTypes;
        }


    }
}

using Test_Builder.Models;

namespace Test_Builder.Services
{
    public class QuestionTypeService : IQuestionTypeService
    {
        private readonly IDBHelper dBHelper;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly int customer_id;

        public QuestionTypeService(IDBHelper dBHelper, IHttpContextAccessor httpContextAccessor)
        {
            this.dBHelper = dBHelper;
            this.httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<QuestionType> List()
        {
            var questionTypes = dBHelper.QueryList2<QuestionType>(
                @"SELECT id AS Id, name AS Name, icon AS Icon, link AS Link 
                FROM question_type"
            );

            return questionTypes;
        }


    }
}

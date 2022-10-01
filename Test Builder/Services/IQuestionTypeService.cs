using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Test_Builder.Models;

namespace Test_Builder.Services
{
    public interface IQuestionTypeService
    {
        public IEnumerable<QuestionType> List();
    }
}

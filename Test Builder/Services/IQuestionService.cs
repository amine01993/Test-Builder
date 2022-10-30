using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Test_Builder.Models;

namespace Test_Builder.Services
{
    public interface IQuestionService
    {
        public Question? Get(int questionId);
        public IEnumerable<Question> Get(IEnumerable<int> ids);
        public DataResult<Question> Search(DataParameters parameters);
        public int Insert(Question question);
        public void Update(Question question);
        public int? Duplicate(int questionId);
        public IEnumerable<Test> UsedIn(int questionId);
        public int Delete(int questionId);
        public void Import(IEnumerable<Question> questions);
    }
}

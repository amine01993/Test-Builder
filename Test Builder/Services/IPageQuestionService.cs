using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Test_Builder.Models;

namespace Test_Builder.Services
{
    public interface IPageQuestionService
    {
        public IEnumerable<int> GetUsedQuestions(IEnumerable<int> IEnumerable, int pageId);
        public int Insert(PageQuestion category);
        public int MaxPosition(int pageId);
        public void SetPositions(IEnumerable<Position> positions);
        public void Delete(int id);
    }
}

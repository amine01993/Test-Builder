using Test_Builder.Models;

namespace Test_Builder.Services
{
    public interface ITestService    {
        public Test Get(int id);
        public IEnumerable<Test> List();

        public int Add(Test test);
    }
}

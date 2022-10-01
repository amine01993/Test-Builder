using Test_Builder.Models;

namespace Test_Builder.Services
{
    public interface IPageService {
        public Page Get(int id);
        public void Insert(Page page);
        public void Update(Page page);
        public void InsertOrUpdate(Page page);
        public void Delete(int id, int testId);
        public void SetPositions(IEnumerable<Position> positions, int testId);
    }
}

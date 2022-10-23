using Test_Builder.Models;

namespace Test_Builder.Services
{
    public interface ICustomerService
    {
        public object Insert(Customer customer);
        public Customer? Get(int id);
        public Customer? GetByEmail(string email);
    }
}

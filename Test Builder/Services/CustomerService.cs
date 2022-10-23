using Test_Builder.Models;

namespace Test_Builder.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IDBContext dBContext;

        public CustomerService(IDBContext dBContext)
        {
            this.dBContext = dBContext;
        }
        
        public object Insert(Customer customer)
        {
            var query = @"INSERT INTO customer(name, email, password)
                    OUTPUT INSERTED.id
                    VALUES(@name, @email, @password)";

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(customer.Password);

            var parameters = new Dictionary<string, object>();
            parameters.Add("name", customer.Name);
            parameters.Add("email", customer.Email);
            parameters.Add("password", hashedPassword);

            // return Id
            var id = dBContext.Write(query, parameters);
            return id;
        }

        public Customer? Get(int id)
        {
            var customer = dBContext.Get<Customer>(
               @"SELECT name AS Name FROM customer WHERE id = @id",
               new Dictionary<string, object>() { { "id", id } }
           );

            return customer;
        }

        public Customer? GetByEmail(string email)
        {
            string query = @"SELECT id AS Id, password AS Password
                FROM customer
                WHERE email = @email";

            var parameters = new Dictionary<string, object>() { { "email", email } };

            var customer = dBContext.Get<Customer>(query, parameters);

            return customer;
        }
    }
}

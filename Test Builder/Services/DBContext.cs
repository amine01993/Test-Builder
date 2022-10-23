using System.Data.SqlClient;
using Dapper;

namespace Test_Builder.Services
{
    public class DBContext: IDBContext
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString = "TestBuilder";
        private readonly string sqlDataSource;

        public DBContext(IConfiguration configuration)
        {
            _configuration = configuration;
            sqlDataSource = _configuration.GetConnectionString(connectionString);
        }

        public int Execute(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(sqlDataSource))
            {
                return connection.Execute(sql, new DynamicParameters(parameters));
            }
        }

        public object Write(string sql, Dictionary<string, object> parameters = null)
        {
            object id = null;
            using (SqlConnection connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var el in parameters)
                        {
                            command.Parameters.AddWithValue("@" + el.Key, el.Value);
                        }
                    }

                    id = command.ExecuteScalar();
                    connection.Close();
                }
            }
            return id;
        }

        public T? GetScalar<T>(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(sqlDataSource))
            {
                var scalar = connection.ExecuteScalar<T>(sql, new DynamicParameters(parameters));
                return scalar;
            }
        }

        public T? Get<T>(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(sqlDataSource))
            {
                var result = connection.QueryFirstOrDefault<T>(sql, new DynamicParameters(parameters));
                return result;
            }
        }

        public IEnumerable<T> List<T>(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(sqlDataSource))
            {
                var result = connection.Query<T>(sql, new DynamicParameters(parameters));
                
                //connection.Query<Question, Category, Question>(sql, );
                return result;
            }
        }

        public SqlConnection Connection()
        {
            return new SqlConnection(sqlDataSource);
        }
    }
}

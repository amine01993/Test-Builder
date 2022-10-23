using System.Data.SqlClient;

namespace Test_Builder.Services
{
    public interface IDBContext
    {
        public int Execute(string sql, Dictionary<string, object> parameters = null);
        public object Write(string sql, Dictionary<string, object> parameters = null);
        public T? GetScalar<T>(string sql, Dictionary<string, object> parameters = null);
        public T? Get<T>(string sql, Dictionary<string, object> parameters = null);
        public IEnumerable<T> List<T>(string sql, Dictionary<string, object> parameters = null);
        public SqlConnection Connection();
    }
}

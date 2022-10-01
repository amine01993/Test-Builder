using Test_Builder.Models;

namespace Test_Builder.Services
{
    public interface IDBHelper
    {
        public object Write(string sql, Dictionary<string, object> parameters = null);
        public T? Query<T>(string sql, Dictionary<string, object> parameters = null);
        public T? Query2<T>(string sql, Dictionary<string, object> parameters = null, string dataTableName = "");
        public List<T> QueryList<T>(string sql, Dictionary<string, object> parameters = null);
        public List<T> QueryList2<T>(string sql, Dictionary<string, object> parameters = null, string dataTableName = "");

    }
}

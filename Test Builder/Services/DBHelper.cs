using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using AutoMapper;
using Dapper;
using Test_Builder.Models;

namespace Test_Builder.Services
{
    public class DBHelper: IDBHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;
        private readonly string connectionString = "TestBuilder";

        public DBHelper(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public T? Query2<T>(string sql, Dictionary<string, object> parameters = null, string dataTableName = "")
        {
            DataTable table = new DataTable(dataTableName);
            string sqlDataSource = _configuration.GetConnectionString(connectionString);
            SqlDataReader reader;
            using (SqlConnection connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                //connection.
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var el in parameters)
                        {
                            command.Parameters.AddWithValue("@" + el.Key, el.Value);
                        }
                    }

                    reader = command.ExecuteReader();
                    table.Load(reader);

                    reader.Close();
                    connection.Close();
                }
            }

            if (table.Rows.Count > 0)
                return _mapper.Map<T>(table.Rows[0]);
            return default;
        }

        public List<T> QueryList2<T>(string sql, Dictionary<string, object> parameters = null, string dataTableName = "")
        {
            DataTable table = new DataTable(dataTableName);
            string sqlDataSource = _configuration.GetConnectionString(connectionString);
            SqlDataReader reader;
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

                    reader = command.ExecuteReader();
                    table.Load(reader);

                    reader.Close();
                    connection.Close();
                }
            }

            var data = new List<T>();
            foreach (var el in table.Rows)
            {
                data.Add(_mapper.Map<T>(el));
            }
            return data;
        }

    }
}

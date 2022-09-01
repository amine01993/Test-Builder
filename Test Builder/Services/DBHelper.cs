using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using AutoMapper;

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

        public object Write(string sql, Dictionary<string, object> parameters = null)
        {
            string sqlDataSource = _configuration.GetConnectionString(connectionString);
            object id = null;
            using (SqlConnection connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    if(parameters != null)
                    {
                        foreach(var el in parameters)
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

        public T? Query<T>(string sql, Dictionary<string, object> parameters = null)
        {
            DataTable table = new DataTable();
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

            var list = ConvertDataTable<T>(table);
            return list.Count > 0 ? list[0] : default;
        }
        public T? Query2<T>(string sql, Dictionary<string, object> parameters = null)
        {
            DataTable table = new DataTable();
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

            if (table.Rows.Count > 0)
                return _mapper.Map<T>(table.Rows[0]);
            return default;
        }

        public List<T> QueryList<T>(string sql, Dictionary<string, object> parameters = null)
        {            
            DataTable table = new DataTable();
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

            return ConvertDataTable2<T>(table);
        }
        public List<T> QueryList2<T>(string sql, Dictionary<string, object> parameters = null)
        {
            DataTable table = new DataTable();
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

        public DataResult<TestQuestion> GetDataResult(string sql, DataParameters parameters)
        {
            var sqlParameters = new Dictionary<string, object>() 
            { { "customer_id", _httpContextAccessor.HttpContext.User.Identity.Name } };
            
            IDictionary<string, string> filter = parameters.decodeFilter();
            //IDictionary<string, string> orderBy = parameters.decodeParam(parameters._orderBy);

            foreach (var item in filter)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    //string[] values;
                    //IList<string> paramList;
                    switch (item.Key)
                    {
                        case "status":
                            if (item.Value == "0") {
                                sql = sql.Replace("#status", "");
                            }
                            else if (item.Value == "1") {
                                sql = sql.Replace("#status", "AND tq.id IS NOT NULL");
                            }
                            else if (item.Value == "2") {
                                sql = sql.Replace("#status", "AND tq.id IS NULL");
                            }
                            break;
                        case "type":
                            if(item.Value == "0")
                                sql = sql.Replace("#questionType", "");
                            else
                            {
                                sqlParameters.Add("type", item.Value);
                                sql = sql.Replace("#questionType", "AND qt.id = @type");
                            }
                            break;
                        case "category":
                            if (item.Value == "0")
                                sql = sql.Replace("#category", "");
                            else
                            {
                                sqlParameters.Add("category", item.Value);
                                sql = sql.Replace("#category", "AND c.id = @category");
                            }
                            break;
                        case "subCategory":
                            if (item.Value == "0")
                                sql = sql.Replace("#subCategory", "");
                            else
                            { 
                                sqlParameters.Add("subCategory", item.Value);
                                sql = sql.Replace("#subCategory", "AND sc.id = @subCategory");
                            }
                            break;
                        case "term":
                            sqlParameters.Add("term", "%" + item.Value + "%");
                            sql = sql.Replace("#searchTerm", "AND q.question LIKE @term");
                            break;
                        default:
                            break;
                    }
                }
            }

            sql = sql.Replace("#status", "").Replace("#questionType", "").Replace("#category", "")
                .Replace("#subCategory", "").Replace("#searchTerm", "");

            //var orderList = new List<string>();
            //foreach(var item in orderBy)
            //{
            //    orderList.Add($"{item.Key} {item.Value}");
            //}
            //var orderStr = orderList.Count == 0 ? "" : "ORDER BY " + string.Join(", ", orderList);

            var totalQuery = $@"SELECT COUNT(*) FROM ({sql}) as t";
            var total = Query<int>(totalQuery, sqlParameters);

            var query = $@"{sql} 
                ORDER BY QuestionId
                OFFSET {(parameters.page - 1) * parameters.pageSize} ROWS 
                FETCH NEXT {parameters.pageSize} ROWS ONLY";
            var list = QueryList2<TestQuestion>(query, sqlParameters); // test questions results

            var paramDict = new Dictionary<string, object> 
                { { "customer_id", _httpContextAccessor.HttpContext.User.Identity.Name } };
            var inList = new List<string>() { "0" };
            var index = 0;

            foreach (var question in list)
            {
                if (question.QuestionId.HasValue)
                {
                    paramDict.Add("question_" + index, question.QuestionId);
                    inList.Add("@question_" + index++);
                }
            }

            var answers = QueryList2<Answer>(
                $@"SELECT a.id AS Id, a.answer AS _Answer, a.correct AS Correct, a.match AS Match,
                    a.points AS Points, a.penalty AS Penalty, a.question_id AS QuestionId
                FROM answer a
                WHERE a.question_id IN ({string.Join(',', inList)}) AND a.customer_id = @customer_id
                ORDER BY a.question_id", paramDict
            );

            index = 0;
            foreach (var question in list)
            {
                while (index < answers.Count && answers[index].QuestionId == question.QuestionId)
                {
                    if (question.Answers == null)
                        question.Answers = new List<Answer>();
                    question.Answers.Add(answers[index++]);
                }
            }

            var result = new DataResult<TestQuestion>() { Total = total, Count = list.Count, Data = list };

            return result;
        }

        private List<T> ConvertDataTable2<T>(DataTable dt)
        {
            var data = new List<T>();

            if (dt.Rows.Count == 0)
                return data;
            
            bool firstCol = true;
            foreach (DataColumn column in dt.Columns)
            {
                var prop = typeof(T).GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance);
                if(prop == null || !prop.CanWrite)
                    continue;

                var index = 0;
                foreach (DataRow row in dt.Rows)
                {
                    T obj;
                    if (firstCol)
                    {
                        obj = Activator.CreateInstance<T>();
                        data.Add(obj);
                    }
                    else
                    {
                        obj = data[index++];
                    }

                    try
                    {
                        prop.SetValue(obj, row[column.ColumnName], null);
                    }
                    catch (Exception ex) { }
                }

                if(firstCol)
                    firstCol = false;
            }

            return data;
        }

        private List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);

            if(temp.BaseType?.Name == "ValueType")
            {
                return (T)dr.ItemArray[0];
            }
            else // Object
            {
                T obj = Activator.CreateInstance<T>();

                //foreach (DataColumn column in dr.Table.Columns)
                //{
                //    foreach (PropertyInfo pro in temp.GetProperties())
                //    {
                //        if (pro.Name == column.ColumnName)
                //        {
                //            pro.SetValue(obj, dr[column.ColumnName], null);
                //            continue;
                //        }
                //    }
                //}
                foreach(DataColumn column in dr.Table.Columns)
                {
                    try
                    {
                        var prop = obj.GetType().GetProperty(column.ColumnName, BindingFlags.Public | BindingFlags.Instance);
                        if (null != prop && prop.CanWrite)
                        {
                            prop.SetValue(obj, dr[column.ColumnName], null);
                        }
                    }
                    catch (Exception ex) { }
                }

                //foreach (PropertyInfo pro in temp.GetProperties())
                //{
                //    try
                //    {
                //        pro.SetValue(obj, dr[pro.Name], null);                        
                //    }
                //    catch (Exception ex) { }
                //}
                    return obj;
            }
        }
    }
}

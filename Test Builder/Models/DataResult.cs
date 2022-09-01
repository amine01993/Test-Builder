namespace Test_Builder.Models
{
    public class DataResult<T>
    {
        public int Total { get; set; }
        public int Count { get; set; }
        public List<T>? Data { get; set; }
    }

    public class DataParameters
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public string? _filter { get; set; }
        public string? _orderBy { get; set; }

        public IDictionary<string, string> decodeFilter()
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(_filter))
                return dict;

            var tokens = _filter.Split(';');
            foreach (var token in tokens)
            {
                var t = token.Split('=');
                var key = t[0];
                var val = t[1];
                dict.Add(key, val);
            }
            return dict;
        }
    }
}

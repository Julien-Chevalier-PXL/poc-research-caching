namespace pocCachingApi.BusinessLayer.SQLiteLayer.Models
{
    public class SqliteResponse<T>
    {
        public T Result { get; set; }
        public bool isValid { get; set; } = false;
        public string Message { get; set; }
    }
}
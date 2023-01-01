namespace SqlHelper.Models
{
    public class Column
    {
        public long TableId { get; set; }
        public long ColumnId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Nullable { get; set; }
    }
}

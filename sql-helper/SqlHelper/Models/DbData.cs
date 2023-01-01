namespace SqlHelper.Models
{
    public class DbData
    {
        // SortedDictionary enforces uniqueness 
        public SortedDictionary<long, Table> Tables { get; set; }
        public SortedDictionary<long, Constraint> Constraints { get; set; }
        public SortedDictionary<(long TableId, long ColumnId), Column> Columns { get; set; }
    }
}

namespace SqlHelper.Models
{
    public class SqlQueryParameters
    {
        public IList<Table> Tables { get; set; }

        public IList<Column> Filters { get; set; }
    }
}

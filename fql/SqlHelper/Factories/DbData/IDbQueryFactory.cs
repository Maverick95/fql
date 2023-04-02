namespace SqlHelper.Factories.DbData
{
    public interface IDbQueryFactory
    {
        public string GetQueryTables();

        public string GetQueryColumns();

        public string GetQueryConstraints();
    }
}

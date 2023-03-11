using SqlHelper.Helpers;

namespace SqlHelper.Factories.DbData
{
    public class LocalSqlExpressDbDataFactory: IDbDataFactory
    {
        private readonly string _database;
        private readonly IDbQueryFactory _queryFactory;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDbCommandFactory _commandFactory;

        private string _connectionString
        {
            get => $"Server=localhost\\SQLEXPRESS;Database={_database};Trusted_Connection=true;";
        }

        public LocalSqlExpressDbDataFactory(
            string database,
            IDbQueryFactory queryFactory = null,
            IDbConnectionFactory connectionFactory = null,
            IDbCommandFactory commandFactory = null)
        {
            _database = database;
            _queryFactory = queryFactory ?? new FirstDbQueryFactory();
            _connectionFactory = connectionFactory ?? new SqlDbConnectionFactory();
            _commandFactory = commandFactory ?? new SqlDbTextCommandFactory(30);
        }

        public Models.DbData Create()
        {
            var internalFactory = new ConnectionStringDbDataFactory(
                _connectionString,
                _queryFactory,
                _connectionFactory,
                _commandFactory);

            return internalFactory.Create();
        }
    }
}

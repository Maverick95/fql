using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Data;

namespace SqlHelper.Factories.DbData
{
    public class ConnectionStringDbDataFactory: IDbDataFactory
    {
        private readonly string _connectionString;
        private readonly IUniqueIdProvider _uniqueIdProvider;
        private readonly IDbQueryFactory _queryFactory;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDbCommandFactory _commandFactory;

        public ConnectionStringDbDataFactory(
            string connectionString,
            IUniqueIdProvider uniqueIdProvider,
            IDbQueryFactory queryFactory = null,
            IDbConnectionFactory connectionFactory = null,
            IDbCommandFactory commandFactory = null)
        {
            _connectionString = connectionString;
            _uniqueIdProvider = uniqueIdProvider;
            _queryFactory = queryFactory ?? new FirstDbQueryFactory();
            _connectionFactory = connectionFactory ?? new SqlDbConnectionFactory();
            _commandFactory = commandFactory ?? new SqlDbTextCommandFactory(30);
        }

        public Models.DbData Create()
        {
            var tables = new SortedDictionary<long, Table>();
            var columns = new SortedDictionary<(long TableId, long ColumnId), Column>();
            var constraints = new SortedDictionary<long, Models.Constraint>();

            using var conn = _connectionFactory.Create();
            conn.ConnectionString = _connectionString;
            conn.Open();

            // Tables
            using (IDbCommand command = _commandFactory.Create())
            {
                command.Connection = conn;
                command.CommandText = _queryFactory.GetQueryTables();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var id = Convert.ToInt64(reader["Id"]);
                    var schema = reader["Schema"].ToString();
                    var name = reader["Name"].ToString();

                    tables.Add(id, new Table
                    {
                        Id = id,
                        Schema = schema,
                        Name = name,
                    });
                }
            }

            // Columns
            using (IDbCommand command = _commandFactory.Create())
            {
                command.Connection = conn;
                command.CommandText = _queryFactory.GetQueryColumns();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var tableId = Convert.ToInt64(reader["TableId"]);
                    var columnId = Convert.ToInt64(reader["ColumnId"]);
                    var name = reader["Name"].ToString();
                    var type = reader["Type"].ToString();
                    var nullable = Convert.ToBoolean(reader["Nullable"]);

                    columns.Add((tableId, columnId), new Column
                    {
                        TableId = tableId,
                        ColumnId = columnId,
                        Name = name,
                        Type = type,
                        Nullable = nullable,
                    });
                }
            }

            // Constraints
            using (IDbCommand command = _commandFactory.Create())
            {
                command.Connection = conn;
                command.CommandText = _queryFactory.GetQueryConstraints();
                using var reader = command.ExecuteReader();
                var uniqueIdMappings = new Dictionary<long, long>();
                while (reader.Read())
                {
                    /*
                     * Constraints have unique Id set through the provider
                     * This is because Custom Constraints also require a unique Id on-the-fly
                     * This uniqueness must cover ALL Constraints
                     * Its easier to enforce uniquenes this way
                     */
                    var uniqueIdFromProvider = _uniqueIdProvider.Next();
                    var uniqueIdFromDatabase = Convert.ToInt64(reader["Id"]);
                    var targetTableId = Convert.ToInt64(reader["TargetTableId"]);
                    var sourceTableId = Convert.ToInt64(reader["SourceTableId"]);
                    var targetColumn = Convert.ToInt32(reader["TargetColumn"]);
                    var sourceColumn = Convert.ToInt32(reader["SourceColumn"]);

                    if (uniqueIdMappings.TryGetValue(uniqueIdFromDatabase, out var id))
                    {
                        var constraint = constraints[id];
                        constraint.Columns.Add(new Models.ConstraintColumnPair
                        {
                            TargetColumnId = targetColumn,
                            SourceColumnId = sourceColumn,
                        });
                    }
                    else
                    {
                        constraints.Add(uniqueIdFromProvider, new Models.Constraint
                        {
                            Id = uniqueIdFromProvider,
                            TargetTableId = targetTableId,
                            SourceTableId = sourceTableId,
                            Columns = new List<Models.ConstraintColumnPair>
                            {
                                new()
                                {
                                    TargetColumnId = targetColumn,
                                    SourceColumnId = sourceColumn,
                                }
                            },
                        });
                        uniqueIdMappings.Add(uniqueIdFromDatabase, uniqueIdFromProvider);
                    }
                }
            }

            return new()
            {
                Tables = tables,
                Columns = columns,
                Constraints = constraints,
            };
        }
    }
}

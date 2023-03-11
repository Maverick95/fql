using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Data;

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
                while (reader.Read())
                {
                    var id = Convert.ToInt64(reader["Id"]);
                    var targetTableId = Convert.ToInt64(reader["TargetTableId"]);
                    var sourceTableId = Convert.ToInt64(reader["SourceTableId"]);
                    var targetColumn = Convert.ToInt32(reader["TargetColumn"]);
                    var sourceColumn = Convert.ToInt32(reader["SourceColumn"]);

                    if (constraints.TryGetValue(id, out var constraint))
                    {
                        constraint.Columns.Add(new Models.ConstraintColumnPair
                        {
                            TargetColumnId = targetColumn,
                            SourceColumnId = sourceColumn,
                        });
                    }
                    else
                    {
                        constraints.Add(id, new Models.Constraint
                        {
                            Id = id,
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

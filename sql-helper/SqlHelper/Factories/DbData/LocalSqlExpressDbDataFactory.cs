using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Data;

namespace SqlHelper.Factories.DbData
{
    public class LocalSqlExpressDbDataFactory: IDbDataFactory
    {
        private readonly string _database;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDbCommandFactory _commandFactory;

        private string _connectionString
        {
            get => $"Server=localhost\\SQLEXPRESS;Database={_database};Trusted_Connection=true;";
        }

        private string _queryTables
        {
            get => @"
                SELECT			Id = TAB.[object_id],
				                [Schema] = SCH.[name],
				                [Name] = TAB.[name]
                FROM			[sys].[schemas] SCH
                INNER JOIN		[sys].[database_principals] DPR
	                ON			DPR.[principal_id] = SCH.[principal_id]
                INNER JOIN		[sys].[tables] TAB
	                ON			TAB.[schema_id] = SCH.[schema_id]
                WHERE			DPR.[name] IN ('dbo')
                AND				SCH.[name] NOT IN ('dbo','tSQLt')
                AND				TAB.[temporal_type_desc] NOT IN ('HISTORY_TABLE');
            ";
        }

        private string _queryColumns
        {
            get => @"
                SELECT			TableId = TAB.[object_id],
				                ColumnId = ACO.column_id,
				                [Name] = ACO.[name],
				                [Type] = TYP.[name],
				                Nullable = ACO.is_nullable
                FROM			[sys].[schemas] SCH
                INNER JOIN		[sys].[database_principals] DPR
	                ON			DPR.[principal_id] = SCH.[principal_id]
                INNER JOIN		[sys].[tables] TAB
	                ON			TAB.[schema_id] = SCH.[schema_id]
                INNER JOIN		[sys].[all_columns] ACO
	                ON			ACO.[object_id] = TAB.[object_id]
                INNER JOIN		[sys].[types] TYP
	                ON			TYP.system_type_id = ACO.system_type_id
                AND				TYP.user_type_id = ACO.user_type_id
                WHERE			DPR.[name] IN ('dbo')
                AND				SCH.[name] NOT IN ('dbo','tSQLt')
                AND				TAB.[temporal_type_desc] NOT IN ('HISTORY_TABLE');
            ";
        }

        private string _queryConstraints
        {
            get => @"
                SELECT			Id = FKS.[object_id],
				                TargetTableId = FKC.parent_object_id,
				                SourceTableId = FKC.referenced_object_id,
				                TargetColumn = FKC.parent_column_id,
				                SourceColumn = FKC.referenced_column_id
                FROM			[sys].[schemas] SCH
                INNER JOIN		[sys].[database_principals] DPR
	                ON			DPR.[principal_id] = SCH.[principal_id]
                INNER JOIN		[sys].[tables] TAB
	                ON			TAB.[schema_id] = SCH.[schema_id]
                INNER JOIN		[sys].[foreign_keys] FKS
	                ON			FKS.[parent_object_id] = TAB.[object_id]
                INNER JOIN		[sys].[foreign_key_columns] FKC
	                ON			FKC.constraint_object_id = FKS.[object_id]
                WHERE			DPR.[name] IN ('dbo')
                AND				SCH.[name] NOT IN ('dbo','tSQLt')
                AND				TAB.[temporal_type_desc] NOT IN ('HISTORY_TABLE');
            ";
        }

        public LocalSqlExpressDbDataFactory(
            string database,
            IDbConnectionFactory connectionFactory = null,
            IDbCommandFactory commandFactory = null)
        {
            _database = database;
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
                command.CommandText = _queryTables;
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
                command.CommandText = _queryColumns;
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
                command.CommandText = _queryConstraints;
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

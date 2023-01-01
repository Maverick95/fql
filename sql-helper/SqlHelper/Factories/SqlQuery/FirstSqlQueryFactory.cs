using SqlHelper.Extensions;
using SqlHelper.Factories.DefaultTypeValue;
using SqlHelper.Factories.TableAlias;
using SqlHelper.Models;
using SqlHelper.Paths;

namespace SqlHelper.Factories.SqlQuery
{
    public class FirstSqlQueryFactory : ISqlQueryFactory
    {
        private readonly ITableAliasFactory _tableAliasFactory;
        private readonly IDefaultTypeValueFactory _defaultTypeValueFactory;

        public FirstSqlQueryFactory(
            ITableAliasFactory tableAliasFactory,
            IDefaultTypeValueFactory defaultTypeValueFactory)
        {
            _tableAliasFactory = tableAliasFactory;
            _defaultTypeValueFactory = defaultTypeValueFactory;
        }

        public string Generate(Models.DbData data, SqlHelperResult result, SqlQueryParameters parameters)
        {
            var tables = result.Paths.Select(p => p.Table).ToList();
            tables.Add(result.Start);
            var tableAliases = _tableAliasFactory.Create(tables);

            /*
                SELECT 
                [ALIAS_1].*, [ALIAS_2].*, ...
             */
            var select = parameters.Tables
                .Select(table => table.Id)
                .Select(id => tableAliases[id])
                .Select(alias => $"[{alias}].*")
                .Sentence(", ", "*");

            /*
                FROM
                [SCHEMA].[TABLE] [ALIAS]
             */
            var from_alias = tableAliases[result.Start.Id];

            var from = string.Format("[{0}].[{1}] [{2}]",
                result.Start.Schema,
                result.Start.Name,
                from_alias);

            /*
                INNER JOIN  [SCHEMA_SOURCE].[TABLE_SOURCE] [ALIAS_SOURCE]
                ON          [ALIAS_TARGET].[COLUMN_TARGET_1] = [ALIAS_SOURCE].[COLUMN_SOURCE_1]
                AND         [ALIAS_TARGET].[COLUMN_TARGET_2] = [ALIAS_SOURCE].[COLUMN_SOURCE_2]
                AND ...
                INNER JOIN ...
             */
            var joins_data = result.Paths
                .Select(path =>
                {
                    var alias_source = tableAliases[path.Constraint.SourceTableId];
                    var alias_target = tableAliases[path.Constraint.TargetTableId];

                    var source = string.Format("[{0}].[{1}] [{2}]",
                        path.Table.Schema,
                        path.Table.Name,
                        alias_source);

                    var columns_source = path.Constraint.Columns
                        .Select(columns => (path.Constraint.SourceTableId, columns.SourceColumnId))
                        .Select(key => data.Columns[key].Name);

                    var columns_target = path.Constraint.Columns
                        .Select(columns => (path.Constraint.TargetTableId, columns.TargetColumnId))
                        .Select(key => data.Columns[key].Name);

                    var columns = columns_source.Zip(columns_target, (column_source, column_target) => string.Format(
                            "[{0}].[{1}] = [{2}].[{3}]",
                            alias_source,
                            column_source,
                            alias_target,
                            column_target))
                        .Sentence(" AND ");

                    var join = $"INNER JOIN {source} ON {columns}";

                    return join;
                });

            var joins = joins_data.Sentence(" ");

            /*
                WHERE
                [ALIAS_1].[FILTER_COLUMN_1] = DEFAULT_TYPE_VALUE_1
                AND
                [ALIAS_2].[FILTER_COLUMN_2] = DEFAULT_TYPE_VALUE_2
                ...
            */
            var where_data = parameters.Filters.Select(filter =>
            {
                var alias = tableAliases[filter.TableId];
                var column = data.Columns[(filter.TableId, filter.ColumnId)];

                var default_value = _defaultTypeValueFactory.Create(column.Type);

                var where = string.Format("[{0}].[{1}] = {2}",
                    alias,
                    column.Name,
                    default_value);

                return where;
            });

            var where = where_data.Any() ?
                $"WHERE {where_data.Sentence(" AND ")}" :
                string.Empty;

            var output = $"SELECT {select} FROM {from} {joins} {where};";

            return output;
        }
    }
}

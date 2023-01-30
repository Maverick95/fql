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

        public string Generate(Models.DbData data, ResultRouteTree result, SqlQueryParameters parameters)
        {
            /*
             * Currently we are assuming a particular structure for each ResultRouteTree,
             * each has a single parent and a single child.
             */

            // This is the key line.
            var child = result.Children.First();

            var all_tables = child.route.Route
                .Select(r => r.source)
                .Prepend(result.Table);
            var all_aliases = _tableAliasFactory.Create(all_tables).AppendIndex();
            
            var all_data = all_tables
                .Zip(all_aliases, (table, alias) => new
                {
                    Table = table,
                    Alias = alias,
                });

            var source_aliases = all_aliases.Skip(1);
            var target_aliases = all_aliases.SkipLast(1);
            
            var route_aliases = source_aliases.Zip(
                target_aliases, (source, target) => new
                {
                    Source = source,
                    Target = target,
                });

            /*
                SELECT 
                [ALIAS_1].*, [ALIAS_2].*, ...
             */
            var select = all_data
                .Where(data => parameters.Tables.Any(
                    table => table.Id == data.Table.Id))
                .Select(data => $"[{data.Alias}].*")
                .Sentence(", ", "*");

            /*
                FROM
                [SCHEMA].[TABLE] [ALIAS]
             */
            var from_alias = all_aliases.First();
            var from = string.Format("[{0}].[{1}] [{2}]",
                result.Table.Schema,
                result.Table.Name,
                from_alias);

            /*
                INNER JOIN  [SCHEMA_SOURCE].[TABLE_SOURCE] [ALIAS_SOURCE]
                ON          [ALIAS_TARGET].[COLUMN_TARGET_1] = [ALIAS_SOURCE].[COLUMN_SOURCE_1]
                AND         [ALIAS_TARGET].[COLUMN_TARGET_2] = [ALIAS_SOURCE].[COLUMN_SOURCE_2]
                AND ...
                INNER JOIN ...
             */
            var joins_data = child.route.Route
                .Zip(route_aliases, (route, alias) =>
                {
                    var source = string.Format("[{0}].[{1}] [{2}]",
                        route.source.Schema,
                        route.source.Name,
                        alias.Source);

                    var columns_source = route.constraint.Columns
                        .Select(columns => (route.constraint.SourceTableId, columns.SourceColumnId))
                        .Select(key => data.Columns[key].Name);

                    var columns_target = route.constraint.Columns
                        .Select(columns => (route.constraint.TargetTableId, columns.TargetColumnId))
                        .Select(key => data.Columns[key].Name);

                    var columns = columns_source.Zip(columns_target, (column_source, column_target) => string.Format(
                            "[{0}].[{1}] = [{2}].[{3}]",
                            alias.Source,
                            column_source,
                            alias.Target,
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
            var where_data = parameters.Filters
                .Join(
                    all_data,
                    filter => filter.TableId,
                    data => data.Table.Id,
                    (filter, data) =>
                    {
                        var default_value = _defaultTypeValueFactory.Create(filter.Type);
                        var where = string.Format("[{0}].[{1}] = {2}",
                            data.Alias,
                            filter.Name,
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

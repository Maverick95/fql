using SqlHelper.Extensions;
using SqlHelper.Factories.DefaultTypeValue;
using SqlHelper.Factories.TableAlias;
using SqlHelper.Models;
using SqlHelper.Paths;

namespace SqlHelper.Factories.SqlQuery
{
    public class PrettierSqlQueryFactory : ISqlQueryFactory
    {
        private readonly ITableAliasFactory _tableAliasFactory;
        private readonly IDefaultTypeValueFactory _defaultTypeValueFactory;
        private readonly int _padding;

        public PrettierSqlQueryFactory(
            ITableAliasFactory tableAliasFactory,
            IDefaultTypeValueFactory defaultTypeValueFactory,
            int padding)
        {
            _tableAliasFactory = tableAliasFactory;
            _defaultTypeValueFactory = defaultTypeValueFactory;
            _padding = padding;
        }

        public string Generate(Models.DbData data, ResultRoute result, SqlQueryParameters parameters)
        {
            // Prepare prefixes.
            var prefixes = new Dictionary<string, string>
            {
                {   "select_first"      ,       "SELECT"            },
                {   "select_other"      ,       "              ,"   },
                {   "from"              ,       "FROM"              },
                {   "join"              ,       "INNER JOIN"        },
                {   "join_on_first"     ,       "  ON"              },
                {   "join_on_other"     ,       "AND"               },
                {   "where_first"       ,       "WHERE"             },
                {   "where_other"       ,       "AND"               },
            };

            var prefixes_max_length = prefixes.Max(prefix => prefix.Value.Length);

            var padded_prefixes = prefixes.ToDictionary(
                prefix => prefix.Key,
                prefix => prefix.Value.PadRight(prefixes_max_length + _padding));

            var all_tables = result.Route
                .Select(r => r.source)
                .Prepend(result.Start);
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
            var content_select = all_data
                .Where(data => parameters.Tables.Any(
                    table => table.Id == data.Table.Id))
                .Select(data => $"[{data.Alias}].*");

            var padded_prefixes_select = Enumerable
                .Repeat(
                    padded_prefixes["select_other"], content_select.Count() - 1)
                .Prepend(
                    padded_prefixes["select_first"]
                );

            var selects = padded_prefixes_select.Zip(content_select,
                (prefix, content) => $"{prefix}{content}");

            /*
                FROM
                [SCHEMA].[TABLE] [ALIAS]
             */
            var from_alias = all_aliases.First();
            var from = string.Format("{0}[{1}].[{2}] [{3}]",
                padded_prefixes["from"],
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
            var joins = result.Route
                .Zip(route_aliases, (route, alias) => new
                {
                    Route = route,
                    Alias = alias,
                })
                .SelectMany(input =>
                {
                    var source = string.Format("{0}[{1}].[{2}] [{3}]",
                        padded_prefixes["join"],
                        input.Route.source.Schema,
                        input.Route.source.Name,
                        input.Alias.Source);

                    var columns_source = input.Route.constraint.Columns
                        .Select(columns => (input.Route.constraint.SourceTableId, columns.SourceColumnId))
                        .Select(key => data.Columns[key].Name);

                    var columns_target = input.Route.constraint.Columns
                        .Select(columns => (input.Route.constraint.TargetTableId, columns.TargetColumnId))
                        .Select(key => data.Columns[key].Name);

                    var content_columns = columns_source.Zip(columns_target, (column_source, column_target) => string.Format(
                            "[{0}].[{1}] = [{2}].[{3}]",
                            input.Alias.Source,
                            column_source,
                            input.Alias.Target,
                            column_target));

                    var padded_prefixes_columns = Enumerable
                        .Repeat(
                            padded_prefixes["join_on_other"], content_columns.Count() - 1)
                        .Prepend(
                            padded_prefixes["join_on_first"]
                        );


                    var columns = padded_prefixes_columns.Zip(content_columns,
                        (prefix, content) => $"{prefix}{content}");

                    return columns.Prepend(source);
                });

            /*
                WHERE
                [ALIAS_1].[FILTER_COLUMN_1] = DEFAULT_TYPE_VALUE_1
                AND
                [ALIAS_2].[FILTER_COLUMN_2] = DEFAULT_TYPE_VALUE_2
                ...
            */
            var content_where = parameters.Filters
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

            var where = new List<string>();

            if (content_where.Any())
            {
                var padded_prefixes_where = Enumerable
                    .Repeat(
                        padded_prefixes["where_other"], content_where.Count() - 1)
                    .Prepend(
                        padded_prefixes["where_first"]
                    );

                where = padded_prefixes_where
                    .Zip(content_where, (prefix, content) => $"{prefix}{content}")
                    .ToList();
            }

            var output = selects
                .Append(from)
                .Concat(joins)
                .Concat(where)
                .Sentence("\n");

            return output + ";";
        }
    }
}

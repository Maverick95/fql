using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Text.RegularExpressions;
using SqlHelper.Extensions;

namespace SqlHelper.UserInterface.Parameters
{
    public class FirstParameterUserInterface : IParameterUserInterface
    {
        private enum HandlerResult
        {
            NEXT_HANDLER,
            NEXT_COMMAND,
            FINISH,
        }

        private readonly IStream _stream;

        public FirstParameterUserInterface(IStream stream)
        {
            _stream = stream;
        }

        #region Help command
        private HandlerResult Handler_Help(string input)
        {
            var cleaned = input.Clean();
            var help_commands = new string[] { "h", "help" };

            if (help_commands.Contains(cleaned))
            {
                _stream.Write("TODO : write help instructions lol rofl");
                _stream.Padding();
                return HandlerResult.NEXT_COMMAND;
            }

            return HandlerResult.NEXT_HANDLER;
        }
        #endregion

        #region Finish command
        private HandlerResult Handler_Finish(string input)
        {
            var cleaned = input.Clean();
            var execute_commands = new string[] { "e", "exec", "execute" };

            return execute_commands.Contains(cleaned) ? HandlerResult.FINISH : HandlerResult.NEXT_HANDLER;
        }
        #endregion

        #region Add Filters command
        private HandlerResult Handler_AddFilters(string input, DbData data, SqlQueryParameters parameters)
        {
            const int padding = 3;

            var cleaned = input.Clean();
            var rgx_filter = new Regex("^(f|filter) ");
            var match = rgx_filter.Match(cleaned);

            if (match.Success == false)
            {
                return HandlerResult.NEXT_HANDLER;
            }

            var lookups = cleaned
                .Substring(match.Length)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(filter => new Regex($"^{filter}", RegexOptions.IgnoreCase));

            var matches = data.Columns
                .Where(column => lookups.Any(
                    lookup => lookup.IsMatch(column.Value.Name)));

            if (matches.Any() == false)
            {
                _stream.Write("filter command contains no matches, please try again");
                _stream.Padding();
                return HandlerResult.NEXT_COMMAND;
            }

            if (matches.Count() == 1)
            {
                var filter = matches.First().Value;
                var table = data.Tables[filter.TableId];
                var filter_output = $"[{table.Schema}].[{table.Name}].[{filter.Name}]";

                _stream.Write($"Adding 1 filters to the selection ({filter_output})");
                _stream.Padding();

                parameters.Filters = parameters.Filters
                    .UnionBy(new List<Column> { filter }, (filter) => (filter.TableId, filter.ColumnId))
                    .ToList();

                return HandlerResult.NEXT_COMMAND;
            }

            var options_data = matches
                .Select(match => new
                {
                    Table = data.Tables[match.Key.TableId],
                    Column = match.Value,
                })
                .OrderBy(data =>
                {
                    (
                        string column,
                        string schema,
                        string table
                    ) order =
                    (
                        data.Column.Name,
                        data.Table.Schema,
                        data.Table.Name
                    );

                    return order;
                });

            var schema_max_length =
                options_data.Max(data => data.Table.Schema.Length);

            var table_max_length =
                options_data.Max(data => data.Table.Name.Length);

            var schema_space = schema_max_length + padding + 1; // Extra space for the . separator
            var table_space = table_max_length + padding + 1; // Extra space for the . separator

            var id_space = matches.Count().ToString().Length + padding;
            var ids = Enumerable.Range(1, matches.Count());

            var options = ids.Zip(options_data, (id, option) => new
            {
                Id = id,
                Column = option.Column,
                Text =
                    $"{id}".PadRight(id_space) +
                    $"{option.Table.Schema}.".PadRight(schema_space) +
                    $"{option.Table.Name}.".PadRight(table_space) +
                    $"{option.Column.Name}",
            });

            _stream.Write("Enter comma-separated options, for example, to select options 1 and 2, enter '1,2' or '1, 2'");
            foreach (var option in options)
            {
                _stream.Write(option.Text);
            }

            cleaned = _stream.Read().Clean();
            _stream.Padding();

            var selected = cleaned
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Join(
                    options,
                    input => input,
                    option => option.Id.ToString(),
                    (input, option) => option.Column);

            var selected_output = selected
                .Select(column =>
                {
                    var table = data.Tables[column.TableId];
                    return $"[{table.Schema}].[{table.Name}].[{column.Name}]";
                })
                .Sentence(", ", "none found");

            _stream.Write($"Adding {selected.Count()} columns to the selection ({selected_output})");
            _stream.Padding();

            parameters.Filters = parameters.Filters
                .UnionBy(selected, (filter) => (filter.TableId, filter.ColumnId))
                .ToList();

            return HandlerResult.NEXT_COMMAND;
        }
        #endregion

        #region Add Tables command
        private HandlerResult Handler_AddTables(string input, DbData data, SqlQueryParameters parameters)
        {
            const int padding = 3;

            var cleaned = input.Clean();
            var rgx_table = new Regex("^(t|table) ");
            var match = rgx_table.Match(cleaned);

            if (match.Success == false)
            {
                return HandlerResult.NEXT_HANDLER;
            }

            var lookups = cleaned
                .Substring(match.Length)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(table => new Regex($"^{table}", RegexOptions.IgnoreCase));

            var matches = data.Tables
                .Where(table => lookups.Any(
                    lookup => lookup.IsMatch(table.Value.Name)));

            if (matches.Any() == false)
            {
                _stream.Write("table command contains no matches, please try again");
                _stream.Padding();
                return HandlerResult.NEXT_COMMAND;
            }

            if (matches.Count() == 1)
            {
                var table = matches.First().Value;
                var table_output = $"[{table.Schema}].[{table.Name}]";

                _stream.Write($"Adding 1 tables to the selection ({table_output})");
                _stream.Padding();

                parameters.Tables = parameters.Tables
                    .UnionBy(new List<Table> { table }, (table) => table.Id)
                    .ToList();

                return HandlerResult.NEXT_COMMAND;
            }

            var id_space = matches.Count().ToString().Length + padding;
            var ids = Enumerable.Range(1, matches.Count());

            var options_data = matches
                .Select(match => match.Value)
                .OrderBy(table =>
                {
                    (
                        string table,
                        string schema
                    ) order =
                    (
                        table.Name,
                        table.Schema
                    );
                    return order;
                });

            var schema_max_length =
                options_data.Max(data => data.Schema.Length);

            var schema_space = schema_max_length + padding + 1; // Extra space for the . separator

            var options = ids.Zip(options_data, (id, option) => new
            {
                Id = id,
                Table = option,
                Text = $"{id}".PadRight(id_space) + $"{option.Schema}.".PadRight(schema_space) + option.Name,
            });

            _stream.Write("Enter comma-separated options, for example, to select options 1 and 2, enter '1,2' or '1, 2'");
            foreach (var option in options)
            {
                _stream.Write(option.Text);
            }

            cleaned = _stream.Read().Clean();
            _stream.Padding();

            var selected = cleaned
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Join(
                    options,
                    input => input,
                    option => option.Id.ToString(),
                    (input, option) => option.Table);

            var selected_output = selected
                .Select(table => $"[{table.Schema}].[{table.Name}]")
                .Sentence(", ", "none found");

            _stream.Write($"Adding {selected.Count()} tables to the selection ({selected_output})");
            _stream.Padding();

            parameters.Tables = parameters.Tables
                .UnionBy(selected, (table) => table.Id)
                .ToList();

            return HandlerResult.NEXT_COMMAND;
        }
        #endregion

        public SqlQueryParameters GetParameters(DbData data)
        {
            var parameters = new SqlQueryParameters
            {
                Tables = new List<Table>(),
                Filters = new List<Column>(),
            };

            var handlers = new List<Func<string, HandlerResult>>
            {
                (input) => Handler_Finish(input),
                (input) => Handler_AddTables(input, data, parameters),
                (input) => Handler_AddFilters(input, data, parameters),
                (input) => Handler_Help(input),
            };

            var finished = false;

            while (finished == false)
            {
                _stream.Write("Enter command (type 'h' or 'help' for options) :");
                var input = _stream.Read();
                _stream.Padding();
                var handled = false;
                
                foreach (var handler in handlers)
                {
                    var result = handler(input);
                    finished = result == HandlerResult.FINISH;
                    if (result != HandlerResult.NEXT_HANDLER)
                    {
                        handled = true;
                        break;
                    }
                }

                if (handled == false)
                {
                    _stream.Write("Command not found, please try again");
                    _stream.Padding();
                }
            }

            return parameters;
        }
    }
}

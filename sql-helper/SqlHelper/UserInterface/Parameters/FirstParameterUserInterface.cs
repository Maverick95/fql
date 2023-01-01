using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Text.RegularExpressions;

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

        private string Clean(string input)
        {
            var input_transformed = input.Trim().ToLowerInvariant();
            var rgx_whitespace = new Regex("\\s+");
            return rgx_whitespace.Replace(input_transformed, " ");
        }

        private HandlerResult Handler_Help(string input)
        {
            var cleaned = Clean(input);
            var help_commands = new string[] { "h", "help" };

            if (help_commands.Contains(cleaned))
            {
                _stream.Write("TODO : write help instructions lol rofl");
                _stream.Padding();
                return HandlerResult.NEXT_COMMAND;
            }

            return HandlerResult.NEXT_HANDLER;
        }

        private HandlerResult Handler_Finish(string input)
        {
            var cleaned = Clean(input);
            var execute_commands = new string[] { "e", "exec", "execute" };

            return execute_commands.Contains(cleaned) ? HandlerResult.FINISH : HandlerResult.NEXT_HANDLER;
        }

        private HandlerResult Handler_AddFilters(string input, DbData data, SqlQueryParameters parameters)
        {
            var cleaned = Clean(input);
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

            var id_length = matches.Count().ToString().Length + 1;
            var ids = Enumerable.Range(1, matches.Count());

            var options = ids.Zip(matches, (id, match) =>
            {
                var table = data.Tables[match.Key.TableId];
                return new
                {
                    Id = id,
                    Column = match.Value,
                    Text = $"{id}".PadRight(id_length) + $"[{table.Schema}].[{table.Name}].[{match.Value.Name}]",
                };
            });

            _stream.Write("Enter comma-separated options, for example, to select options 1 and 2, enter '1,2' or '1, 2'");
            foreach (var option in options)
            {
                _stream.Write(option.Text);
            }

            cleaned = Clean(_stream.Read());
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

        private HandlerResult Handler_AddTables(string input, DbData data, SqlQueryParameters parameters)
        {
            var cleaned = Clean(input);
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

            var id_length = matches.Count().ToString().Length + 1;
            var ids = Enumerable.Range(1, matches.Count());

            var options = ids.Zip(matches, (id, match) => new
            {
                Id = id,
                Table = match.Value,
                Text = $"{id}".PadRight(id_length) + match.Value.Name,
            });

            _stream.Write("Enter comma-separated options, for example, to select options 1 and 2, enter '1,2' or '1, 2'");
            foreach (var option in options)
            {
                _stream.Write(option.Text);
            }

            cleaned = Clean(_stream.Read());
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

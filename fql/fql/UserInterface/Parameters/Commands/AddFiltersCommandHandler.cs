using fql.UserInterface.Choices.Formatters;
using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Text.RegularExpressions;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class AddFiltersCommandHandler : ICommandHandler
    {
        private readonly IStream _stream;
        private readonly IChoiceFormatter<FilterChoice> _formatter;

        public AddFiltersCommandHandler(IStream stream, IChoiceFormatter<FilterChoice> formatter)
        {
            _stream = stream;
            _formatter = formatter;
        }

        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            const int padding = 3;

            var cleaned = input.Clean();
            var rgx_filter = new Regex("^(f|filter) ");
            var match = rgx_filter.Match(cleaned);

            if (!match.Success)
            {
                return (HandlerResult.NEXT_HANDLER, data, parameters);
            }

            var lookups = cleaned
                .Substring(match.Length)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(filter => new Regex($"{filter}", RegexOptions.IgnoreCase));

            var matches = data.Columns
                .Where(column => lookups.Any(
                    lookup => lookup.IsMatch(column.Value.Name)));

            if (!matches.Any())
            {
                _stream.WriteLine("filter command contains no matches, please try again");
                _stream.Padding();
                return (HandlerResult.NEXT_COMMAND, data, parameters);
            }

            var choices = matches
                .Select(match => new FilterChoice
                {
                    Table = data.Tables[match.Key.TableId],
                    Column = match.Value,
                })
                .OrderBy(data => (data.Column.Name, data.Table.Schema, data.Table.Name));

            var formats = _formatter.Format(choices);

            var id_space = formats.Count().ToString().Length + padding;
            var ids = Enumerable.Range(1, formats.Count());

            var options = ids.Zip(choices, formats).Select(data => new
            {
                Id = data.First,
                Column = data.Second.Column,
                Text = $"{data.First}".PadRight(id_space) + data.Third,
            });

            foreach (var option in options)
            {
                _stream.WriteLine(option.Text);
            }
            _stream.Padding();
            _stream.Write("> ");
            cleaned = _stream.ReadLine().Clean();
            _stream.Padding();

            var selected = cleaned
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
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

            _stream.WriteLine($"Adding {selected.Count()} columns to the selection ({selected_output})");
            _stream.Padding();

            var new_parameters = new SqlQueryParameters
            {
                Tables = parameters.Tables,
                Filters = parameters.Filters
                    .UnionBy(selected, (filter) => (filter.TableId, filter.ColumnId))
                    .ToList(),
            };

            return (HandlerResult.NEXT_COMMAND, data, new_parameters);
        }
    }
}

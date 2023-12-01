using fql.UserInterface.Choices.Formatters;
using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Text.RegularExpressions;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class AddTablesCommandHandler : ICommandHandler
    {
        private readonly IStream _stream;
        private readonly IChoiceFormatter<TableChoice> _formatter;

        public AddTablesCommandHandler(IStream stream, IChoiceFormatter<TableChoice> formatter)
        {
            _stream = stream;
            _formatter = formatter;
        }

        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            const int padding = 3;

            var cleaned = input.Clean();
            var rgx_table = new Regex("^(t|table) ");
            var match = rgx_table.Match(cleaned);

            if (!match.Success)
            {
                return (HandlerResult.NEXT_HANDLER, data, parameters);
            }

            var lookups = cleaned
                .Substring(match.Length)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(table => new Regex($"{table}", RegexOptions.IgnoreCase));

            var matches = data.Tables
                .Where(table => lookups.Any(
                    lookup => lookup.IsMatch(table.Value.Name)));

            if (!matches.Any())
            {
                _stream.WriteLine("table command contains no matches, please try again");
                _stream.Padding();
                return (HandlerResult.NEXT_COMMAND, data, parameters);
            }

            var choices = matches
                .Select(match => new TableChoice { Table = match.Value })
                .OrderBy(table => (table.Table.Name, table.Table.Schema));

            var formats = _formatter.Format(choices);

            var id_space = matches.Count().ToString().Length + padding;
            var ids = Enumerable.Range(1, matches.Count());

            var options = ids.Zip(choices, formats).Select(data => new
            {
                Id = data.First,
                Table = data.Second.Table,
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
                    (input, option) => option.Table);

            var selected_output = selected
                .Select(table => $"[{table.Schema}].[{table.Name}]")
                .Sentence(", ", "none found");

            _stream.WriteLine($"Adding {selected.Count()} tables to the selection ({selected_output})");
            _stream.Padding();

            var new_parameters = new SqlQueryParameters
            {
                Filters = parameters.Filters,
                Tables = parameters.Tables
                    .UnionBy(selected, (table) => table.Id)
                    .ToList(),
            };

            return (HandlerResult.NEXT_COMMAND, data, new_parameters);
        }
    }
}

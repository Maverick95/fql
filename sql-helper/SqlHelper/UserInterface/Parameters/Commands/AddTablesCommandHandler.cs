using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Text.RegularExpressions;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class AddTablesCommandHandler : ICommandHandler
    {
        private readonly IStream _stream;

        public AddTablesCommandHandler(IStream stream)
        {
            _stream = stream;
        }

        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            const int padding = 3;

            var cleaned = input.Clean();
            var rgx_table = new Regex("^(t|table) ");
            var match = rgx_table.Match(cleaned);

            if (match.Success == false)
            {
                return (HandlerResult.NEXT_HANDLER, data, parameters);
            }

            var lookups = cleaned
                .Substring(match.Length)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(table => new Regex($"{table}", RegexOptions.IgnoreCase));

            var matches = data.Tables
                .Where(table => lookups.Any(
                    lookup => lookup.IsMatch(table.Value.Name)));

            if (matches.Any() == false)
            {
                _stream.WriteLine("table command contains no matches, please try again");
                _stream.Padding();
                return (HandlerResult.NEXT_COMMAND, data, parameters);
            }

            var id_space = matches.Count().ToString().Length + padding;
            var ids = Enumerable.Range(1, matches.Count());

            var options_data = matches
                .Select(match => match.Value)
                .OrderBy(table => (table.Name, table.Schema));

            var schema_max_length =
                options_data.Max(data => data.Schema.Length);

            var schema_space = schema_max_length + padding + 1; // Extra space for the . separator

            var options = ids.Zip(options_data, (id, option) => new
            {
                Id = id,
                Table = option,
                Text = $"{id}".PadRight(id_space) + $"{option.Schema}.".PadRight(schema_space) + option.Name,
            });

            foreach (var option in options)
            {
                _stream.WriteLine(option.Text);
            }
            _stream.Padding();
            _stream.Write("Enter comma-separated options, for example, to select options 1 and 2, enter '1,2' or '1, 2' : ");
            cleaned = _stream.ReadLine().Clean();
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

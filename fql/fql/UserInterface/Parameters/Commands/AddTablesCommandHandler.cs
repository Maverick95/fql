using fql.UserInterface.Choices.Formatters;
using fql.UserInterface.Choices.Models;
using fql.UserInterface.Choices.Selectors;
using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Text.RegularExpressions;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class AddTablesCommandHandler : ICommandHandler
    {
        private readonly IStream _stream;
        private readonly IChoiceSelector<TableChoice> _selector;
        private readonly IChoiceFormatter<TableChoice> _formatter;

        public AddTablesCommandHandler(IStream stream, IChoiceSelector<TableChoice> selector, IChoiceFormatter<TableChoice> formatter)
        {
            _stream = stream;
            _selector = selector;
            _formatter = formatter;
        }

        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            var cleaned = input.Clean();

            return _selector switch
            {
                var s when s is FzfChoiceSelector<TableChoice> => TryCommandHandleInternal_Fzf(cleaned, data, parameters),
                var s when s is NumberedListChoiceSelector<TableChoice> => TryCommandHandleInternal_NumberedList(cleaned, data, parameters),
                _ => throw new NotImplementedException("Selector not implemented")
            };
        }

        private (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandleInternal_NumberedList(string input, DbData data, SqlQueryParameters parameters)
        {
            var rgx_table = new Regex("^(t|table) ");
            var match = rgx_table.Match(input);

            if (!match.Success)
            {
                return (HandlerResult.NEXT_HANDLER, data, parameters);
            }

            var lookups = input
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
                .Select(match => new TableChoice { Table = match.Value });
            var selected = _selector.Choose(choices, _formatter);

            var selected_output = selected
                .Select(table => $"[{table.Table.Schema}].[{table.Table.Name}]")
                .Sentence(", ", "none found");

            _stream.WriteLine($"Adding {selected.Count()} tables to the selection ({selected_output})");
            _stream.Padding();

            var new_parameters = new SqlQueryParameters
            {
                Filters = parameters.Filters,
                Tables = parameters.Tables
                    .UnionBy(selected.Select(s => s.Table), (table) => table.Id)
                    .ToList(),
            };

            return (HandlerResult.NEXT_COMMAND, data, new_parameters);
        }

        private (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandleInternal_Fzf(string input, DbData data, SqlQueryParameters parameters)
        {
            var inputOptions = new[] { "t", "table" };

            if (!inputOptions.Contains(input))
            {
                return (HandlerResult.NEXT_HANDLER, data, parameters);
            }

            var choices = data.Tables
                .Select(match => new TableChoice { Table = match.Value })
                .OrderBy(table => (table.Table.Name, table.Table.Schema));

            var selected = _selector.Choose(choices, _formatter);

            var selected_output = selected
                .Select(table => $"[{table.Table.Schema}].[{table.Table.Name}]")
                .Sentence(", ", "none found");

            _stream.WriteLine($"Adding {selected.Count()} tables to the selection ({selected_output})");
            _stream.Padding();

            var new_parameters = new SqlQueryParameters
            {
                Filters = parameters.Filters,
                Tables = parameters.Tables
                    .UnionBy(selected.Select(s => s.Table), (table) => table.Id)
                    .ToList(),
            };

            return (HandlerResult.NEXT_COMMAND, data, new_parameters);
        }
    }
}

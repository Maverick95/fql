using fql.UserInterface.Choices.Formatters;
using fql.UserInterface.Choices.Models;
using fql.UserInterface.Choices.Selectors;
using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Text.RegularExpressions;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class AddFiltersCommandHandler : ICommandHandler
    {
        private readonly IStream _stream;
        private readonly IChoiceSelector<FilterChoice> _selector;
        private readonly IChoiceFormatter<FilterChoice> _formatter;

        public AddFiltersCommandHandler(IStream stream, IChoiceSelector<FilterChoice> selector, IChoiceFormatter<FilterChoice> formatter)
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
                var s when s is FzfChoiceSelector<FilterChoice> => TryCommandHandleInternal_Fzf(cleaned, data, parameters),
                var s when s is NumberedListChoiceSelector<FilterChoice> => TryCommandHandleInternal_NumberedList(cleaned, data, parameters),
                _ => throw new NotImplementedException("Selector not implemented")
            };
        }

        private (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandleInternal_NumberedList(string input, DbData data, SqlQueryParameters parameters)
        {
            var rgx_filter = new Regex("^(f|filter) ");
            var match = rgx_filter.Match(input);

            if (!match.Success)
            {
                return (HandlerResult.NEXT_HANDLER, data, parameters);
            }

            var lookups = input
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
                });

            var selected = _selector.Choose(choices, _formatter);

            var selected_output = selected
                .Select(choice =>
                {
                    var table = data.Tables[choice.Table.Id];
                    return $"[{table.Schema}].[{table.Name}].[{choice.Column.Name}]";
                })
                .Sentence(", ", "none found");

            _stream.WriteLine($"Adding {selected.Count()} columns to the selection ({selected_output})");
            _stream.Padding();

            var new_parameters = new SqlQueryParameters
            {
                Tables = parameters.Tables,
                Filters = parameters.Filters
                    .UnionBy(selected.Select(s => s.Column), (filter) => (filter.TableId, filter.ColumnId))
                    .ToList(),
            };

            return (HandlerResult.NEXT_COMMAND, data, new_parameters);
        }

        private (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandleInternal_Fzf(string input, DbData data, SqlQueryParameters parameters)
        {
            var inputOptions = new[] { "f", "filter" };

            if (!inputOptions.Contains(input))
            {
                return (HandlerResult.NEXT_HANDLER, data, parameters);
            }
            
            var choices = data.Columns
                .Select(match => new FilterChoice
                {
                    Table = data.Tables[match.Key.TableId],
                    Column = match.Value,
                })
                .OrderBy(data => (data.Column.Name, data.Table.Schema, data.Table.Name));

            var selected = _selector.Choose(choices, _formatter);

            var selected_output = selected
                .Select(choice =>
                {
                    var table = data.Tables[choice.Table.Id];
                    return $"[{table.Schema}].[{table.Name}].[{choice.Column.Name}]";
                })
                .Sentence(", ", "none found");

            _stream.WriteLine($"Adding {selected.Count()} columns to the selection ({selected_output})");
            _stream.Padding();

            var new_parameters = new SqlQueryParameters
            {
                Tables = parameters.Tables,
                Filters = parameters.Filters
                    .UnionBy(selected.Select(s => s.Column), (filter) => (filter.TableId, filter.ColumnId))
                    .ToList(),
            };

            return (HandlerResult.NEXT_COMMAND, data, new_parameters);
        }
    }
}

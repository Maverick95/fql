﻿using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Text.RegularExpressions;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class AddFiltersCommandHandler : ICommandHandler
    {
        private readonly IStream _stream;

        public AddFiltersCommandHandler(IStream stream)
        {
            _stream = stream;
        }

        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            const int padding = 3;

            var cleaned = input.Clean();
            var rgx_filter = new Regex("^(f|filter) ");
            var match = rgx_filter.Match(cleaned);

            if (match.Success == false)
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

            if (matches.Any() == false)
            {
                _stream.WriteLine("filter command contains no matches, please try again");
                _stream.Padding();
                return (HandlerResult.NEXT_COMMAND, data, parameters);
            }

            var options_data = matches
                .Select(match => new
                {
                    Table = data.Tables[match.Key.TableId],
                    Column = match.Value,
                })
                .OrderBy(data => (data.Column.Name, data.Table.Schema, data.Table.Name));

            var column_max_length =
                options_data.Max(data => data.Column.Name.Length);

            var schema_max_length =
                options_data.Max(data => data.Table.Schema.Length);

            var column_space = column_max_length + padding;
            var schema_space = schema_max_length + padding + 1; // Extra space for the . separator

            var id_space = matches.Count().ToString().Length + padding;
            var ids = Enumerable.Range(1, matches.Count());

            var options = ids.Zip(options_data, (id, option) => new
            {
                Id = id,
                Column = option.Column,
                Text =
                    $"{id}".PadRight(id_space) +
                    $"{option.Column.Name}".PadRight(column_space) +
                    $"{option.Table.Schema}.".PadRight(schema_space) +
                    $"{option.Table.Name}",
            });

            foreach (var option in options)
            {
                _stream.WriteLine(option.Text);
            }
            _stream.Padding();
            _stream.Write("Enter selection(s) : ");
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
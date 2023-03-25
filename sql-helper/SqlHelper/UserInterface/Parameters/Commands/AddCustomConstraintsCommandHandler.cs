using SqlHelper.Config;
using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class AddCustomConstraintsCommandHandler : ICommandHandler
    {
        private readonly IUniqueIdProvider _uniqueIdProvider;
        private readonly IStream _stream;
        private readonly IConfigManager _config;
        private readonly bool _saveConfig;
        private readonly string _alias;

        public AddCustomConstraintsCommandHandler(
            IUniqueIdProvider uniqueIdProvider,
            IStream stream,
            IConfigManager config,
            bool saveConfig,
            string alias)
        {
            _uniqueIdProvider = uniqueIdProvider;
            _stream = stream;
            _config = config;
            _saveConfig = saveConfig;
            _alias = alias;
        }

        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            const int padding = 3;

            var cleaned = input.Clean();
            var inputOptions = new[] { "c", "constraint" };
            
            if (inputOptions.Contains(cleaned) == false)
            {
                return (HandlerResult.NEXT_HANDLER, data, parameters);
            }

            var columns = data.Columns.Select(kv => kv.Value);
            var constraints = data.Constraints.Select(kv => kv.Value);

            var primaryKeyColumns = columns
                .Where(column => column.IsPrimaryKey)
                .GroupBy(column => column.TableId);

            var primaryKeyMatchingColumns = columns
                .Where(column => column.IsPrimaryKey)
                .Join(
                    columns,
                    c1 => new { c1.Name, c1.Type },
                    c2 => new { c2.Name, c2.Type },
                    (c1, c2) => new { SourceColumn = c1, TargetColumn = c2 }
                    )
                // We are looking for matches across tables
                .Where(c => c.SourceColumn.TableId != c.TargetColumn.TableId)
                // Constraint cannot already exist between source / target table
                .Where(c => constraints.Any(constraint =>
                    constraint.SourceTableId == c.SourceColumn.TableId &&
                    constraint.TargetTableId == c.TargetColumn.TableId) == false)
                .GroupBy(columns => new
                {
                    SourceTableId = columns.SourceColumn.TableId,
                    TargetTableId = columns.TargetColumn.TableId
                });

            var customConstraints = primaryKeyColumns
                .Join(
                    primaryKeyMatchingColumns,
                    pk => pk.Key,
                    pkm => pkm.Key.SourceTableId,
                    (pk, pkm) => new { PrimaryKeyColumns = pk, PrimaryKeyMatchingColumns = pkm }
                    )
                // This is the best attempt at a condition we have currently
                .Where(pk => pk.PrimaryKeyColumns.Count() == pk.PrimaryKeyMatchingColumns.Count())
                .Select(pk => new Constraint
                {
                    Id = _uniqueIdProvider.Next(),
                    IsCustom = true,
                    TargetTableId = pk.PrimaryKeyMatchingColumns.Key.TargetTableId,
                    SourceTableId = pk.PrimaryKeyMatchingColumns.Key.SourceTableId,
                    Columns = pk.PrimaryKeyMatchingColumns.Select(columns => new ConstraintColumnPair
                    {
                        SourceColumnId = columns.SourceColumn.ColumnId,
                        TargetColumnId = columns.TargetColumn.ColumnId,
                    }).ToList(),
                });

            if (customConstraints.Any() == false)
            {
                _stream.WriteLine("constraint command contains no matches, please try again");
                _stream.Padding();
                return (HandlerResult.NEXT_COMMAND, data, parameters);
            }

            var id_space = customConstraints.Count().ToString().Length + padding;
            var ids = Enumerable.Range(1, customConstraints.Count());

            var options_data = customConstraints
                .Select(constraint => new
                {
                    Constraint = constraint,
                    SourceTable = data.Tables[constraint.SourceTableId],
                    TargetTable = data.Tables[constraint.TargetTableId],
                })
                .OrderBy(data => (data.SourceTable.Schema, data.SourceTable.Name, data.TargetTable.Schema, data.TargetTable.Name));

            var source_table_max_length = options_data
                .Select(data => $"{data.SourceTable.Schema}.{data.SourceTable.Name}")
                .Max(name => name.Length);

            var source_table_space = source_table_max_length + padding;

            var options = ids.Zip(options_data, (id, option) => new
            {
                Id = id,
                Constraint = option.Constraint,
                Text =
                    $"{id}".PadRight(id_space) +
                    $"{option.SourceTable.Schema}.{option.SourceTable.Name}".PadRight(source_table_space) +
                    $"<----".PadRight(5 + padding) +
                    $"{option.TargetTable.Schema}.{option.TargetTable.Name}",
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
                    (input, option) => option.Constraint);

            _stream.WriteLine($"Adding {selected.Count()} constraints to the data-set");
            _stream.Padding();

            var new_constraints = new SortedDictionary<long, Constraint>(data.Constraints);
            foreach (var s in selected)
            {
                new_constraints.Add(s.Id, s);
            }

            var new_data = new DbData
            {
                Tables = data.Tables,
                Columns = data.Columns,
                Constraints = new_constraints,
            };

            if (_saveConfig)
            {
                _config.Write(_alias, new_data);
            }

            return (HandlerResult.NEXT_COMMAND, new_data, parameters);
        }
    }
}

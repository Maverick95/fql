using SqlHelper.Models;

namespace SqlHelper.Helpers
{
    public static class DbDataHelpers
    {
        public static DbData TryMergeDbDataCustomConstraints(DbData dataFrom, DbData dataTo, IUniqueIdProvider uniqueIdProvider)
        {
            // Transform dataFrom custom constraints
            var customConstraintsFrom = dataFrom.Constraints.Select(kv => kv.Value)
                .Where(constraint => constraint.IsCustom)
                .Select(constraint =>
                {
                    var targetTable = dataFrom.Tables[constraint.TargetTableId];
                    var sourceTable = dataFrom.Tables[constraint.SourceTableId];

                    return new
                    {
                        TargetSchema = targetTable.Schema,
                        TargetName = targetTable.Name,
                        SourceSchema = sourceTable.Schema,
                        SourceName = sourceTable.Name,

                        Columns = constraint.Columns
                            .Select(column =>
                            {
                                var targetColumn = dataFrom.Columns[(constraint.TargetTableId, column.TargetColumnId)];
                                var sourceColumn = dataFrom.Columns[(constraint.SourceTableId, column.SourceColumnId)];

                                return new
                                {
                                    TargetType = targetColumn.Type,
                                    TargetName = targetColumn.Name,
                                    SourceType = sourceColumn.Type,
                                    SourceName = sourceColumn.Name,
                                };
                            }),
                    };
                });

            var newConstraintsTo = new SortedDictionary<long, Constraint>(dataTo.Constraints);

            // Try each constraint in turn
            foreach (var try_constraint in customConstraintsFrom)
            {
                // Source / target tables must exist in dataTo
                var sourceTableTo = dataTo.Tables.Select(kv => kv.Value)
                    .SingleOrDefault(table =>
                        table.Schema == try_constraint.SourceSchema &&
                        table.Name == try_constraint.SourceName);

                var targetTableTo = dataTo.Tables.Select(kv => kv.Value)
                    .SingleOrDefault(table =>
                        table.Schema == try_constraint.TargetSchema &&
                        table.Name == try_constraint.TargetName);

                if (sourceTableTo is null || targetTableTo is null) continue;

                // No constraint between source / target tables in dataTo
                var constraintExists = dataTo.Constraints.Select(kv => kv.Value)
                    .Any(constraint =>
                        constraint.SourceTableId == sourceTableTo.Id &&
                        constraint.TargetTableId == targetTableTo.Id);

                if (constraintExists) continue;

                // Columns must exist in dataTo
                var columnPairsTo = try_constraint.Columns
                    .Select(column =>
                    {
                        var sourceColumn = dataTo.Columns.Select(kv => kv.Value)
                            .SingleOrDefault(sc =>
                                sc.TableId == sourceTableTo.Id &&
                                sc.Name == column.SourceName &&
                                sc.Type == column.SourceType);

                        var targetColumn = dataTo.Columns.Select(kv => kv.Value)
                            .SingleOrDefault(tc =>
                                tc.TableId == targetTableTo.Id &&
                                tc.Name == column.TargetName &&
                                tc.Type == column.TargetType);

                        return (sourceColumn, targetColumn);
                    });

                var allColumnPairsExist = columnPairsTo
                    .All(columnPair =>
                        columnPair.sourceColumn is not null &&
                        columnPair.targetColumn is not null);

                if (allColumnPairsExist == false) continue;

                // Target columns are all primary key columns
                var allSourceColumnsArePrimary = columnPairsTo
                    .All(columnPair =>
                        columnPair.sourceColumn.IsPrimaryKey);

                if (allSourceColumnsArePrimary == false) continue;

                // Constraint covers entire Source primary key
                var sourcePrimaryKeyCount = dataTo.Columns.Select(kv => kv.Value)
                    .Count(column =>
                        column.TableId == sourceTableTo.Id &&
                        column.IsPrimaryKey);

                if (columnPairsTo.Count() != sourcePrimaryKeyCount) continue;

                // Constraint is valid
                var id = uniqueIdProvider.Next();
                newConstraintsTo.Add(id, new Constraint
                {
                    Id = id,
                    IsCustom = true,
                    TargetTableId = targetTableTo.Id,
                    SourceTableId = sourceTableTo.Id,
                    Columns = columnPairsTo
                        .Select(column => new ConstraintColumnPair
                        {
                            SourceColumnId = column.sourceColumn.ColumnId,
                            TargetColumnId = column.targetColumn.ColumnId,
                        })
                        .ToList(),
                });
            }

            var newDataTo = new DbData
            {
                Tables = dataTo.Tables,
                Columns = dataTo.Columns,
                Constraints = newConstraintsTo,
            };

            return newDataTo;
        }
    }
}

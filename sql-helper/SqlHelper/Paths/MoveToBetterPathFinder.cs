using SqlHelper.Models;
using SqlHelper.Helpers;

namespace SqlHelper.Paths
{
    public class MoveToBetterPathFinder: IPathFinder
    {
        private void HelpInternalRecursive(
            DbData graph,
            IEnumerable<long> tablesRequired,
            Dictionary<long, bool> constraintsUsed,
            Dictionary<long, List<long>> constraintsByTargetTable,
            Stack<Table> tablesPath,
            Stack<Constraint> constraintsPath,
            IList<SqlHelperResult> results,
            long tableId)
        {
            var trLookup = graph.Tables[tableId];
            tablesPath.Push(new Table
            {
                Id = trLookup.Id,
                Schema = trLookup.Schema,
                Name = trLookup.Name,
            });

            // Maybe articulate on this.
            var isFinished =
                tablesPath.Count > 1 &&
                tablesRequired.Any(id => id == tableId);

            if (isFinished)
            {
                // Stacks store the journey in reverse-order.
                var start = tablesPath.Last();
                var tables = tablesPath.SkipLast(1).Reverse();
                var constraints = constraintsPath.Reverse();

                (long targetTableId, long sourceTableId) key = (start.Id, tables.Last().Id);

                var newSqlHelperResult = new SqlHelperResult
                {
                    Start = start,
                    Paths = tables.Zip(constraints, (table, constraint) => new SqlHelperResultPath
                    {
                        Table = table,
                        Constraint = constraint,
                    }).ToList(),
                };
                
                results.Add(newSqlHelperResult);
            }
            else if (constraintsByTargetTable.ContainsKey(tableId))
            {
                var constraints = constraintsByTargetTable[tableId].Where(c => constraintsUsed[c] == false).ToList();
                foreach (var con in constraints)
                {
                    constraintsUsed[con] = true;
                    var conLookup = graph.Constraints[con];
                    constraintsPath.Push(new Constraint
                    {
                        Id = conLookup.Id,
                        TargetTableId = conLookup.TargetTableId,
                        SourceTableId = conLookup.SourceTableId,
                        Columns = conLookup.Columns,
                    });

                    HelpInternalRecursive(
                        graph,
                        tablesRequired,
                        constraintsUsed,
                        constraintsByTargetTable,
                        tablesPath,
                        constraintsPath,
                        results,
                        tableId: conLookup.SourceTableId);

                    constraintsUsed[con] = false;
                    constraintsPath.Pop();
                }
            }
            tablesPath.Pop();
        }

        public IList<SqlHelperResult> Help(DbData graph, IList<long> tables)
        {
            var results = new List<SqlHelperResult>();

            // Below is lifted by FirstStupidPathFinder, I think I can do something with this.

            var tablesRequired = tables.Distinct();

            var constraintsUsed = graph.Constraints.ToDictionary(
                keySelector: c => c.Key,
                elementSelector: _ => false);

            var constraintsByTargetTable = graph.Constraints.GroupBy(c => c.Value.TargetTableId).ToDictionary(
                keySelector: grp => grp.Key,
                elementSelector: grp => grp.Select(c => c.Key).ToList());

            var tablesPath = new Stack<Table>();
            var constraintsPath = new Stack<Constraint>();

            foreach (var tr in tablesRequired)
            {
                HelpInternalRecursive(
                    graph,
                    tablesRequired,
                    constraintsUsed,
                    constraintsByTargetTable,
                    tablesPath,
                    constraintsPath,
                    results,
                    tableId: tr);
            }

            // Generate the iterator to use these connections.
            var count = results.Count;
            // If there are N tables, a valid result must contain at least N - 1 paths between tables.
            var choose = tablesRequired.Count() - 1;
            var iterator = IListChooseElementIterator.GetEnumerable(count, choose);

            var nSolutions = 0;

            foreach(var indices in iterator)
            {
                var paths = indices
                    .Select(i => results[i]);

                var sourceTables = paths
                    .Select(p => p.Paths.Last().Table.Id);

                // Valid results contain 0 or 1 tables that are NOT listed as a source.
                var parentTableCount = tablesRequired
                    .GroupJoin(
                        sourceTables,
                        id => id, id => id,
                        (id, tables) => tables.Any()
                    )
                    .Count(any => any == false);

                if (parentTableCount < 2)
                {
                    // This is really wrong for now.
                    //return paths.ToList();
                    Console.WriteLine($"Solution number {++nSolutions}");
                }
            }

            // This is really wrong for now.
            return null;
        }
    }
}

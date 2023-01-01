using SqlHelper.Models;

namespace SqlHelper.Paths
{
    public class FirstStupidPathFinder : IPathFinder
    {
        /* Okay so this class is named after what it is. A first stupid dumb attempt.
         * But hey it works!
         * But it is pretty stupid and inefficient.
         * It looks for paths across the DbData model that include all required tables.
         * It does this by considering each required table separately.
         * Beginning at this table,
         * it expands greedily by following constraints from target -> source
         * For each table it touches, it checks if all the required tables are in the path,
         * if so this path is recorded.
         * So yeah it's pretty dumb.
         * BUT it only follows constraints one-way, so there are no duplicates.
         * You are allowed to visit the same table more than once,
         * BUT each constraint can only be used once.
         */

        private void HelpInternalRecursive(
            DbData graph,
            Dictionary<long, bool> tablesFound,
            Dictionary<long, bool> constraintsFound,
            Dictionary<long, List<long>> constraintsByTargetTable,
            Stack<Table> tablesPath,
            Stack<Constraint> constraintsPath,
            IList<SqlHelperResult> results,
            long tableId)
        {
            if (tablesFound.ContainsKey(tableId))
            {
                tablesFound[tableId] = true;
            }
            var trLookup = graph.Tables[tableId];
            tablesPath.Push(new Table
            {
                Id = trLookup.Id,
                Schema = trLookup.Schema,
                Name = trLookup.Name,
            });

            var isFinished = tablesFound.All(kv => kv.Value == true);

            if (isFinished)
            {
                // Stacks store the journey in reverse-order.
                var start = tablesPath.Last();
                var tables = tablesPath.SkipLast(1).Reverse();
                var constraints = constraintsPath.Reverse();

                results.Add(
                    new SqlHelperResult
                    {
                        Start = start,
                        Paths = tables.Zip(constraints, (table, constraint) => new SqlHelperResultPath
                        {
                            Table = table,
                            Constraint = constraint,
                        }).ToList(),
                    });
            }
            else if (constraintsByTargetTable.ContainsKey(tableId))
            {
                var constraints = constraintsByTargetTable[tableId].Where(c => constraintsFound[c] == false).ToList();
                foreach (var con in constraints)
                {
                    constraintsFound[con] = true;
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
                        tablesFound,
                        constraintsFound,
                        constraintsByTargetTable,
                        tablesPath,
                        constraintsPath,
                        results,
                        tableId: conLookup.SourceTableId);

                    constraintsFound[con] = false;
                    constraintsPath.Pop();
                }
            }

            if (tablesFound.ContainsKey(tableId))
            {
                tablesFound[tableId] = false;
            }
            tablesPath.Pop();
        }

        public IList<SqlHelperResult> Help(DbData graph, IList<long> tables)
        {
            var tablesRequired = tables.Distinct();

            var tablesFound = tablesRequired.ToDictionary(
                keySelector: id => id,
                elementSelector: _ => false);

            var constraintsFound = graph.Constraints.ToDictionary(
                keySelector: c => c.Key,
                elementSelector: _ => false);

            var constraintsByTargetTable = graph.Constraints.GroupBy(c => c.Value.TargetTableId).ToDictionary(
                keySelector: grp => grp.Key,
                elementSelector: grp => grp.Select(c => c.Key).ToList());

            var tablesPath = new Stack<Table>();
            var constraintsPath = new Stack<Constraint>();

            var results = new List<SqlHelperResult>();

            foreach (var tr in tablesRequired)
            {
                HelpInternalRecursive(
                    graph,
                    tablesFound,
                    constraintsFound,
                    constraintsByTargetTable,
                    tablesPath,
                    constraintsPath,
                    results,
                    tableId: tr);
            }

            return results;
        }
    }
}

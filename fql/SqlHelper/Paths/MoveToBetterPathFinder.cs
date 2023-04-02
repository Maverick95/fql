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
            IList<ResultRoute> results,
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

                var newRoute = new ResultRoute
                {
                    Start = start,
                    Route = tables.Zip(constraints, (table, constraint) =>
                    {
                        (Table source, Constraint constraint) link = (table, constraint);
                        return link;
                    }).ToList(),
                };
                
                results.Add(newRoute);
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

        private class TableDepthDataComparer: IComparer<TableDepthData>
        {
            public int Compare(TableDepthData x, TableDepthData y)
            {
                if (x.Depth < y.Depth)
                    return -1;
                if (x.Depth > y.Depth)
                    return 1;
                if (x.TreeId < y.TreeId)
                    return -1;
                if (x.TreeId > y.TreeId)
                    return 1;
                return 0;
            }
        }

        private class TableDepthData
        {
            public long TableId { get; set; }
            public int Depth { get; set; }
            public int TreeId { get; set; }
        }

        private ResultRouteTree MergeDepthFirst(IEnumerable<ResultRouteTree> trees)
        {
            var treeData = new List<(int treeId, ResultRouteTree tree)>();
            var treeMergeData = new List<TableDepthData>();

            var treeId = 0;

            var pathInitiator = (ResultRouteTree rootTree) =>
            {
                var rootTreeId = treeId;
                var rootDepth = 0;

                treeData.Add((rootTreeId, rootTree));
                treeMergeData.Add(new TableDepthData
                {
                    TableId = rootTree.Table.Id,
                    Depth = rootDepth,
                    TreeId = rootTreeId
                });

                treeId++;
                return (rootTreeId, rootTree, rootDepth);
            };

            var pathGenerator = ((int, ResultRouteTree, int) parent, ResultRoute _, ResultRouteTree childTree) =>
            {
                (var rootTreeId, var rootTree, var parentDepth) = parent;
                var childDepth = parentDepth + 1;

                treeMergeData.Add(new TableDepthData
                {
                    TableId = childTree.Table.Id,
                    Depth = childDepth,
                    TreeId = rootTreeId
                });

                return (rootTreeId, rootTree, childDepth);
            };

            trees.ToList().ForEach(tree => ResultRouteTreeHelpers.EnumerateTreeDepthFirst(tree, pathInitiator, pathGenerator));

            var comparer = new TableDepthDataComparer();
            
            while (treeData.Count() > 1)
            {
                var nextTree = treeData.First();
                treeData.RemoveAt(0);
                
                var treeMergeLookup = treeMergeData
                    .Where(tmd => tmd.TableId == nextTree.tree.Table.Id)
                    .Where(tmd => tmd.TreeId != nextTree.treeId)
                    .Min(comparer);

                if (treeMergeLookup is not null)
                {
                    var treeMerge = treeData.Single(tree => tree.treeId == treeMergeLookup.TreeId);
                    ResultRouteTreeHelpers.TryMergeTreesFromRoot(treeMerge.tree, nextTree.tree);
                    var updates = treeMergeData
                        .Where(tmd => tmd.TreeId == nextTree.treeId);

                    foreach(var update in updates)
                    {
                        update.TreeId = treeMergeLookup.TreeId;
                        update.Depth = update.Depth + treeMergeLookup.Depth;
                    }
                }
                else
                {
                    treeData.Add(nextTree);
                }
            }

            return treeData.First().tree;
        }

        public IEnumerable<ResultRouteTree> Help(DbData graph, IList<long> tables)
        {
            var results = new List<ResultRoute>();

            var tablesRequired = tables.Distinct();

            // Handle easy case.
            if (tablesRequired.Count() == 1)
            {
                var rootTable = graph.Tables[tablesRequired.Single()];
                yield return ResultRouteTreeHelpers.CreateTreeFromTable(rootTable);
                yield break;
            }

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
            var minChoose = tablesRequired.Count() - 1;

            for (var choose = minChoose; choose <= count; choose++)
            {
                var iterator = IListChooseElementIterator.GetElementCombinationsWithChooseElements(count, choose);
                foreach(var indices in iterator)
                {
                    var routes = indices
                        .Select(i => results[i]);

                    // Must contain at most 1 table that is NOT listed as the source for a distinct target.
                    var sourceTables = routes
                        .Where(r => r.Start.Id != r.Route.Last().source.Id)
                        .Select(p => p.Route.Last().source.Id);

                    var parentTableCount = tablesRequired
                        .GroupJoin(
                            sourceTables,
                            id => id, id => id,
                            (id, tables) => tables.Any()
                        )
                        .Count(any => any == false);

                    if (parentTableCount < 2)
                    {
                        var trees = routes.Select(route => ResultRouteTreeHelpers.CreateTreeFromRoute(route));
                        var result = MergeDepthFirst(trees);
                        yield return result;
                    }
                }
            }
        }
    }
}

using SqlHelper.Models;

namespace SqlHelper.Helpers
{
    public static class ResultRouteTreeHelpers
    {
        /*
         * Try to test CreateTree functions instead of constructing trees by hand,
         * except for in tests.
         */ 
        public static ResultRouteTree CreateTreeFromTable(Table table)
        {
            return new ResultRouteTree
            {
                Table = table,
                Children = new List<(ResultRoute, ResultRouteTree)> { },
            };
        }

        public static ResultRouteTree CreateTreeFromRoute(ResultRoute route)
        {
            return new ResultRouteTree
            {
                Table = route.Start,
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        route,
                        new ResultRouteTree
                        {
                            Table = route.Route.Last().source,
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    ),
                },
            };
        }

        public static bool TryMergeTreesFromRoot(ResultRouteTree master, ResultRouteTree incoming)
        {
            var queue = new Queue<ResultRouteTree>();
            queue.Enqueue(master);
            while (queue.Any())
            {
                var next = queue.Dequeue();
                if (next.Table.Id == incoming.Table.Id)
                {
                    foreach (var child in incoming.Children)
                    {
                        next.Children.Add(child);
                    }
                    return true;
                }
                foreach (var child in next.Children)
                {
                    queue.Enqueue(child.child);
                }
            }

            return false;
        }

        /*
         * A quick note about this function.
         * At first glance it seems it doesn't do anything.
         * The idea is that initiator / generator have side-effects.
         */
        public static void EnumerateTreeDepthFirst<T>(
            ResultRouteTree master,
            Func<ResultRouteTree, T> initiator,
            Func<T, ResultRoute, ResultRouteTree, T> generator)
        {
            var elements_depth_first = new Stack<(ResultRouteTree, T)>();
            var root = (master, initiator(master));
            elements_depth_first.Push(root);

            while (elements_depth_first.Any())
            {
                (var tree, var transform) = elements_depth_first.Pop();
                foreach (var child in tree.Children)
                {
                    var next = (child.child, generator(transform, child.route, child.child));
                    elements_depth_first.Push(next);
                }
            }
        }
    }
}

using SqlHelper.Models;

namespace SqlHelper.Paths
{
    public class ResultRoute
    {
        public Table Start { get; set; }
        public IList<(Table source, Constraint constraint)> Route { get; set; }
    }

    public class ResultRouteTree
    {
        public Table Table { get; private set; }

        public IList<(ResultRoute route, ResultRouteTree child)> Children { get; private set; }

        public bool TryMergeFromRoot(ResultRouteTree incoming)
        {
            var queue = new Queue<ResultRouteTree>();
            queue.Enqueue(this);
            while (queue.Any())
            {
                var next = queue.Dequeue();
                if (next.Table.Id == incoming.Table.Id)
                {
                    foreach(var child in incoming.Children)
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

        public ResultRouteTree(Table table)
        {
            Table = table;
            Children = new List<(ResultRoute, ResultRouteTree)>();
        }

        public ResultRouteTree(ResultRoute route)
        {
            Table = route.Start;
            Children = new List<(ResultRoute, ResultRouteTree)>();

            var child = new ResultRouteTree(route.Route.Last().source);
            Children.Add((route, child));
        }

        /*
         * A quick note about this function.
         * At first glance it seems it doesn't do anything.
         * The idea is that initiator / generator have side-effects.
         */
        public void EnumerateDepthFirst<T>(
            Func<ResultRouteTree, T> initiator,
            Func<T, ResultRoute, ResultRouteTree, T> generator)
        {
            var elements_depth_first = new Stack<(ResultRouteTree, T)>();
            var root = (this, initiator(this));
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

    public interface IPathFinder
    {
        public IEnumerable<ResultRouteTree> Help(DbData graph, IList<long> tables);
    }
}

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

        public ResultRouteTree Parent { get; private set; }

        public IList<(ResultRoute route, ResultRouteTree child)> Children { get; private set; }

        public bool IsRoot
        {
            get => Parent is null;
        }

        public bool IsLeaf
        {
            get => Children is null || Children.Any() == false;
        }

        private IEnumerable<(int depth, Table table)> DepthsInternal(int initialDepth = 0)
        {
            var depths = Children?.Select(c => c.child)
                .SelectMany(child => child.DepthsInternal(initialDepth + 1))
                .ToList() ?? new();

            depths.Add((initialDepth, Table));
            return depths;
        }

        public IEnumerable<(int depth, Table table)> Depths
        {
            get => DepthsInternal();
        }

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

        public ResultRouteTree() { }

        public ResultRouteTree(ResultRoute route)
        {
            Table = route.Start;
            Children = new List<(ResultRoute route, ResultRouteTree child)>();

            var child = new ResultRouteTree();
            child.Parent = this;
            child.Table = route.Route.Last().source;

            Children.Add((route, child));
        }
    }

    public interface IPathFinder
    {
        public IEnumerable<ResultRouteTree> Help(DbData graph, IList<long> tables);
    }
}

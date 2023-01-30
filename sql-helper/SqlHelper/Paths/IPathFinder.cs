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
        public IList<ResultRouteTree> Help(DbData graph, IList<long> tables);
    }
}

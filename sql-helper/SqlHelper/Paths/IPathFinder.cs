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
        public Table Table { get; set; }

        public ResultRouteTree Parent { get; set; }

        public IList<(ResultRoute, ResultRouteTree)> Children { get; set; }

        public bool IsRoot
        {
            get => Parent is null;
        }

        public bool IsLeaf
        {
            get => Children is null || Children.Any() == false;
        }
    }

    public interface IPathFinder
    {
        public IList<ResultRoute> Help(DbData graph, IList<long> tables);
    }
}

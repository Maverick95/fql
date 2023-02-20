using SqlHelper.Models;

namespace SqlHelper.Paths
{
    public interface IPathFinder
    {
        public IEnumerable<ResultRouteTree> Help(DbData graph, IList<long> tables);
    }
}

using SqlHelper.Models;

namespace SqlHelper.Paths
{
    public interface IPathFinderVersionTree
    {
        public IList<ResultRouteTree> Help(DbData graph, IList<long> tables);
    }
}

using SqlHelper.Models;

namespace SqlHelper.Paths
{
    public class SqlHelperResultPath
    {
        public Table Table { get; set; }
        public Constraint Constraint { get; set; }
    }

    public class SqlHelperResult
    {
        public Table Start { get; set; }
        public IList<SqlHelperResultPath> Paths { get; set; }
    }

    public interface IPathFinder
    {
        public IList<SqlHelperResult> Help(DbData graph, IList<long> tables);
    }
}

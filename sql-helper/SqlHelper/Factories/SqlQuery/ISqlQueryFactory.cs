using SqlHelper.Models;
using SqlHelper.Paths;

namespace SqlHelper.Factories.SqlQuery
{
    public interface ISqlQueryFactory
    {
        public string Generate(Models.DbData graph, SqlHelperResult result, SqlQueryParameters parameters);
    }
}

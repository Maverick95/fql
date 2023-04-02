using SqlHelper.Models;

namespace SqlHelper.Factories.SqlQuery
{
    public interface ISqlQueryFactory
    {
        public string Generate(Models.DbData data, ResultRouteTree result, SqlQueryParameters parameters);
    }
}

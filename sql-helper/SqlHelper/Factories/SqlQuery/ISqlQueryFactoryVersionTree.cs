﻿using SqlHelper.Models;
using SqlHelper.Paths;

namespace SqlHelper.Factories.SqlQuery
{
    public interface ISqlQueryFactoryVersionTree
    {
        public string Generate(Models.DbData graph, ResultRouteTree result, SqlQueryParameters parameters);
    }
}

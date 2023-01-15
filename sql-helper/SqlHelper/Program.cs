using SqlHelper.Factories.DbData;
using SqlHelper.Factories.DefaultTypeValue;
using SqlHelper.Factories.SqlQuery;
using SqlHelper.Factories.TableAlias;
using SqlHelper.Helpers;
using SqlHelper.Paths;
using SqlHelper.UserInterface.Parameters;
using SqlHelper.UserInterface.Path;

namespace SqlHelper
{
    public class Program
    {
        private class Solution
        {
            private readonly IDbDataFactory _dbDataFactory;
            private readonly IPathFinder _pathFinder;
            private readonly ISqlQueryFactory _sqlQueryFactory;
            private readonly IParameterUserInterface _parameterUserInterface;
            private readonly IPathUserInterface _pathUserInterface;

            public Solution(
                IDbDataFactory dbDataFactory,
                IPathFinder pathFinder,
                ISqlQueryFactory sqlQueryFactory,
                IParameterUserInterface parameterUserInterface,
                IPathUserInterface pathUserInterface)
            {
                _dbDataFactory = dbDataFactory;
                _pathFinder = pathFinder;
                _sqlQueryFactory = sqlQueryFactory;
                _parameterUserInterface = parameterUserInterface;
                _pathUserInterface = pathUserInterface;
            }

            public void Solve()
            {
                var data = _dbDataFactory.Create();

                var parameters = _parameterUserInterface.GetParameters(data);
                
                var tables = parameters.Tables
                    .Select(table => table.Id)
                    .Union(parameters.Filters.Select(filter => filter.TableId))
                    .ToList();

                var paths = _pathFinder.Help(data, tables);

                if (paths.Any() == false)
                {
                    Console.Write("No output to generate!");
                    return;
                }
                var path = paths.Count == 1 ?
                    paths.First() :
                    _pathUserInterface.Choose(paths);
                
                var output = _sqlQueryFactory.Generate(data, path, parameters);
                Console.Write(output);
            }
        }

        static void Main(string[] args)
        {
            IDbDataFactory dbDataFactory = new LocalSqlExpressDbDataFactory(args[0]);

            IPathFinder pathFinder = new FirstStupidPathFinder();

            ISqlQueryFactory sqlQueryFactory = new PrettierSqlQueryFactory(
                new FullyQualifiedTableAliasFactory(),
                new FirstDefaultTypeValueFactory(),
                5);

            IParameterUserInterface parameterUserInterface = new FirstParameterUserInterface(new ConsoleStream());

            IPathUserInterface pathUserInterface = new ChoosePathUserInterface(new ConsoleStream());

            var solution = new Solution(
                dbDataFactory,
                pathFinder,
                sqlQueryFactory,
                parameterUserInterface,
                pathUserInterface);

            solution.Solve();
        }
    }
}
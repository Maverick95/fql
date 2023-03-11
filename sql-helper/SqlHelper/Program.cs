using CommandLine;
using SqlHelper.Factories.DbData;
using SqlHelper.Factories.DefaultTypeValue;
using SqlHelper.Factories.SqlQuery;
using SqlHelper.Factories.TableAlias;
using SqlHelper.Helpers;
using SqlHelper.Models;
using SqlHelper.Output;
using SqlHelper.Paths;
using SqlHelper.UserInterface.Parameters;
using SqlHelper.UserInterface.Path;

namespace SqlHelper
{
    public class Program
    {
        static void Main(string[] args)
        {
            IStream stream = new ConsoleStream();

            var parserResult = Parser.Default.ParseArguments<Options>(args);

            if (parserResult.Tag is ParserResultType.NotParsed)
            {
                stream.WriteLine("Failed to parse arguments. Exiting...");
                return;
            }

            var options = parserResult.Value;

            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                stream.WriteLine("Failed to supply connection string. Exiting...");
                return;
            }

            IDbDataFactory dbDataFactory = new ConnectionStringDbDataFactory(options.ConnectionString);

            IPathFinder pathFinder = new MoveToBetterPathFinder();

            ISqlQueryFactory sqlQueryFactory = new MoveToBetterPrettierSqlQueryFactory(
                new FullyQualifiedTableAliasFactory(),
                new FirstDefaultTypeValueFactory(),
                padding: 5);

            IParameterUserInterface parameterUserInterface = new FirstParameterUserInterface(stream);

            IPathUserInterface pathUserInterface = new MoveToBetterPathUserInterface(stream);

            IOutputHandler outputHandler = new PrintToConsoleOutputHandler(stream);

            var data = dbDataFactory.Create();
            var parameters = parameterUserInterface.GetParameters(data);

            var tables = parameters.Tables
                .Select(table => table.Id)
                .Union(parameters.Filters.Select(filter => filter.TableId))
                .ToList();

            var paths = pathFinder.Help(data, tables);

            if (paths.Any() == false)
            {
                Console.Write("No output to generate!");
                return;
            }

            var path = paths.Count() == 1 ?
                paths.First() :
                pathUserInterface.Choose(paths);

            var output = sqlQueryFactory.Generate(data, path, parameters);
            outputHandler.Handle(output);
        }
    }
}
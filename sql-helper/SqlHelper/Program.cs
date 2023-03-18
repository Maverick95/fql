using CommandLine;
using SqlHelper.Config;
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
            IFileManager fileManager = new FileManager();
            IConfigManager configManager = new AppResourceConfigManager(fileManager);

            var parserResult = Parser.Default.ParseArguments<Options>(args);

            if (parserResult.Tag is ParserResultType.NotParsed)
            {
                stream.WriteLine("Failed to parse arguments. Exiting...");
                return;
            }

            var options = parserResult.Value;

            if (string.IsNullOrEmpty(options.ConnectionString) && string.IsNullOrEmpty(options.Alias))
            {
                stream.WriteLine("Failed to supply Connection String or Alias. Exiting...");
                return;
            }

            DbData data;

            if (string.IsNullOrEmpty(options.ConnectionString) == false)
            {
                IDbDataFactory dbDataFactory = new ConnectionStringDbDataFactory(options.ConnectionString);
                data = dbDataFactory.Create();

                if (string.IsNullOrEmpty(options.Alias) == false)
                {
                    configManager.Write(options.Alias, data);
                }
            }
            else
            {
                (var exists, data) = configManager.Read(options.Alias);
                if (exists == false)
                {
                    stream.WriteLine("Failed to supply valid Alias. Exiting...");
                    return;
                }
            }

            IPathFinder pathFinder = new MoveToBetterPathFinder();

            ISqlQueryFactory sqlQueryFactory = new MoveToBetterPrettierSqlQueryFactory(
                new FullyQualifiedTableAliasFactory(),
                new FirstDefaultTypeValueFactory(),
                padding: 5);

            IParameterUserInterface parameterUserInterface = new FirstParameterUserInterface(stream);
            IPathUserInterface pathUserInterface = new MoveToBetterPathUserInterface(stream);
            IOutputHandler outputHandler = new PrintToConsoleOutputHandler(stream);

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
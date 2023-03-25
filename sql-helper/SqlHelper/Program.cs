using CommandLine;
using SqlHelper.Contexts;
using SqlHelper.Factories.DbData;
using SqlHelper.Factories.DefaultTypeValue;
using SqlHelper.Factories.SqlQuery;
using SqlHelper.Factories.TableAlias;
using SqlHelper.Models;
using SqlHelper.Output;
using SqlHelper.Paths;
using SqlHelper.UserInterface.Parameters;
using SqlHelper.UserInterface.Parameters.Commands;
using SqlHelper.UserInterface.Path;

namespace SqlHelper
{
    public static class Program
    {
        static void Main(string[] args)
        {   
            var parserResult = Parser.Default.ParseArguments<Options>(args);

            if (parserResult.Tag is ParserResultType.NotParsed)
            {
                Context.Stream.WriteLine("Failed to parse arguments. Exiting...");
                return;
            }

            var options = parserResult.Value;

            if (string.IsNullOrEmpty(options.ConnectionString) && string.IsNullOrEmpty(options.Alias))
            {
                Context.Stream.WriteLine("Failed to supply Connection String or Alias. Exiting...");
                return;
            }

            DbData data;

            if (string.IsNullOrEmpty(options.ConnectionString) == false)
            {
                IDbDataFactory dbDataFactory = new ConnectionStringDbDataFactory(options.ConnectionString, Context.UniqueIdProvider);
                data = dbDataFactory.Create();

                if (string.IsNullOrEmpty(options.Alias) == false)
                {
                    Context.Config.Write(options.Alias, data);
                }
            }
            else
            {
                (var exists, data) = Context.Config.Read(options.Alias);
                if (exists == false)
                {
                    Context.Stream.WriteLine("Failed to supply valid Alias. Exiting...");
                    return;
                }
            }

            IPathFinder pathFinder = new MoveToBetterPathFinder();

            ISqlQueryFactory sqlQueryFactory = new MoveToBetterPrettierSqlQueryFactory(
                new FullyQualifiedTableAliasFactory(),
                new FirstDefaultTypeValueFactory(),
                padding: 5);

            ICommandHandler
                addFiltersCommandHandler = new AddFiltersCommandHandler(Context.Stream),
                addTablesCommandHandler = new AddTablesCommandHandler(Context.Stream),
                finishCommandHandler = new FinishCommandHandler(),
                helpCommandHandler = new HelpCommandHandler(Context.Stream);

            IParameterUserInterface parameterUserInterface = new FirstParameterUserInterface(Context.Stream,
                addFiltersCommandHandler,
                addTablesCommandHandler,
                finishCommandHandler,
                helpCommandHandler);

            IPathUserInterface pathUserInterface = new MoveToBetterPathUserInterface(Context.Stream);
            IOutputHandler outputHandler = new PrintToConsoleOutputHandler(Context.Stream);

            (data, var parameters) = parameterUserInterface.GetParameters(data);

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
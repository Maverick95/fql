using CommandLine;
using SqlHelper.Contexts;
using SqlHelper.Factories.DbData;
using SqlHelper.Factories.DefaultTypeValue;
using SqlHelper.Factories.SqlQuery;
using SqlHelper.Factories.TableAlias;
using SqlHelper.Helpers;
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
        private enum SaveDbDataToAliasAction
        {
            NONE,
            MERGE_ALIAS,
            NEW_ALIAS,
            OVERRIDE_ALIAS,
        }

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
                var dataFromConnection = dbDataFactory.Create();

                if (string.IsNullOrEmpty(options.Alias) == false)
                {
                    var optionsToActions = new List<(bool, SaveDbDataToAliasAction)>
                    {
                        (options.IsMergeAliasOptionSupplied, SaveDbDataToAliasAction.MERGE_ALIAS),
                        (options.IsNewAliasOptionSupplied, SaveDbDataToAliasAction.NEW_ALIAS),
                        (options.IsOverrideAliasOptionSupplied, SaveDbDataToAliasAction.OVERRIDE_ALIAS),
                    };

                    SaveDbDataToAliasAction action = SaveDbDataToAliasAction.NONE;

                    foreach(var oa in optionsToActions)
                    {
                        if (action is not SaveDbDataToAliasAction.NONE && oa.Item1)
                        {
                            Context.Stream.WriteLine("Only one option allowed when Alias and Connection String supplied. Exiting...");
                            return;
                        }
                        if (oa.Item1)
                        {
                            action = oa.Item2;
                        }
                    }

                    if (action is SaveDbDataToAliasAction.NONE)
                    {
                        Context.Stream.WriteLine("Option required when Alias and Connection String supplied. Exiting...");
                        return;
                    }

                    switch (action)
                    {
                        case SaveDbDataToAliasAction.MERGE_ALIAS:
                            {
                                (var exists, var dataFromAlias) = Context.Config.Read(options.Alias);
                                if (exists == false)
                                {
                                    Context.Stream.WriteLine("Option supplied for Merge Alias with non-existing Alias. Exiting...");
                                }
                                dataFromConnection = DbDataHelpers.TryMergeDbDataCustomConstraints(
                                    dataFromAlias,
                                    dataFromConnection,
                                    Context.UniqueIdProvider);
                                Context.Config.Write(options.Alias, dataFromConnection);
                            }
                            break;
                        case SaveDbDataToAliasAction.NEW_ALIAS:
                            {
                                (var exists, _) = Context.Config.Read(options.Alias);
                                if (exists)
                                {
                                    Context.Stream.WriteLine("Option supplied for New Alias with existing Alias. Exiting...");
                                    return;
                                }
                                Context.Config.Write(options.Alias, dataFromConnection);
                            }
                            break;
                        case SaveDbDataToAliasAction.OVERRIDE_ALIAS:
                            {
                                (var exists, _) = Context.Config.Read(options.Alias);
                                if (exists == false)
                                {
                                    Context.Stream.WriteLine("Option supplied for Override Alias with non-existing Alias. Exiting...");
                                    return;
                                }
                                Context.Config.Write(options.Alias, dataFromConnection);
                            }
                            break;
                    }
                }

                data = dataFromConnection;
            }
            else
            {
                (var exists, var dataFromAlias) = Context.Config.Read(options.Alias);
                if (exists == false)
                {
                    Context.Stream.WriteLine("Failed to supply valid Alias. Exiting...");
                    return;
                }

                data = dataFromAlias;
            }

            IPathFinder pathFinder = new MoveToBetterPathFinder();

            ISqlQueryFactory sqlQueryFactory = new MoveToBetterPrettierSqlQueryFactory(
                new FullyQualifiedTableAliasFactory(),
                new FirstDefaultTypeValueFactory(),
                padding: 5);

            ICommandHandler
                addFiltersCommandHandler = new AddFiltersCommandHandler(Context.Stream),
                addTablesCommandHandler = new AddTablesCommandHandler(Context.Stream),
                addCustomConstraintsCommandHandler = new AddCustomConstraintsCommandHandler(
                    Context.UniqueIdProvider,
                    Context.Stream,
                    Context.Config,
                    !string.IsNullOrEmpty(options.Alias),
                    options.Alias),
                finishCommandHandler = new FinishCommandHandler();

            IParameterUserInterface parameterUserInterface = new FirstParameterUserInterface(Context.Stream,
                addFiltersCommandHandler,
                addTablesCommandHandler,
                addCustomConstraintsCommandHandler,
                finishCommandHandler);

            IPathUserInterface pathUserInterface = new MoveToBetterPathUserInterface(Context.Stream);
            IOutputHandler outputHandler = options.IsPrintQueryOptionSupplied ?
                new PrintToConsoleOutputHandler(Context.Stream) :
                new SendToClipboardOutputHandler();

            (data, var parameters) = parameterUserInterface.GetParameters(data);

            var tables = parameters.Tables
                .Select(table => table.Id)
                .Union(parameters.Filters.Select(filter => filter.TableId))
                .ToList();

            var paths = pathFinder.Help(data, tables);

            if (paths.Any() == false)
            {
                Context.Stream.WriteLine("No output to generate! Your choices left no options to choose from.");
                Context.Stream.WriteLine("There can only be one table that has no link on it's primary field(s).");
                Context.Stream.Padding();
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
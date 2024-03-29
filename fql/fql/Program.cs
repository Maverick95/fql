﻿using CommandLine;
using fql.UserInterface.Choices.Formatters;
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

            if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                IDbDataFactory dbDataFactory = new ConnectionStringDbDataFactory(options.ConnectionString, Context.UniqueIdProvider);
                var dataFromConnection = dbDataFactory.Create();
                // Not strictly necessary, but a good idea to standardize reseeding constraints on creation
                dataFromConnection = DbDataHelpers.ReseedConstraints(dataFromConnection, Context.UniqueIdProvider);

                if (!string.IsNullOrEmpty(options.Alias))
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
                                // Required to standardize reseeding constraints on creation
                                dataFromAlias = DbDataHelpers.ReseedConstraints(dataFromAlias, Context.UniqueIdProvider);
                                if (exists)
                                {
                                    dataFromConnection = DbDataHelpers.TryMergeDbDataCustomConstraints(
                                        dataFromAlias,
                                        dataFromConnection,
                                        Context.UniqueIdProvider);
                                }
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
                // Required to standardize reseeding constraints on creation
                dataFromAlias = DbDataHelpers.ReseedConstraints(dataFromAlias, Context.UniqueIdProvider);
                if (!exists)
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

                addFiltersCommandHandler = new AddFiltersCommandHandler(
                    Context.Stream,
                    Context.FilterChoiceSelector,
                    new FilterChoiceFormatter(padding: 3)),

                addTablesCommandHandler = new AddTablesCommandHandler(
                    Context.Stream,
                    Context.TableChoiceSelector,
                    new TableChoiceFormatter(padding: 3)),

                addCustomConstraintsCommandHandler = new AddCustomConstraintsCommandHandler(
                    Context.UniqueIdProvider,
                    Context.Stream,
                    Context.Config,
                    Context.CustomConstraintChoiceSelector,
                    new CustomConstraintChoiceFormatter(padding: 3),
                    !string.IsNullOrEmpty(options.Alias),
                    options.Alias),

                finishCommandHandler = new FinishCommandHandler(Context.Stream);

            IParameterUserInterface parameterUserInterface = new FirstParameterUserInterface(Context.Stream,
                Context.CommandChoiceSelector,
                new StringFormatter(),
                addFiltersCommandHandler,
                addTablesCommandHandler,
                addCustomConstraintsCommandHandler,
                finishCommandHandler);

            IPathUserInterface pathUserInterface = new MoveToBetterPathUserInterface(
                Context.Stream,
                Context.PathChoiceSelector);

            IOutputHandler outputHandler = options.IsPrintQueryOptionSupplied ?
                new PrintToConsoleOutputHandler(Context.Stream) :
                new SendToClipboardOutputHandler();

            (data, var parameters) = parameterUserInterface.GetParameters(data);

            var tables = parameters.Tables
                .Select(table => table.Id)
                .Union(parameters.Filters.Select(filter => filter.TableId))
                .ToList();

            var paths = pathFinder.Help(data, tables);

            if (!paths.Any())
            {
                Context.Stream.WriteLine("No output to generate! Your choices left no options to choose from.");
                Context.Stream.WriteLine("There can only be one table that has no link on its primary field(s).");
                Context.Stream.Padding();
                return;
            }

            ResultRouteTree path = null;

            if (paths.Count() is 1)
            {
                Context.Stream.WriteLine("Path selected!");
                Context.Stream.Padding();
                path = paths.First();
            }
            else
            {
                path = pathUserInterface.Choose(paths);
            }

            var output = sqlQueryFactory.Generate(data, path, parameters);
            outputHandler.Handle(output);
        }
    }
}
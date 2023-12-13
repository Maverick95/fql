using fql.UserInterface.Choices.Formatters;
using fql.UserInterface.Choices.Selectors;
using SqlHelper.Helpers;
using SqlHelper.Models;

namespace SqlHelper.UserInterface.Path
{
    public class MoveToBetterPathUserInterface : IPathUserInterface
    {
        private enum NextPathDirection
        {
            FORWARDS = 0,
            BACKWARDS = 1,
        }

        private enum UserChoice
        {
            CHOOSE_CURRENT = 0,
            MOVE_FORWARDS = 1,
            MOVE_BACKWARDS = 2,
        }

        private readonly IStream _stream;
        private readonly IChoiceSelector<string> _selector;

        public MoveToBetterPathUserInterface(IStream stream, IChoiceSelector<string> selector)
        {
            _stream = stream;
            _selector = selector;
        }

        private void Write_Path(ResultRouteTree path)
        {
            var writePathData = new List<(int depth, int offset, Table table)>();
            var offset = 0;

            var pathInitiator = (ResultRouteTree parentTree) =>
            {
                writePathData.Add((0, offset, parentTree.Table));
                return 0;
            };

            var pathGenerator = (int parentDepth, ResultRoute childRoute, ResultRouteTree childTree) =>
            {
                var tables = childRoute.Route.Select(r => r.source).ToList();

                var childDepth = parentDepth + childRoute.Route.Count;

                var tablesDepths = Enumerable.Range(parentDepth + 1, tables.Count);
                var newWritePathData = tablesDepths.Zip(tables, (depth, table) => (depth, offset, table));
                writePathData.AddRange(newWritePathData);

                if (!childTree.Children.Any())
                {
                    offset += 1;
                }

                return childDepth;
            };
            
            ResultRouteTreeHelpers.EnumerateTreeDepthFirst(path, pathInitiator, pathGenerator);
            
            var maxNameLength = writePathData
                .SelectMany(data => new List<string> { data.table.Schema, data.table.Name })
                .Max(name => name.Length);

            const int padding = 3;
            var outputLength = maxNameLength + padding;
            var empty = new string(' ', outputLength);

            var writePathDataGroups = writePathData
                .GroupBy(data => data.depth)
                .OrderBy(group => group.Key);


            foreach (var group in writePathDataGroups)
            {
                var maxOffset = group.Max(data => data.offset);

                var writeGroupData = Enumerable.Range(0, maxOffset + 1)
                    .GroupJoin(
                        group,
                        offset => offset,
                        data => data.offset,
                        (offset, data) => data.Any() ? data.Single().table : null);

                var outputData = writeGroupData
                    .Select(data =>
                        data is not null ?
                        new
                        {
                            Arrow = "|".PadRight(outputLength),
                            Schema = data.Schema.PadRight(outputLength),
                            Name = data.Name.PadRight(outputLength),
                        } :
                        new
                        {
                            Arrow = empty,
                            Schema = empty,
                            Name = empty,
                        });
                
                var outputLines = string.Join("", outputData.Select(output => output.Arrow));
                _stream.WriteLine(outputLines);

                var outputSchemas = string.Join("", outputData.Select(output => output.Schema));
                _stream.WriteLine(outputSchemas);

                var outputNames = string.Join("", outputData.Select(output => output.Name));
                _stream.WriteLine(outputNames);
            }

            _stream.Padding();
        }

        public ResultRouteTree Choose(IEnumerable<ResultRouteTree> paths)
        {
            var enumerator = paths.GetEnumerator();
            var pathsBackwards = new Stack<ResultRouteTree>();
            var pathsForwards = new Stack<ResultRouteTree>();
            NextPathDirection direction = NextPathDirection.FORWARDS;

            ResultRouteTree chosen_path = null;

            while (chosen_path is null)
            {
                ResultRouteTree current_path = direction switch
                {
                    NextPathDirection.FORWARDS when pathsForwards.Any() => pathsForwards.Pop(),
                    NextPathDirection.FORWARDS when enumerator.MoveNext() => enumerator.Current,
                    NextPathDirection.FORWARDS when pathsBackwards.Any() => pathsBackwards.Pop(),

                    NextPathDirection.BACKWARDS when pathsBackwards.Any() => pathsBackwards.Pop(),
                    NextPathDirection.BACKWARDS when pathsForwards.Any() => pathsForwards.Pop(),
                    NextPathDirection.BACKWARDS when enumerator.MoveNext() => enumerator.Current,

                    _ => enumerator.Current,
                };

                Write_Path(current_path);
                
                var optionsToChoices = _selector switch
                {
                    var s when s is PathUserInterfaceOptionChoiceSelector =>
                        new Dictionary<string, UserChoice>
                        {
                            {   "p",                UserChoice.MOVE_BACKWARDS       },
                            {   "previous",         UserChoice.MOVE_BACKWARDS       },
                            {   "n",                UserChoice.MOVE_FORWARDS        },
                            {   "next",             UserChoice.MOVE_FORWARDS        },
                            {   "c",                UserChoice.CHOOSE_CURRENT       },
                            {   "current",          UserChoice.CHOOSE_CURRENT       },
                        },

                    var s when s is FzfChoiceSelector<string> =>
                        new Dictionary<string, UserChoice>
                        {
                            {   "previous",         UserChoice.MOVE_BACKWARDS       },
                            {   "next",             UserChoice.MOVE_FORWARDS        },
                            {   "current",          UserChoice.CHOOSE_CURRENT       },
                        },

                    _ => throw new NotImplementedException("Selector not implemented"),
                };

                var options = optionsToChoices.Keys;
                var selected = _selector.Choose(options, new StringFormatter());

                if (selected.Count() == 1)
                {
                    var choice = optionsToChoices[selected.First()];
                    switch (choice)
                    {
                        case UserChoice.CHOOSE_CURRENT:
                            chosen_path = current_path;
                            _stream.WriteLine("Path selected!");
                            _stream.Padding();
                            break;
                        case UserChoice.MOVE_FORWARDS:
                            direction = NextPathDirection.FORWARDS;
                            pathsBackwards.Push(current_path);
                            _stream.WriteLine("Printing next path...");
                            _stream.Padding();
                            break;
                        case UserChoice.MOVE_BACKWARDS:
                            direction = NextPathDirection.BACKWARDS;
                            pathsForwards.Push(current_path);
                            _stream.WriteLine("Printing previous path...");
                            _stream.Padding();
                            break;
                    }
                }
                else
                {
                    _stream.WriteLine("Invalid command, please try again");
                    _stream.Padding();
                }
            }

            return chosen_path;
        }
    }
}

using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using SqlHelper.Paths;

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

        public MoveToBetterPathUserInterface(IStream stream)
        {
            _stream = stream;
        }

        private UserChoice? Handler_UserChoice(string input)
        {
            var cleaned = input.Clean();
            var inputsMatchingChoices = new Dictionary<string, UserChoice>
            {
                {   "p",                UserChoice.MOVE_BACKWARDS       },
                {   "previous",         UserChoice.MOVE_BACKWARDS       },
                {   "n",                UserChoice.MOVE_FORWARDS        },
                {   "next",             UserChoice.MOVE_FORWARDS        },
                {   "c",                UserChoice.CHOOSE_CURRENT       },
                {   "current",          UserChoice.CHOOSE_CURRENT       },
            };

            if (inputsMatchingChoices.TryGetValue(cleaned, out var choice))
                return choice;

            return null;
        }

        private class PathData
        {
            public int Depth { get; set; }
            public ResultRoute Route { get; set; }
            public ResultRouteTree Tree { get; set; }
        }

        private void Write_Path(ResultRouteTree path)
        {
            var pathInitiator = (ResultRouteTree parentTree) => new PathData { Depth = 0, Route = null, Tree = parentTree };

            var pathGenerator = (PathData parent, ResultRoute childRoute, ResultRouteTree childTree) => new PathData
            {
                Depth = parent.Depth + (parent.Route?.Route.Count ?? 1),
                Route = childRoute,
                Tree = childTree,
            };
            
            var pathEnumerator = path.EnumerateDepthFirst(pathInitiator, pathGenerator);
            
            var writePathData = new List<(int depth, int offset, Table table)>();
            var offset = 0;

            foreach (var p in pathEnumerator)
            {
                if (p.Route is not null)
                {
                    var tables = p.Route.Route
                        .Select(r => r.source)
                        .ToList();

                    var tablesDepths = Enumerable.Range(p.Depth, tables.Count());
                    var newWritePathData = tablesDepths.Zip(tables, (depth, table) => (depth, offset, table));
                    writePathData.AddRange(newWritePathData);
                }
                else // Handle root node
                {
                    writePathData.Add((p.Depth, offset, p.Tree.Table));
                }

                if (p.Tree.IsLeaf)
                {
                    offset += 1;
                }
            }

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
                _stream.Write(outputLines);

                var outputSchemas = string.Join("", outputData.Select(output => output.Schema));
                _stream.Write(outputSchemas);

                var outputNames = string.Join("", outputData.Select(output => output.Name));
                _stream.Write(outputNames);
            }
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
                };

                Write_Path(current_path);
                _stream.Write("");

                UserChoice? choice = null;
                while (choice is null)
                {
                    var input = _stream.Read();
                    _stream.Write("");
                    choice = Handler_UserChoice(input);
                }

                switch (choice.Value)
                {
                    case UserChoice.CHOOSE_CURRENT:
                        chosen_path = current_path;
                        break;
                    case UserChoice.MOVE_FORWARDS:
                        direction = NextPathDirection.FORWARDS;
                        pathsBackwards.Push(current_path);
                        break;
                    case UserChoice.MOVE_BACKWARDS:
                        direction = NextPathDirection.BACKWARDS;
                        pathsForwards.Push(current_path);
                        break;
                }
            }

            return chosen_path;
        }
    }
}

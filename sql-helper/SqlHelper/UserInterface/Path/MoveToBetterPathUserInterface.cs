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

        private void Write_Path(ResultRouteTree path)
        {
            var writePathData = new List<(int depth, int offset, Table table)>();

            var writePathDataHelper = new Stack<(int depth, ResultRoute route, ResultRouteTree tree)>();
            writePathDataHelper.Push((0, null, path));

            var offset = 0;

            while (writePathDataHelper.Any())
            {
                (var depth, var route, var tree) = writePathDataHelper.Pop();

                var newDepth = depth;

                if (route is not null)
                {
                    var tables = route.Route
                        .Select(r => r.source)
                        .ToList();

                    var tablesDepths = Enumerable.Range(depth, tables.Count());

                    var newWritePathData = tablesDepths.Zip(tables, (depth, table) => (depth, offset, table));

                    writePathData.AddRange(newWritePathData);
                    newDepth += tables.Count();
                }
                else // Should only hit when considering Root Node.
                {
                    writePathData.Add((depth, offset, tree.Table));
                    newDepth += 1;
                }

                if (tree.IsLeaf)
                {
                    offset += 1;
                }
                else
                {
                    foreach (var child in tree.Children)
                    {
                        writePathDataHelper.Push((newDepth, child.route, child.child));
                    }
                }
            }

            var maxNameLength = writePathData
                .SelectMany(data => new List<string> { data.table.Schema, data.table.Name })
                .Max(name => name.Length);


            var writePathDataGroups = writePathData
                .GroupBy(data => data.depth)
                .OrderBy(group => group.Key);

            const int padding = 3;

            foreach (var group in writePathDataGroups)
            {
                var maxOffset = group.Max(data => data.offset);

                var writeGroupData = Enumerable.Range(0, maxOffset + 1)
                    .GroupJoin(
                        group,
                        offset => offset,
                        data => data.offset,
                        (offset, data) => data.Any() ? data.Single().table : null);

                var outputDataLines = writeGroupData
                    .Select(data => data is not null ? "|" : "")
                    .Select(output => output.PadRight(maxNameLength + padding));

                var outputLines = string.Join("", outputDataLines);

                _stream.Write(outputLines);

                var outputDataSchemas = writeGroupData
                    .Select(data => data is not null ? data.Schema : "")
                    .Select(output => output.PadRight(maxNameLength + padding));

                var outputSchemas = string.Join("", outputDataSchemas);

                _stream.Write(outputSchemas);

                var outputDataTableNames = writeGroupData
                    .Select(data => data is not null ? data.Name : "")
                    .Select(output => output.PadRight(maxNameLength + padding));

                var outputTableNames = string.Join("", outputDataTableNames);

                _stream.Write(outputTableNames);
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

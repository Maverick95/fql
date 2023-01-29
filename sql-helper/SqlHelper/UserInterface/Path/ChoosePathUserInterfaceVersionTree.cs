using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Paths;

namespace SqlHelper.UserInterface.Path
{
    public class ChoosePathUserInterfaceVersionTree : IPathUserInterfaceVersionTree
    {
        private readonly IStream _stream;

        public ChoosePathUserInterfaceVersionTree(IStream stream)
        {
            _stream = stream;
        }

        public ResultRouteTree Choose(IList<ResultRouteTree> results)
        {
            /*
             * Currently we are assuming a particular structure for each ResultRouteTree,
             * each has a single parent and a single child.
             */

            const int padding = 3;

            var names_start = results.SelectMany(
                result => new List<string>
                {
                    $"{result.Table.Schema}.",
                    result.Table.Name,
                });

            var names_paths = results.SelectMany(
                result =>
                {
                    // This is the key line.
                    var child = result.Children.First();
                    var links = child.route.Route.SelectMany(path =>
                        new List<string>
                        {
                            $"{path.source.Schema}.",
                            path.source.Name,
                        });
                    return links;
                });

            var max_name_length = names_start
                .Concat(names_paths)
                .Max(name => name.Length);

            var name_space = max_name_length + padding;

            var ids = Enumerable.Range(1, results.Count);

            _stream.Write("Select the path to use, for example, to select path 1, enter '1'");

            var data = ids.Zip(results, (id, result) => new
            {
                Id = id,
                Result = result
            });

            foreach (var d in data)
            {
                // Path option number
                _stream.Write(d.Id.ToString());

                // Schemas
                var child = d.Result.Children.First();
                var schemas = child.route.Route
                    .Select(path => path.source.Schema)
                    .ToList()
                    .Prepend(d.Result.Table.Schema)
                    .Select(schema => $"{schema}.".PadRight(name_space))
                    .Sentence(" -> ");

                _stream.Write(schemas);

                // Tables
                var tables = child.route.Route
                    .Select(path => path.source.Name)
                    .ToList()
                    .Prepend(d.Result.Table.Name)
                    .Select(table => table.PadRight(name_space))
                    .Sentence("    ");

                _stream.Write(tables);

                _stream.Write(string.Empty);
            }

            ResultRouteTree chosen_result = null;
            while (chosen_result is null)
            {
                var input = _stream.Read();
                
                if (int.TryParse(input, out var id) == false) continue;

                chosen_result = data
                    .Where(d => d.Id == id)
                    .Select(d => d.Result)
                    .FirstOrDefault((ResultRouteTree)null);
            }

            return chosen_result;
        }
    }
}

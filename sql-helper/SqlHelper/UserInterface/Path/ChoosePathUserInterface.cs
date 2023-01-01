using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Paths;

namespace SqlHelper.UserInterface.Path
{
    public class ChoosePathUserInterface : IPathUserInterface
    {
        private readonly IStream _stream;

        public ChoosePathUserInterface(IStream stream)
        {
            _stream = stream;
        }

        SqlHelperResult IPathUserInterface.Choose(IList<SqlHelperResult> results)
        {
            const int padding = 3;

            var names_start = results.SelectMany(
                result => new List<string>
                {
                    $"{result.Start.Schema}.",
                    result.Start.Name,
                });

            var names_paths = results.SelectMany(
                result => result.Paths.SelectMany(
                    path => new List<string>
                    {
                        $"{path.Table.Schema}.",
                        path.Table.Name,
                    }));

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
                var schemas = d.Result.Paths
                    .Select(path => path.Table.Schema)
                    .ToList()
                    .Prepend(d.Result.Start.Schema)
                    .Select(schema => $"{schema}.".PadRight(name_space))
                    .Sentence(" -> ");

                _stream.Write(schemas);

                // Tables
                var tables = d.Result.Paths
                    .Select(path => path.Table.Name)
                    .ToList()
                    .Prepend(d.Result.Start.Name)
                    .Select(table => table.PadRight(name_space))
                    .Sentence("    ");

                _stream.Write(tables);

                _stream.Write(string.Empty);
            }

            SqlHelperResult chosen_result = null;
            while (chosen_result is null)
            {
                var input = _stream.Read();
                
                if (int.TryParse(input, out var id) == false) continue;

                chosen_result = data
                    .Where(d => d.Id == id)
                    .Select(d => d.Result)
                    .FirstOrDefault((SqlHelperResult)null);
            }

            return chosen_result;
        }
    }
}

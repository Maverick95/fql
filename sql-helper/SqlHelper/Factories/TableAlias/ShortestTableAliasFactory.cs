using SqlHelper.Models;

namespace SqlHelper.Factories.TableAlias
{
    public class ShortestTableAliasFactory: ITableAliasFactory
    {
        public IEnumerable<string> Create(IEnumerable<Table> tables)
        {
            // Comment this out until we can fix it up proper.

            /*
            var unique_tables = tables
                .DistinctBy(table => table.Id)
                .ToList();

            var results = new SortedDictionary<long, string>();

            var max_substr_length = unique_tables.Max(t => t.Name.Length);

            var finished = false;
            var current_substr_length = 1;

            do
            {
                var aliases = unique_tables
                    .Where(t => results.ContainsKey(t.Id) == false)
                    .Select(t =>
                    {
                        
                        // This fixes a bug AND solves an edge case at the same time.
                        // If I had table names "USER" and "USERAPPLICATION",
                        // I would expect "USER" to be an alias for 1st table,
                        // and "USERA" to be an alias for 2nd table.
                         
                        var alias_substr_length = Math.Min(t.Name.Length, current_substr_length);
                        return new
                        {
                            Id = t.Id,
                            Alias = t.Name.Substring(0, alias_substr_length),
                        };
                    })
                    .GroupBy(t => t.Alias, t => t.Id)
                    .Where(grp => grp.Count() == 1)
                    .Select(grp => new
                    {
                        Id = grp.First(),
                        Alias = grp.Key,
                    });

                foreach(var alias in aliases)
                {
                    results.Add(alias.Id, alias.Alias);
                }

                // Edge-case for the finale.
                if (current_substr_length == max_substr_length)
                {
                    var aliases_with_schemas = unique_tables
                        .Where(t => results.ContainsKey(t.Id) == false)
                        .Select(t => new
                        {
                            Id = t.Id,
                            Alias = $"{t.Schema}_{t.Name}",
                        });

                    foreach(var alias in aliases_with_schemas)
                    {
                        results.Add(alias.Id, alias.Alias);
                    }

                    finished = true;
                }
                else
                {
                    finished = (unique_tables.Count == results.Count);
                }

                current_substr_length += 1;

            } while (finished == false);

            return results;
            */

            return null;
        }
    }
}

using SqlHelper.Models;

namespace SqlHelper.Factories.TableAlias
{
    public class FullyQualifiedTableAliasFactory : ITableAliasFactory
    {
        public SortedDictionary<long, string> Create(IEnumerable<Table> tables)
        {
            var result = new SortedDictionary<long, string>();
            foreach (var t in tables)
            {
                var alias = $"{t.Schema}_{t.Name}";
                result.TryAdd(t.Id, alias);
            }
            return result;
        }
    }
}

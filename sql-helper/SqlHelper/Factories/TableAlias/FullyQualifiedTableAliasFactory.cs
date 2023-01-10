using SqlHelper.Models;

namespace SqlHelper.Factories.TableAlias
{
    public class FullyQualifiedTableAliasFactory : ITableAliasFactory
    {
        public IEnumerable<string> Create(IEnumerable<Table> tables)
        {
            var result = tables.Select(t => $"{t.Schema}_{t.Name}");
            return result;
        }
    }
}

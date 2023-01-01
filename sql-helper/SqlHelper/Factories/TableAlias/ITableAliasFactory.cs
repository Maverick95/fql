using SqlHelper.Models;

namespace SqlHelper.Factories.TableAlias
{
    public interface ITableAliasFactory
    {
        public SortedDictionary<long, string> Create(IEnumerable<Table> tables);
    }
}

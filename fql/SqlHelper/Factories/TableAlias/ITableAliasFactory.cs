using SqlHelper.Models;

namespace SqlHelper.Factories.TableAlias
{
    public interface ITableAliasFactory
    {
        public IEnumerable<string> Create(IEnumerable<Table> tables);
    }
}

using SqlHelper.Models;

namespace SqlHelper.Config
{
    public interface IConfigManager
    {
        public IEnumerable<string> List();

        public (bool, DbData) Read(string alias);

        public void Write(string alias, DbData data);
    }
}

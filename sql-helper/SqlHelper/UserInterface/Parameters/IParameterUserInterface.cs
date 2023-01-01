using SqlHelper.Models;

namespace SqlHelper.UserInterface.Parameters
{
    public interface IParameterUserInterface
    {
        public SqlQueryParameters GetParameters(DbData data);
    }
}

using SqlHelper.Models;

namespace SqlHelper.UserInterface.Parameters
{
    public interface IParameterUserInterface
    {
        public (DbData data, SqlQueryParameters parameters) GetParameters(DbData data);
    }
}

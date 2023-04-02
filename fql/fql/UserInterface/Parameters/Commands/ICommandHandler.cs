using SqlHelper.Models;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public enum HandlerResult
    {
        NEXT_HANDLER,
        NEXT_COMMAND,
        FINISH,
    }

    public interface ICommandHandler
    {
        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters);
    }
}

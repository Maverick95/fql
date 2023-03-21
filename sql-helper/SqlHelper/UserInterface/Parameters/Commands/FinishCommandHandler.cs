using SqlHelper.Extensions;
using SqlHelper.Models;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class FinishCommandHandler : ICommandHandler
    {
        public (HandlerResult result, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            var cleaned = input.Clean();
            var execute_commands = new string[] { "e", "exec", "execute" };

            return execute_commands.Contains(cleaned) ?
                (HandlerResult.FINISH, parameters) :
                (HandlerResult.NEXT_HANDLER, parameters);
        }
    }
}

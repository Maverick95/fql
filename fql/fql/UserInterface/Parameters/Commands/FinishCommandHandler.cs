using SqlHelper.Extensions;
using SqlHelper.Models;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class FinishCommandHandler : ICommandHandler
    {
        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            var cleaned = input.Clean();
            var execute_commands = new string[] { "e", "exec", "execute" };
            var result = execute_commands.Contains(cleaned) ? HandlerResult.FINISH : HandlerResult.NEXT_HANDLER;

            return (result, data, parameters);
        }
    }
}

using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class FinishCommandHandler : ICommandHandler
    {
        private readonly IStream _stream;

        public FinishCommandHandler(IStream stream)
        {
            _stream = stream;
        }

        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            var cleaned = input.Clean();
            var execute_commands = new string[] { "e", "exec", "execute" };

            var result = HandlerResult.NEXT_HANDLER;

            if (execute_commands.Contains(cleaned))
            {
                if (parameters.Tables.Any() || parameters.Filters.Any())
                {
                    result = HandlerResult.FINISH;
                }
                else
                {
                    _stream.WriteLine("Can only execute after applying filters or tables.");
                    _stream.Padding();
                    result = HandlerResult.NEXT_COMMAND;
                }
            }

            return (result, data, parameters);
        }
    }
}

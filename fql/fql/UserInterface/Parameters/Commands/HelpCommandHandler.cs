using SqlHelper.Extensions;
using SqlHelper.Helpers;
using SqlHelper.Models;

namespace SqlHelper.UserInterface.Parameters.Commands
{
    public class HelpCommandHandler: ICommandHandler
    {
        private readonly IStream _stream;

        public HelpCommandHandler(IStream stream)
        {
            _stream = stream;
        }

        public (HandlerResult result, DbData data, SqlQueryParameters parameters) TryCommandHandle(string input, DbData data, SqlQueryParameters parameters)
        {
            var cleaned = input.Clean();
            var help_commands = new string[] { "h", "help" };
            
            if (help_commands.Contains(cleaned))
            {
                _stream.WriteLine("TODO : write help instructions lol rofl");
                _stream.Padding();
                return (HandlerResult.NEXT_COMMAND, data, parameters);
            }

            return (HandlerResult.NEXT_HANDLER, data, parameters);
        }
    }
}

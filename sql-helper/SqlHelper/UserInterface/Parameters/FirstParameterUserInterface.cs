using SqlHelper.Helpers;
using SqlHelper.Models;
using SqlHelper.UserInterface.Parameters.Commands;

namespace SqlHelper.UserInterface.Parameters
{
    public class FirstParameterUserInterface : IParameterUserInterface
    {
        private readonly IStream _stream;
        private readonly List<ICommandHandler> _commandHandlers;

        public FirstParameterUserInterface(IStream stream, params ICommandHandler[] commandHandlers)
        {
            _stream = stream;
            _commandHandlers = commandHandlers.ToList();
        }

        public SqlQueryParameters GetParameters(DbData data)
        {
            var parameters = new SqlQueryParameters
            {
                Tables = new List<Table>(),
                Filters = new List<Column>(),
            };

            var finished = false;
            var notHandledInputCount = 0;

            while (finished == false)
            {
                _stream.Write("Enter command (type 'h' or 'help' for options) : ");
                var input = _stream.ReadLine();
                _stream.Padding();
                var handled = false;
                
                foreach (var handler in _commandHandlers)
                {
                    (var result, parameters) = handler.TryCommandHandle(input, data, parameters);
                    finished = result == HandlerResult.FINISH;
                    if (result != HandlerResult.NEXT_HANDLER)
                    {
                        handled = true;
                        break;
                    }
                }

                if (handled == false)
                {
                    _stream.WriteLine("Command not found, please try again");
                    _stream.Padding();
                    if (++notHandledInputCount == 10)
                    {
                        _stream.WriteLine("Maximum number of incorrect commands hit, finishing...");
                        finished = true;
                    }
                }
                else
                {
                    notHandledInputCount = 0;
                }
            }

            return parameters;
        }
    }
}

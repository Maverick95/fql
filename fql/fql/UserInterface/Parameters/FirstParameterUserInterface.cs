using fql.UserInterface.Choices.Formatters;
using fql.UserInterface.Choices.Selectors;
using SqlHelper.Helpers;
using SqlHelper.Models;
using SqlHelper.UserInterface.Parameters.Commands;

namespace SqlHelper.UserInterface.Parameters
{
    public class FirstParameterUserInterface : IParameterUserInterface
    {
        private readonly IStream _stream;
        private readonly IChoiceSelector<string> _selector;
        private readonly IChoiceFormatter<string> _formatter;
        private readonly List<ICommandHandler> _commandHandlers;

        public FirstParameterUserInterface(
            IStream stream,
            IChoiceSelector<string> selector,
            IChoiceFormatter<string> formatter,
            params ICommandHandler[] commandHandlers)
        {
            _stream = stream;
            _selector = selector;
            _formatter = formatter;
            _commandHandlers = commandHandlers.ToList();
        }

        public (DbData data, SqlQueryParameters parameters) GetParameters(DbData data)
        {
            var parameters = new SqlQueryParameters
            {
                Tables = new List<Table>(),
                Filters = new List<Column>(),
            };

            var finished = false;
            var notHandledInputCount = 0;

            var commands = new List<string>
            {
                "constraint",
                "filter",
                "table",
                "execute",
            };

            while (!finished)
            {
                var choices = _selector.Choose(commands.OrderBy(c => c), _formatter);
                var handled = false;

                if (choices.Count() == 1)
                {
                    var command = choices.First();
                    foreach (var handler in _commandHandlers)
                    {
                        (var result, data, parameters) = handler.TryCommandHandle(command, data, parameters);
                        finished = result == HandlerResult.FINISH;
                        if (result != HandlerResult.NEXT_HANDLER)
                        {
                            handled = true;
                            break;
                        }
                    }
                }

                if (!handled)
                {
                    _stream.WriteLine("Invalid command, please try again");
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

            return (data, parameters);
        }
    }
}

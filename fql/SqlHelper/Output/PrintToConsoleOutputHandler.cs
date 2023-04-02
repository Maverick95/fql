using SqlHelper.Helpers;

namespace SqlHelper.Output
{
    public class PrintToConsoleOutputHandler: IOutputHandler
    {
        private readonly IStream _stream;

        public PrintToConsoleOutputHandler(IStream stream)
        {
            _stream = stream;
        }

        public void Handle(string output)
        {
            _stream.WriteLine(output);
        }
    }
}

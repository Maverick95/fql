namespace SqlHelper.Helpers
{
    public interface IStream
    {
        public string ReadLine();
        public void Write(string content);
        public void WriteLine(string content);
        public void Padding(int lines = 1);
    }

    /*
     * Most commonly used stream
     * Prints/reads content to/from the standard console window
     */
    public class ConsoleStream: IStream
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public void Write(string content)
        {
            Console.Write(content);
        }

        public void WriteLine(string content)
        {
            Console.WriteLine(content);
        }

        public void Padding(int lines = 1)
        {
            for (var _ = 0; _ < lines; _++)
            {
                Console.WriteLine(string.Empty);
            }
        }
    }

    /*
     * Used for testing
     * Wrapper around the base interface that logs all operations
     * Supply a fake internal stream with mocked return values for ReadLine
     * Don't care about padding in tests
     */

    public class LoggerStream : IStream
    {
        private readonly List<LogRecord> _logs;
        private readonly IStream _internalStream;
        private readonly bool _includePadding;

        public LoggerStream(IStream stream, bool includePadding = false)
        {
            _logs = new();
            _internalStream = stream;
            _includePadding = includePadding;
        }

        public void Padding(int lines)
        {
            _internalStream.Padding(lines);
            if (_includePadding)
            {
                _logs.AddRange(Enumerable.Repeat(new LogRecord { Type = "padding" }, lines));
            }
        }

        public string ReadLine()
        {
            var content = _internalStream.ReadLine();
            _logs.Add(new LogRecord { Type = "readline", Content = content });
            return content;
        }

        public void Write(string content)
        {
            _internalStream.Write(content);
            _logs.Add(new LogRecord { Type = "write", Content = content });
        }

        public void WriteLine(string content)
        {
            _internalStream.WriteLine(content);
            _logs.Add(new LogRecord { Type = "writeline", Content = content });
        }

        public List<LogRecord> Logs { get => _logs; }
    }

    public class LogRecord
    {
        public string Type { get; set; }
        public string Content { get; set; }
    }
}

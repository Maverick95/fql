namespace SqlHelper.Helpers
{
    public interface IStream
    {
        public string ReadLine();
        public void Write(string content);
        public void WriteLine(string content);
        public void Padding(int lines = 1);
    }

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
}

namespace SqlHelper.Helpers
{
    public interface IStream
    {
        public string Read();
        public void Write(string content);
    }

    public class ConsoleStream: IStream
    {
        public string Read()
        {
            return Console.ReadLine();
        }

        public void Write(string content)
        {
            Console.WriteLine(content);
        }
    }
}

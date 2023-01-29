using SqlHelper.Paths;

namespace SqlHelper.UserInterface.Path
{
    /* This is the stupid method that you used originally. */

    public class FirstStupidPathUserInterface: IPathUserInterface
    {
        public ResultRoute Choose(IList<ResultRoute> paths)
        {
            return paths.First();
        }
    }
}

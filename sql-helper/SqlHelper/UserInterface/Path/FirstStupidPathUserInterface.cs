using SqlHelper.Paths;

namespace SqlHelper.UserInterface.Path
{
    /* This is the stupid method that you used originally. */

    public class FirstStupidPathUserInterface: IPathUserInterface
    {
        public SqlHelperResult Choose(IList<SqlHelperResult> paths)
        {
            return paths.First();
        }
    }
}

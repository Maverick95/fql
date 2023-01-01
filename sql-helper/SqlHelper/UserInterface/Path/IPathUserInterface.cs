using SqlHelper.Paths;

namespace SqlHelper.UserInterface.Path
{
    public interface IPathUserInterface
    {
        public SqlHelperResult Choose(IList<SqlHelperResult> paths);
    }
}

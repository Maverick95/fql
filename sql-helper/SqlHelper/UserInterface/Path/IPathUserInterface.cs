using SqlHelper.Paths;

namespace SqlHelper.UserInterface.Path
{
    public interface IPathUserInterface
    {
        public ResultRoute Choose(IList<ResultRoute> paths);
    }
}

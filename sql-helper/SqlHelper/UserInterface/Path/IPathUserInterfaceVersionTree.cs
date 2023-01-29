using SqlHelper.Paths;

namespace SqlHelper.UserInterface.Path
{
    public interface IPathUserInterfaceVersionTree
    {
        public ResultRouteTree Choose(IList<ResultRouteTree> paths);
    }
}

using SqlHelper.Models;
using SqlHelper.Paths;
namespace SqlHelper.Test.TestUtilities.Paths
{
    public class ResultRouteTreeTest
    {
        public Table Table { get; set; }

        public IList<(ResultRoute route, ResultRouteTreeTest child)> Children { get; set; }
    }
}

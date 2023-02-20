namespace SqlHelper.Models
{
    public class ResultRoute
    {
        public Table Start { get; set; }
        public IList<(Table source, Constraint constraint)> Route { get; set; }
    }

    public class ResultRouteTree
    {
        public Table Table { get; set; }

        public IList<(ResultRoute route, ResultRouteTree child)> Children { get; set; }
    }
}

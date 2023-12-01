using SqlHelper.Models;

namespace fql.UserInterface.Choices.Models
{
    public class CustomConstraintChoice
    {
        public Constraint Constraint { get; set; }
        public Table SourceTable { get; set; }
        public Table TargetTable { get; set; }
    }
}

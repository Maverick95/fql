namespace SqlHelper.Models
{
    public class ConstraintColumnPair
    {
        public long TargetColumnId { get; set; }
        public long SourceColumnId { get; set; }
    }

    public class Constraint
    {
        public long Id { get; set; }
        public long TargetTableId { get; set; }
        public long SourceTableId { get; set; }
        
        // Constraints can be on more than 1 field.
        public IList<ConstraintColumnPair> Columns { get; set; }
    }
}

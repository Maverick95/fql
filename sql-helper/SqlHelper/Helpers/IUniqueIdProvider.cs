namespace SqlHelper.Helpers
{
    public interface IUniqueIdProvider
    {
        public long Next();
    }

    public class SequentialUniqueIdProvider : IUniqueIdProvider
    {
        private long _next;

        public SequentialUniqueIdProvider(long start = 0L)
        {
            _next = start;
        }

        public long Next()
        {
            var next = _next++;
            return next;
        }
    }


}

namespace SqlHelper.Helpers
{
    public static class IListChooseElementIterator
    {
        public static IEnumerable<IList<int>> GetEnumerable(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Must be >= 0");

            if (count == 0)
                yield break;

            var indices = new Stack<int>();
            indices.Push(0);
            
            yield return indices.ToList();

            while (indices.Any())
            {
                var last = indices.Pop();
                if (last < (count - 1))
                {
                    indices.Push(last);
                    indices.Push(last + 1);
                    yield return indices.ToList();
                }
                else if (indices.Any())
                {
                    last = indices.Pop();
                    indices.Push(last + 1);
                    yield return indices.ToList();
                }
            }
        }

        public static IEnumerable<IList<int>> GetEnumerable(int count, int choose)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Must be >= 0");

            if (choose < 0 || choose > count)
                throw new ArgumentOutOfRangeException("choose", "Must be >= 0 and <= count");

            if (count == 0)
                yield break;

            var indices = new Stack<int>();
            indices.Push(0);

            if (indices.Count >= choose)
                yield return indices.ToList();

            while (indices.Any())
            {
                var last = indices.Pop();
                if (last < (count - 1))
                {
                    indices.Push(last);
                    indices.Push(last + 1);
                    if (indices.Count >= choose)
                        yield return indices.ToList();
                }
                else if (indices.Any())
                {
                    last = indices.Pop();
                    indices.Push(last + 1);
                    if (indices.Count >= choose)
                        yield return indices.ToList();
                }
            }
        }
    }
}

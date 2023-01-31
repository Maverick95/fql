namespace SqlHelper.Helpers
{
    public static class IListChooseElementIterator
    {
        public static IEnumerable<IList<int>> GetAllElementCombinations(int count)
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

        public static IEnumerable<IList<int>> GetElementCombinationsWithChooseElements(int count, int choose)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Must be >= 0");

            if (choose < 0 || choose > count)
                throw new ArgumentOutOfRangeException("choose", "Must be >= 0 and <= count");

            if (count == 0 || choose == 0)
                yield break;

            var indices = new Stack<int>();
            foreach(var i in Enumerable.Range(0, choose))
                indices.Push(i);

            yield return indices.ToList();

            var indicesRemoved = 0;

            while (indices.Any())
            {
                var lastIndex = indices.Pop();
                var newLastIndex = lastIndex + 1;
                var maxIndex = count - 1;
                var difference = maxIndex - newLastIndex;
                if (indicesRemoved <= difference)
                {
                    indices.Push(newLastIndex);
                    foreach(var i in Enumerable.Range(1, indicesRemoved))
                    {
                        indices.Push(newLastIndex + i);
                    }
                    indicesRemoved = 0;
                    yield return indices.ToList();
                }
                else
                {
                    indicesRemoved++;
                }
            }
        }

        public static IEnumerable<IList<int>> GetElementCombinationsWithAtLeastChooseElements(int count, int choose)
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

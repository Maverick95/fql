namespace SqlHelper.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string Sentence(this IEnumerable<string> words, string separator = "", string emptyValue = "")
            => words.Any() ? string.Join(separator, words) : emptyValue;
    }
}

using System.Text.RegularExpressions;

namespace SqlHelper.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string Sentence(this IEnumerable<string> words, string separator = "", string emptyValue = "")
            => words.Any() ? string.Join(separator, words) : emptyValue;

        public static IEnumerable<string> AppendIndex(this IEnumerable<string> inputs, string separator = "_")
        {
            if (new Regex("\\D").IsMatch(separator) == false)
            {
                throw new ArgumentException("Numeric separator allows for duplicate results", "separator");
            }

            var indices = Enumerable.Range(0, inputs.Count());
            var results = inputs.Zip(indices, (input, index) => $"{input}{separator}{index}");

            return results;
        }
    }
}

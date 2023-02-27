using System.Text.RegularExpressions;

namespace SqlHelper.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string Sentence(this IEnumerable<string> words, string separator = "", string emptyValue = "")
        {
            if (words is null) throw new ArgumentNullException(nameof(words));
            if (separator is null) throw new ArgumentNullException(nameof(separator));
            if (emptyValue is null) throw new ArgumentNullException(nameof(emptyValue));

            return words.Any() ? string.Join(separator, words) : emptyValue;
        }

        public static IEnumerable<string> AppendIndex(this IEnumerable<string> inputs, string separator = "_")
        {
            if (inputs is null) throw new ArgumentNullException(nameof(inputs));
            
            if (new Regex("\\D").IsMatch(separator) == false)
                throw new ArgumentException("Numeric separator allows for duplicate results", nameof(separator));

            var indices = Enumerable.Range(0, inputs.Count());
            var results = inputs.Zip(indices, (input, index) => $"{input}{separator}{index}");

            return results;
        }
    }
}

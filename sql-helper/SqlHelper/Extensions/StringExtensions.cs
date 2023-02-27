using System.Text.RegularExpressions;

namespace SqlHelper.Extensions
{
    public static class StringExtensions
    {
        public static string Clean(this string input)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            var input_transformed = input.Trim().ToLowerInvariant();
            var rgx_whitespace = new Regex("\\s+");
            return rgx_whitespace.Replace(input_transformed, " ");
        }
    }
}
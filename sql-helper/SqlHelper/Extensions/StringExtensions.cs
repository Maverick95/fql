using System.Text.RegularExpressions;

namespace SqlHelper.Extensions
{
    public static class StringExtensions
    {
        public static string Clean(this string input)
        {
            var input_transformed = input.Trim().ToLowerInvariant();
            var rgx_whitespace = new Regex("\\s+");
            return rgx_whitespace.Replace(input_transformed, " ");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SqlHelper.Extensions;
using FluentAssertions;

namespace SqlHelper.Test.Extensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("ANGRY STRING", "angry string")]
        [InlineData(@"
            string with    lots        of  unnecessary
            white space", "string with lots of unnecessary white space")]
        [InlineData("     string with lEadinG and TRAILING white space", "string with leading and trailing white space")]
        public void Clean_ShouldCleanInputStrings(string input, string expected)
        {
            // ARRANGE / ACT
            var actual = input.Clean();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

using FluentAssertions;
using SqlHelper.Extensions;
using Xunit;
using FakeItEasy;

namespace SqlHelper.Test.Extensions
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void Sentence_ShouldReturnDefaultEmptyValue_ForEmptyList()
        {
            // ARRANGE
            var input = new List<string>();
            var expected = "";

            // ACT
            var actual = input.Sentence();

            // ASSERT
            actual.Should().Be(expected);
        }

        [Fact]
        public void Sentence_ShouldReturnProvidedEmptyValue_ForEmptyList()
        {
            // ARRANGE
            var input = new List<string>();
            var emptyValue = "test";
            var expected = "test";

            // ACT
            var actual = input.Sentence(emptyValue: emptyValue);

            // ASSERT
            actual.Should().Be(expected);
        }

        [Fact]
        public void Sentence_ShouldUseDefaultSeperator()
        {
            // ARRANGE
            var input = new List<string>
            {
                "and", "you", "will", "know", "us", "by", "the", "trail", "of", "the", "dead",
            };

            var expected = "andyouwillknowusbythetrailofthedead";

            // ACT
            var actual = input.Sentence();

            // ASSERT
            actual.Should().Be(expected);
        }

        [Fact]
        public void Sentence_ShouldUseProvidedSeperator()
        {
            // ARRANGE
            var input = new List<string>
            {
                "this", "is", "known", "as", "kebab", "case", null,
            };

            var separator = "-";

            var expected = "this-is-known-as-kebab-case-";

            // ACT
            var actual = input.Sentence(separator: separator);

            // ASSERT
            actual.Should().Be(expected);
        }

        [Fact]
        public void AppendIndex_ShouldAppendIndexWithDefaultSeparator()
        {
            // ARRANGE
            var inputs = new List<string>
            {
                "zero", "one", "two", "three", "four",
            };

            var expected = new List<string>
            {
                "zero_0", "one_1", "two_2", "three_3", "four_4",
            };

            // ACT
            var actual = inputs.AppendIndex();

            // ASSERT
            actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }

        [Fact]
        public void AppendIndex_ShouldAppendIndexWithProvidedSeparator()
        {
            // ARRANGE
            var inputs = new List<string>
            {
                "zero", "one", "two", "three", "four",
            };

            var separator = "-sep-";

            var expected = new List<string>
            {
                "zero-sep-0", "one-sep-1", "two-sep-2", "three-sep-3", "four-sep-4",
            };

            // ACT
            var actual = inputs.AppendIndex(separator);

            // ASSERT
            actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }

        [Fact]
        public void AppendIndex_ShouldThrowException_IfSeparatorOnlyContainsDigits()
        {
            // ARRANGE
            var inputs = new List<string>
            {
                "zero", "one", "two", "three", "four",
            };

            var separator = "789";

            var wrapper = () => inputs.AppendIndex(separator);

            // ACT / ASSERT
            wrapper.Should().Throw<ArgumentException>();
        }

    }
}

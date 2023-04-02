using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SqlHelpers = SqlHelper.Helpers;
using SqlHelper.Test.Helpers.IListChooseElementIterator.TestData;
using Xunit;

namespace SqlHelper.Test.Helpers.IListChooseElementIterator
{
    public class IListChooseElementIteratorTests
    {
        [Fact]
        public void GetAllElementCombinations_ShouldThrowException()
        {
            // ARRANGE
            var count = -2;
            var _ = new List<IList<int>>();
            var action = () =>
            {
                var iterator = SqlHelpers.IListChooseElementIterator.GetAllElementCombinations(count);
                foreach (var element in iterator)
                {
                    _.Add(element);
                }
            };

            // ASSERT
            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage($"Must be >= 0 (Parameter 'count')");
        }

        [Theory]
        [ClassData(typeof(GetAllElementCombinationsTestData))]
        public void GetAllElementCombinations_ShouldGenerateCorrectResults(int count, IList<IList<int>> expected)
        {
            // ARRANGE
            var iterator = SqlHelpers.IListChooseElementIterator.GetAllElementCombinations(count);
            var actual = new List<IList<int>>();

            // ACT
            foreach (var elements in iterator)
            {
                actual.Add(elements);
            }

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(-1, 3, "count", "Must be >= 0")]
        [InlineData(3, -1, "choose", "Must be >= 0 and <= count")]
        [InlineData(5, 6, "choose", "Must be >= 0 and <= count")]
        public void GetElementCombinationsWithAtLeastChooseElements_ShouldThrowException(int count, int choose, string arg, string message)
        {
            // ARRANGE
            var _ = new List<IList<int>>();
            var action = () =>
            {
                var iterator = SqlHelpers.IListChooseElementIterator.GetElementCombinationsWithAtLeastChooseElements(count, choose);
                foreach (var element in iterator)
                {
                    _.Add(element);
                }
            };

            // ASSERT
            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage($"{message} (Parameter '{arg}')");
        }

        [Theory]
        [ClassData(typeof(GetElementCombinationsWithAtLeastChooseElementsTestData))]
        public void GetElementCombinationsWithAtLeastChooseElements_ShouldGenerateCorrectResults(int count, int choose, IList<IList<int>> expected)
        {
            // ARRANGE
            var iterator = SqlHelpers.IListChooseElementIterator.GetElementCombinationsWithAtLeastChooseElements(count, choose);
            var actual = new List<IList<int>>();

            // ACT
            foreach (var elements in iterator)
            {
                actual.Add(elements);
            }

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [ClassData(typeof(GetElementCombinationsWithChooseElementsTestData))]
        public void GetElementCombinationsWithChooseElements_ShouldGenerateCorrectResults(int count, int choose, IList<IList<int>> expected)
        {
            // ARRANGE
            var iterator = SqlHelpers.IListChooseElementIterator.GetElementCombinationsWithChooseElements(count, choose);
            var actual = new List<IList<int>>();

            // ACT
            foreach (var elements in iterator)
            {
                actual.Add(elements);
            }

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

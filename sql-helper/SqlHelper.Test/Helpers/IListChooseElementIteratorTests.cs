using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using SqlHelper.Helpers;
using SqlHelper.Test.Helpers.TestData;
using Xunit;

namespace SqlHelper.Test.Helpers
{
    public class IListChooseElementIteratorTests
    {
        [Fact]
        public void GetEnumerable_CountOnly_ShouldThrowException()
        {
            // ARRANGE
            var count = -2;
            var _ = new List<IList<int>>();
            var action = () =>
            {
                var iterator = IListChooseElementIterator.GetEnumerable(count);
                foreach (var element in iterator)
                {
                    _.Add(element);
                }
            };

            // ASSERT
            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage($"Must be >= 0 (Parameter 'count')");
        }

        [Theory]
        [ClassData(typeof(IListChooseElementIteratorTestDataCountOnly))]
        public void GetEnumerable_CountOnly_ShouldGenerateCorrectResults(int count, IList<IList<int>> expected)
        {
            // ARRANGE
            var iterator = IListChooseElementIterator.GetEnumerable(count);
            var actual = new List<IList<int>>();

            // ACT
            foreach(var elements in iterator)
            {
                actual.Add(elements);
            }

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(-1, 3, "count", "Must be >= 0")]
        [InlineData(3, -1, "choose", "Must be >= 0 and <= count")]
        [InlineData(5,  6, "choose", "Must be >= 0 and <= count")]
        public void GetEnumerable_CountAndChoose_ShouldThrowException(int count, int choose, string arg, string message)
        {
            // ARRANGE
            var _ = new List<IList<int>>();
            var action = () =>
            {
                var iterator = IListChooseElementIterator.GetEnumerable(count, choose);
                foreach (var element in iterator)
                {
                    _.Add(element);
                }
            };

            // ASSERT
            action.Should().Throw<ArgumentOutOfRangeException>().WithMessage($"{message} (Parameter '{arg}')");
        }


        [Theory]
        [ClassData(typeof(IListChooseElementIteratorTestDataCountAndChoose))]
        public void GetEnumerable_CountAndChoose_ShouldGenerateCorrectResults(int count, int choose, IList<IList<int>> expected)
        {
            // ARRANGE
            var iterator = IListChooseElementIterator.GetEnumerable(count, choose);
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

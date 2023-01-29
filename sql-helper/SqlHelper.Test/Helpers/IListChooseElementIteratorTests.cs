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

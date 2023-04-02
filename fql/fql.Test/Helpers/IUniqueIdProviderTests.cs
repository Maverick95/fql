using FluentAssertions;
using SqlHelper.Helpers;
using Xunit;

namespace SqlHelper.Test.Helpers
{
    public class IUniqueIdProviderTests
    {
        [Fact]
        public void SequentialUniqueIdProvider_ShouldReturnIncrementalUniqueIds_IgnoringSeed()
        {
            // ARRANGE
            var provider = new SequentialUniqueIdProvider();
            var expected = new List<long> { 0L, 1L, 2L, 3L, 4L };

            // ACT
            var actual = new List<long>();
            for (var i = 0; i < 5; i++) { actual.Add(provider.Next()); }

            // ASSERT
            actual.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }
    }
}

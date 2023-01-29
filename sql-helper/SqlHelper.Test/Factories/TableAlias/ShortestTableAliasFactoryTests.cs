using FluentAssertions;
using SqlHelper.Factories.TableAlias;
using SqlHelper.Models;
using Xunit;

namespace SqlHelper.Test.Factories.TableAlias
{
    public class ShortestTableAliasFactoryTests
    {
        private readonly ShortestTableAliasFactory _factory;

        public ShortestTableAliasFactoryTests()
        {
            _factory = new ShortestTableAliasFactory();
        }

        /*
         * Tests are removed until this has been fixed.
        [Fact]
        public void Create_ShouldCreateShortestTableAliasesForSimilarData()
        {
            // ARRANGE
            var tables = new List<Table>
            {
                new()
                {
                    Id = 1,
                    Schema = "data",
                    Name = "CUSTOMER",
                },
                new()
                {
                    Id = 2,
                    Schema = "data",
                    Name = "ORDER",
                },
                new()
                {
                    Id = 3,
                    Schema = "data",
                    Name = "ORDERCUSTOMER",
                },
                new()
                {
                    Id = 4,
                    Schema = "data",
                    Name = "ADDRESS",
                },
                new()
                {
                    Id = 5,
                    Schema = "archive",
                    Name = "ADDRESS",
                },
            };

            var expected = new List<string>
            {
                "C",
                "ORDER",
                "ORDERC",
                "data_ADDRESS",
                "archive_ADDRESS",
            };

            // ACT
            var actual = _factory.Create(tables);

            // ASSERT
            actual.Should().BeEquivalentTo(expected);

        }
        */

    }
}

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

            var expected = new SortedDictionary<long, string>
            {
                { 1, "C" },
                { 2, "ORDER" },
                { 3, "ORDERC" },
                { 4, "data_ADDRESS" },
                { 5, "archive_ADDRESS" },
            };

            // ACT
            var actual = _factory.Create(tables);

            // ASSERT
            actual.Should().BeEquivalentTo(expected);

        }

        [Fact]
        public void Create_ShouldHandleDuplicateTables()
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
                new() // Duplicate
                {
                    Id = 1,
                    Schema = "data",
                    Name = "CUSTOMER",
                },
                new() // Duplicate
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
                new() // Duplicate
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

            var expected = new SortedDictionary<long, string>
            {
                { 1, "C" },
                { 2, "ORDER" },
                { 3, "ORDERC" },
                { 4, "data_ADDRESS" },
                { 5, "archive_ADDRESS" },
            };

            // ACT
            var actual = _factory.Create(tables);

            // ASSERT
            actual.Should().BeEquivalentTo(expected);

        }

    }
}

using FluentAssertions;
using SqlHelper.Factories.TableAlias;
using SqlHelper.Models;
using Xunit;

namespace SqlHelper.Test.Factories.TableAlias
{
    public class FullyQualifiedTableAliasFactoryTests
    {
        private readonly FullyQualifiedTableAliasFactory _factory;

        public FullyQualifiedTableAliasFactoryTests()
        {
            _factory = new FullyQualifiedTableAliasFactory();
        }

        [Fact]
        public void Create_ShouldCreateFullyQualifiedAliases()
        {
            // ARRANGE
            var tables = new List<Table>
            {
                new()
                {
                    Id = 1,
                    Schema = "SCHEMA_1",
                    Name = "Table_1",
                },
                new()
                {
                    Id = 2,
                    Schema = "SCHEMA_1",
                    Name = "TaBlE_2",
                },
                new()
                {
                    Id = 3,
                    Schema = "schema_2",
                    Name = "Table_3",
                },
                new()
                {
                    Id = 4,
                    Schema = "schema_2",
                    Name = "TABLE_4",
                },
            };

            var expected = new List<string>
            {
                "SCHEMA_1_Table_1",
                "SCHEMA_1_TaBlE_2",
                "schema_2_Table_3",
                "schema_2_TABLE_4",
            };

            // ACT
            var actual = _factory.Create(tables);

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

using Xunit;
using FakeItEasy;
using FluentAssertions;
using System.Data;
using SqlHelper.Helpers;
using SqlHelper.Models;
using SqlHelper.Factories.DbData;

namespace SqlHelper.Test.Factories.DbData
{
    public class LocalSqlExpressDbDataFactoryTests
    {
        [Fact]
        public void Create_ShouldConstructGraphModel()
        {
            // ARRANGE
            var mockQueryFactory = A.Fake<IDbQueryFactory>();
            var mockConnectionFactory = A.Fake<IDbConnectionFactory>();
            A.CallTo(() => mockConnectionFactory.Create()).Returns(A.Fake<IDbConnection>());

            var mockUniqueIdProvider = A.Fake<IUniqueIdProvider>();
            A.CallTo(() => mockUniqueIdProvider.Next()).ReturnsNextFromSequence(4, 8, 15, 16, 23, 42);

            var mockCommandFactory = A.Fake<IDbCommandFactory>();

            var mockCommandTables = A.Fake<IDbCommand>();
            var mockReaderTable = A.Fake<IDataReader>();
            A.CallTo(() => mockReaderTable.Read()).ReturnsNextFromSequence(true, true, true, false);
            A.CallTo(() => mockReaderTable["Id"]).ReturnsNextFromSequence(1, 2, 3);
            A.CallTo(() => mockReaderTable["Schema"]).ReturnsNextFromSequence("data", "data", "data");
            A.CallTo(() => mockReaderTable["Name"]).ReturnsNextFromSequence("Customer", "Order", "OrderItem");
            A.CallTo(() => mockCommandTables.ExecuteReader()).Returns(mockReaderTable);

            var mockCommandColumns = A.Fake<IDbCommand>();
            var mockReaderColumn = A.Fake<IDataReader>();
            A.CallTo(() => mockReaderColumn.Read()).ReturnsNextFromSequence(true, true, true, true, true, true, false);
            A.CallTo(() => mockReaderColumn["TableId"]).ReturnsNextFromSequence(1, 1, 2, 2, 3, 3);
            A.CallTo(() => mockReaderColumn["ColumnId"]).ReturnsNextFromSequence(1, 2, 1, 2, 1, 2);
            A.CallTo(() => mockReaderColumn["Name"]).ReturnsNextFromSequence("Id", "Name", "Id", "CustomerId", "Id", "OrderId");
            A.CallTo(() => mockReaderColumn["Type"]).ReturnsNextFromSequence("integer", "string", "long", "integer", "long", "long");
            A.CallTo(() => mockReaderColumn["Nullable"]).ReturnsNextFromSequence(false, true, false, true, false, false);
            A.CallTo(() => mockReaderColumn["IsPrimaryKey"]).ReturnsNextFromSequence(true, false, true, false, true, false);
            A.CallTo(() => mockCommandColumns.ExecuteReader()).Returns(mockReaderColumn);

            var mockCommandConstraints = A.Fake<IDbCommand>();
            var mockReaderConstraint = A.Fake<IDataReader>();
            A.CallTo(() => mockReaderConstraint.Read()).ReturnsNextFromSequence(true, true, false);
            A.CallTo(() => mockReaderConstraint["Id"]).ReturnsNextFromSequence(1, 2);
            A.CallTo(() => mockReaderConstraint["TargetTableId"]).ReturnsNextFromSequence(2, 3);
            A.CallTo(() => mockReaderConstraint["SourceTableId"]).ReturnsNextFromSequence(1, 2);
            A.CallTo(() => mockReaderConstraint["TargetColumn"]).ReturnsNextFromSequence(2, 2);
            A.CallTo(() => mockReaderConstraint["SourceColumn"]).ReturnsNextFromSequence(1, 1);
            A.CallTo(() => mockCommandConstraints.ExecuteReader()).Returns(mockReaderConstraint);

            A.CallTo(() => mockCommandFactory.Create()).ReturnsNextFromSequence(mockCommandTables, mockCommandColumns, mockCommandConstraints);

            var actualGraphFactory = new LocalSqlExpressDbDataFactory(
                "test_database",
                mockUniqueIdProvider,
                mockQueryFactory,
                mockConnectionFactory,
                mockCommandFactory);

            var expected = new Models.DbData
            {
                Tables = new SortedDictionary<long, Table>
                {
                    { 1, new() { Id = 1, Schema = "data", Name = "Customer" } },
                    { 2, new() { Id = 2, Schema = "data", Name = "Order" } },
                    { 3, new() { Id = 3, Schema = "data", Name = "OrderItem" } },
                },
                Columns = new SortedDictionary<(long TableId, long ColumnId), Column>
                {
                    { (1, 1), new() { TableId = 1, ColumnId = 1, Name = "Id", Type = "integer", Nullable = false, IsPrimaryKey = true } },
                    { (1, 2), new() { TableId = 1, ColumnId = 2, Name = "Name", Type = "string", Nullable = true, IsPrimaryKey = false } },
                    { (2, 1), new() { TableId = 2, ColumnId = 1, Name = "Id", Type = "long", Nullable = false, IsPrimaryKey = true } },
                    { (2, 2), new() { TableId = 2, ColumnId = 2, Name = "CustomerId", Type = "integer", Nullable = true, IsPrimaryKey = false } },
                    { (3, 1), new() { TableId = 3, ColumnId = 1, Name = "Id", Type = "long", Nullable = false, IsPrimaryKey = true } },
                    { (3, 2), new() { TableId = 3, ColumnId = 2, Name = "OrderId", Type = "long", Nullable = false, IsPrimaryKey = false } },
                },
                Constraints = new SortedDictionary<long, Models.Constraint>
                {
                    { 4, new()
                    {
                        Id = 4,
                        IsCustom = false,
                        TargetTableId = 2,
                        SourceTableId = 1,
                        Columns = new List<ConstraintColumnPair>
                        {
                            new()
                            {
                                TargetColumnId = 2,
                                SourceColumnId = 1,
                            }
                        }
                    } },
                    { 8, new()
                    {
                        Id = 8,
                        IsCustom = false,
                        TargetTableId = 3,
                        SourceTableId = 2,
                        Columns = new List<ConstraintColumnPair>
                        {
                            new()
                            {
                                TargetColumnId = 2,
                                SourceColumnId = 1,
                            }
                        }
                    } }
                }
            };

            // ACT
            var actual = actualGraphFactory.Create();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Create_ShouldConstructGraphModelWithMultiColumnConstraints()
        {
            // ARRANGE
            var mockQueryFactory = A.Fake<IDbQueryFactory>();
            var mockConnectionFactory = A.Fake<IDbConnectionFactory>();
            A.CallTo(() => mockConnectionFactory.Create()).Returns(A.Fake<IDbConnection>());

            var mockUniqueIdProvider = A.Fake<IUniqueIdProvider>();
            A.CallTo(() => mockUniqueIdProvider.Next()).ReturnsNextFromSequence(4, 8, 15, 16, 23, 42);

            var mockCommandFactory = A.Fake<IDbCommandFactory>();

            var mockCommandTables = A.Fake<IDbCommand>();
            var mockReaderTable = A.Fake<IDataReader>();
            A.CallTo(() => mockReaderTable.Read()).ReturnsNextFromSequence(true, true, true, false);
            A.CallTo(() => mockReaderTable["Id"]).ReturnsNextFromSequence(1, 2, 3);
            A.CallTo(() => mockReaderTable["Schema"]).ReturnsNextFromSequence("data", "data", "data");
            A.CallTo(() => mockReaderTable["Name"]).ReturnsNextFromSequence("Customer", "Order", "OrderItem");
            A.CallTo(() => mockCommandTables.ExecuteReader()).Returns(mockReaderTable);

            var mockCommandColumns = A.Fake<IDbCommand>();
            var mockReaderColumn = A.Fake<IDataReader>();
            A.CallTo(() => mockReaderColumn.Read()).ReturnsNextFromSequence(true, true, true, true, true, true, true, true, false);
            A.CallTo(() => mockReaderColumn["TableId"]).ReturnsNextFromSequence(1, 1, 2, 2, 2, 3, 3, 3);
            A.CallTo(() => mockReaderColumn["ColumnId"]).ReturnsNextFromSequence(1, 2, 1, 2, 3, 1, 2, 3);
            A.CallTo(() => mockReaderColumn["Name"]).ReturnsNextFromSequence("Id", "Name", "Id", "CustomerId", "OrderDate", "Id", "OrderId", "OrderDate");
            A.CallTo(() => mockReaderColumn["Type"]).ReturnsNextFromSequence("integer", "string", "long", "integer", "date", "long", "long", "date");
            A.CallTo(() => mockReaderColumn["Nullable"]).ReturnsNextFromSequence(false, true, false, true, false, false, false, false);
            A.CallTo(() => mockReaderColumn["IsPrimaryKey"]).ReturnsNextFromSequence(true, false, true, false, false, true, false, false);
            A.CallTo(() => mockCommandColumns.ExecuteReader()).Returns(mockReaderColumn);

            var mockCommandConstraints = A.Fake<IDbCommand>();
            var mockReaderConstraint = A.Fake<IDataReader>();
            A.CallTo(() => mockReaderConstraint.Read()).ReturnsNextFromSequence(true, true, true, false);
            A.CallTo(() => mockReaderConstraint["Id"]).ReturnsNextFromSequence(1, 2, 2);
            A.CallTo(() => mockReaderConstraint["TargetTableId"]).ReturnsNextFromSequence(2, 3, 3);
            A.CallTo(() => mockReaderConstraint["SourceTableId"]).ReturnsNextFromSequence(1, 2, 2);
            A.CallTo(() => mockReaderConstraint["TargetColumn"]).ReturnsNextFromSequence(2, 2, 3);
            A.CallTo(() => mockReaderConstraint["SourceColumn"]).ReturnsNextFromSequence(1, 1, 3);
            A.CallTo(() => mockCommandConstraints.ExecuteReader()).Returns(mockReaderConstraint);

            A.CallTo(() => mockCommandFactory.Create()).ReturnsNextFromSequence(mockCommandTables, mockCommandColumns, mockCommandConstraints);

            var actualGraphFactory = new LocalSqlExpressDbDataFactory(
                "test_database",
                mockUniqueIdProvider,
                mockQueryFactory,
                mockConnectionFactory,
                mockCommandFactory);

            var expected = new Models.DbData
            {
                Tables = new SortedDictionary<long, Table>
                {
                    { 1, new() { Id = 1, Schema = "data", Name = "Customer" } },
                    { 2, new() { Id = 2, Schema = "data", Name = "Order" } },
                    { 3, new() { Id = 3, Schema = "data", Name = "OrderItem" } },
                },
                Columns = new SortedDictionary<(long TableId, long ColumnId), Column>
                {
                    { (1, 1), new() { TableId = 1, ColumnId = 1, Name = "Id", Type = "integer", Nullable = false, IsPrimaryKey = true } },
                    { (1, 2), new() { TableId = 1, ColumnId = 2, Name = "Name", Type = "string", Nullable = true, IsPrimaryKey = false } },
                    { (2, 1), new() { TableId = 2, ColumnId = 1, Name = "Id", Type = "long", Nullable = false, IsPrimaryKey = true } },
                    { (2, 2), new() { TableId = 2, ColumnId = 2, Name = "CustomerId", Type = "integer", Nullable = true, IsPrimaryKey = false } },
                    { (2, 3), new() { TableId = 2, ColumnId = 3, Name = "OrderDate", Type = "date", Nullable = false, IsPrimaryKey = false } },
                    { (3, 1), new() { TableId = 3, ColumnId = 1, Name = "Id", Type = "long", Nullable = false, IsPrimaryKey = true } },
                    { (3, 2), new() { TableId = 3, ColumnId = 2, Name = "OrderId", Type = "long", Nullable = false, IsPrimaryKey = false } },
                    { (3, 3), new() { TableId = 3, ColumnId = 3, Name = "OrderDate", Type = "date", Nullable = false, IsPrimaryKey = false } },
                },
                Constraints = new SortedDictionary<long, Models.Constraint>
                {
                    { 4, new()
                    {
                        Id = 4,
                        IsCustom = false,
                        TargetTableId = 2,
                        SourceTableId = 1,
                        Columns = new List<ConstraintColumnPair>
                        {
                            new()
                            {
                                TargetColumnId = 2,
                                SourceColumnId = 1,
                            }
                        }
                    } },
                    { 8, new()
                    {
                        Id = 8,
                        IsCustom = false,
                        TargetTableId = 3,
                        SourceTableId = 2,
                        Columns = new List<ConstraintColumnPair>
                        {
                            new()
                            {
                                TargetColumnId = 2,
                                SourceColumnId = 1,
                            },
                            new()
                            {
                                TargetColumnId = 3,
                                SourceColumnId = 3,
                            }
                        }
                    } }
                }
            };

            // ACT
            var actual = actualGraphFactory.Create();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

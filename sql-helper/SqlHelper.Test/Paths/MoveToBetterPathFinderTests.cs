using FluentAssertions;
using SqlHelper.Models;
using SqlHelper.Paths;
using SqlHelper.Test.TestUtilities.Paths;
using Xunit;

namespace SqlHelper.Test.Paths
{
    public class MoveToBetterPathFinderTests
    {
        /*
         * Test Case 1 - Simple Path
         * Single route, Single table, Single chain
        
            5 -> 4 -> 3 -> 2 -> 1

         */
        [Fact]
        public void Help_ShouldLocate_SingleRoute_With_SingleTable_Along_SingleChain()
        {
            // ARRANGE
            var graph = new DbData
            {
                Tables = new()
                {
                    { 1, new() { Id = 1 } },
                    { 2, new() { Id = 2 } },
                    { 3, new() { Id = 3 } },
                    { 4, new() { Id = 4 } },
                    { 5, new() { Id = 5 } },
                },
                Columns = new()
                {
                    { (1, 1),  new() { TableId = 1, ColumnId = 1  } },
                    { (1, 2),  new() { TableId = 1, ColumnId = 2  } },

                    { (2, 3),  new() { TableId = 2, ColumnId = 3  } },
                    { (2, 4),  new() { TableId = 2, ColumnId = 4  } },

                    { (3, 5),  new() { TableId = 3, ColumnId = 5  } },
                    { (3, 6),  new() { TableId = 3, ColumnId = 6  } },

                    { (4, 7),  new() { TableId = 4, ColumnId = 7  } },
                    { (4, 8),  new() { TableId = 4, ColumnId = 8  } },

                    { (5, 9),  new() { TableId = 5, ColumnId = 9  } },
                    { (5, 10), new() { TableId = 5, ColumnId = 10 } },
                },
                Constraints = new()
                {
                    { 1, new() { Id = 1, TargetTableId = 5, SourceTableId = 4 } },
                    { 2, new() { Id = 2, TargetTableId = 4, SourceTableId = 3 } },
                    { 3, new() { Id = 3, TargetTableId = 3, SourceTableId = 2 } },
                    { 4, new() { Id = 4, TargetTableId = 2, SourceTableId = 1 } },
                },
            };

            var tables = new List<long> { 5 };

            var pathFinder = new MoveToBetterPathFinder();

            var expected = new List<ResultRouteTreeTest>
            {
                new()
                {
                    Table = new() { Id = 5 },
                    Children = new List<(ResultRoute, ResultRouteTreeTest)>(),
                },
            };
        
            // ACT
            var actual = pathFinder.Help(graph, tables).ToList();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        /*
         * Test Case 2 - Simple Path 2
         * Single route, Multiple tables, Single chain
          
            5 -> 4 -> 3 -> 2 -> 1 
          
         */
        [Fact]
        public void Help_ShouldLocate_SingleRoute_With_MultipleTables_Along_SingleChain()
        {
            // ARRANGE
            var graph = new DbData
            {
                Tables = new()
                {
                    { 1, new() { Id = 1 } },
                    { 2, new() { Id = 2 } },
                    { 3, new() { Id = 3 } },
                    { 4, new() { Id = 4 } },
                    { 5, new() { Id = 5 } },
                },
                Columns = new()
                {
                    { (1, 1),  new() { TableId = 1, ColumnId = 1  } },
                    { (1, 2),  new() { TableId = 1, ColumnId = 2  } },

                    { (2, 3),  new() { TableId = 2, ColumnId = 3  } },
                    { (2, 4),  new() { TableId = 2, ColumnId = 4  } },

                    { (3, 5),  new() { TableId = 3, ColumnId = 5  } },
                    { (3, 6),  new() { TableId = 3, ColumnId = 6  } },

                    { (4, 7),  new() { TableId = 4, ColumnId = 7  } },
                    { (4, 8),  new() { TableId = 4, ColumnId = 8  } },

                    { (5, 9),  new() { TableId = 5, ColumnId = 9  } },
                    { (5, 10), new() { TableId = 5, ColumnId = 10 } },
                },
                Constraints = new()
                {
                    { 1, new() { Id = 1, TargetTableId = 5, SourceTableId = 4 } },
                    { 2, new() { Id = 2, TargetTableId = 4, SourceTableId = 3 } },
                    { 3, new() { Id = 3, TargetTableId = 3, SourceTableId = 2 } },
                    { 4, new() { Id = 4, TargetTableId = 2, SourceTableId = 1 } },
                },
            };

            var tables = new List<long> { 2, 3, 5 };

            var pathFinder = new MoveToBetterPathFinder();

            var expected = new List<ResultRouteTreeTest>
            {
                new ResultRouteTreeTest
                {
                    Table = new() { Id = 5 },
                    Children = new List<(ResultRoute, ResultRouteTreeTest)>
                    {
                        (
                            new ResultRoute
                            {
                                Start = new() { Id = 5 },
                                Route = new List<(Table, Constraint)>
                                {
                                    (
                                        new() { Id = 4 },
                                        new() { Id = 1, TargetTableId = 5, SourceTableId = 4 }
                                    ),
                                    (
                                        new() { Id = 3 },
                                        new() { Id = 2, TargetTableId = 4, SourceTableId = 3 }
                                    )
                                }
                            },
                            new ResultRouteTreeTest
                            {
                                Table = new() { Id = 3 },
                                Children = new List<(ResultRoute, ResultRouteTreeTest)>
                                {
                                    (
                                        new ResultRoute
                                        {
                                            Start = new() { Id = 3 },
                                            Route = new List<(Table, Constraint)>
                                            {
                                                (
                                                    new() { Id = 2 },
                                                    new() { Id = 3, TargetTableId = 3, SourceTableId = 2 }
                                                )
                                            }
                                        },
                                        new ResultRouteTreeTest
                                        {
                                            Table = new() { Id = 2 },
                                            Children = new List<(ResultRoute, ResultRouteTreeTest)> { },
                                        }
                                    )
                                },
                            }
                        )
                    },
                },
            };

            // ACT
            var actual = pathFinder.Help(graph, tables).ToList();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        /*
         * Test Case 3
         * Single route, Multiple tables, Multiple chains
          
            5 -> 4 -> 3 
              -> 2 -> 1
         */
        [Fact]
        public void Help_ShouldLocate_SingleRoute_With_MultipleTables_Along_MultipleChains()
        {
            // ARRANGE
            var graph = new DbData
            {
                Tables = new()
                {
                    { 1, new() { Id = 1 } },
                    { 2, new() { Id = 2 } },
                    { 3, new() { Id = 3 } },
                    { 4, new() { Id = 4 } },
                    { 5, new() { Id = 5 } },
                },
                Columns = new()
                {
                    { (1, 1),  new() { TableId = 1, ColumnId = 1  } },
                    { (1, 2),  new() { TableId = 1, ColumnId = 2  } },

                    { (2, 3),  new() { TableId = 2, ColumnId = 3  } },
                    { (2, 4),  new() { TableId = 2, ColumnId = 4  } },

                    { (3, 5),  new() { TableId = 3, ColumnId = 5  } },
                    { (3, 6),  new() { TableId = 3, ColumnId = 6  } },

                    { (4, 7),  new() { TableId = 4, ColumnId = 7  } },
                    { (4, 8),  new() { TableId = 4, ColumnId = 8  } },

                    { (5, 9),  new() { TableId = 5, ColumnId = 9  } },
                    { (5, 10), new() { TableId = 5, ColumnId = 10 } },
                },
                Constraints = new()
                {
                    { 1, new() { Id = 1, TargetTableId = 5, SourceTableId = 4 } },
                    { 2, new() { Id = 2, TargetTableId = 4, SourceTableId = 3 } },

                    { 3, new() { Id = 3, TargetTableId = 5, SourceTableId = 2 } },
                    { 4, new() { Id = 4, TargetTableId = 2, SourceTableId = 1 } },
                },
            };

            var tables = new List<long> { 1, 3, 5 };

            var pathFinder = new MoveToBetterPathFinder();

            var expected = new List<ResultRouteTreeTest>
            {
                new ResultRouteTreeTest
                {
                    Table = new() { Id = 5 },
                    Children = new List<(ResultRoute, ResultRouteTreeTest)>
                    {
                        (
                            new ResultRoute
                            {
                                Start = new() { Id = 5 },
                                Route = new List<(Table, Constraint)>
                                {
                                    (
                                        new() { Id = 4 },
                                        new() { Id = 1, TargetTableId = 5, SourceTableId = 4 }
                                    ),
                                    (
                                        new() { Id = 3 },
                                        new() { Id = 2, TargetTableId = 4, SourceTableId = 3 }
                                    )
                                }
                            },
                            new ResultRouteTreeTest
                            {
                                Table = new() { Id = 3 },
                                Children = new List<(ResultRoute, ResultRouteTreeTest)> { },
                            }
                        ),
                        (
                            new ResultRoute
                            {
                                Start = new() { Id = 5 },
                                Route = new List<(Table, Constraint)>
                                {
                                    (
                                        new() { Id = 2 },
                                        new() { Id = 3, TargetTableId = 5, SourceTableId = 2 }
                                    ),
                                    (
                                        new() { Id = 1 },
                                        new() { Id = 4, TargetTableId = 2, SourceTableId = 1 }
                                    ),
                                },
                            },
                            new ResultRouteTreeTest
                            {
                                Table = new() { Id = 1 },
                                Children = new List<(ResultRoute, ResultRouteTreeTest)> { },
                            }
                        )
                    },
                },
            };

            // ACT
            var actual = pathFinder.Help(graph, tables).ToList();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

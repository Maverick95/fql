using SqlHelper.Models;
using SqlHelper.Paths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace SqlHelper.Test.Paths
{
    public class MoveToBetterPathFinderTests
    {
        /*
         * Test Case 1 - Simple Path
         * Single table
         */ 

        [Fact]
        public void Help_ShouldLocate_SingleRoute_Along_SingleChain()
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

            var expected_tree_0 = new ResultRouteTree(new Table { Id = 5 });

            var expected = new List<ResultRouteTree> { expected_tree_0 };

            // ACT
            var actual = pathFinder.Help(graph, tables)
                .ToList();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

using FluentAssertions;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FakeItEasy;

namespace SqlHelper.Test.Helpers
{
    public class ResultRouteTreeHelpersTests
    {
        [Fact]
        public void CreateTreeFromTable_ShouldCreateCorrectTree()
        {
            // ARRANGE
            var table = new Table { Id = 1, Name = "TABLE", Schema = "SCHEMA" };

            var expected = new ResultRouteTree
            {
                Table = new() { Id = 1, Name = "TABLE", Schema = "SCHEMA" },
                Children = new List<(ResultRoute, ResultRouteTree)> { },
            };

            // ACT
            var actual = ResultRouteTreeHelpers.CreateTreeFromTable(table);

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void CreateTreeFromRoute_ShouldCreateCorrectTree()
        {
            // ARRANGE
            var route = new ResultRoute
            {
                Start = new() { Id = 1, Name = "TABLE_1", Schema = "SCHEMA_1" },
                Route = new List<(Table, Constraint)>
                {
                    (
                        new() { Id = 2, Name = "TABLE_2", Schema = "SCHEMA_2" },
                        new() { Id = 101, TargetTableId = 1, SourceTableId = 2 }
                    ),
                    (
                        new() { Id = 3, Name = "TABLE_3", Schema = "SCHEMA_3" },
                        new() { Id = 102, TargetTableId = 2, SourceTableId = 3 }
                    )
                },
            };

            var expected = new ResultRouteTree
            {
                Table = new() { Id = 1, Name = "TABLE_1", Schema = "SCHEMA_1" },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1, Name = "TABLE_1", Schema = "SCHEMA_1" },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 2, Name = "TABLE_2", Schema = "SCHEMA_2" },
                                    new() { Id = 101, TargetTableId = 1, SourceTableId = 2 }
                                ),
                                (
                                    new() { Id = 3, Name = "TABLE_3", Schema = "SCHEMA_3" },
                                    new() { Id = 102, TargetTableId = 2, SourceTableId = 3 }
                                )
                            },
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 3, Name = "TABLE_3", Schema = "SCHEMA_3" },
                            Children = new List<(ResultRoute, ResultRouteTree)> { }
                        }
                    )
                }
            };

            // ACT
            var actual = ResultRouteTreeHelpers.CreateTreeFromRoute(route);

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        /*
         * 
            master      incoming
            1 -> 2      4 -> 5
              -> 3
         *
         */

        [Fact]
        public void TryMergeTreesFromRoot_ShouldFailToMergeIncomingTree_IfRootTableIsNotInMaster()
        {
            // ARRANGE
            var master = new ResultRouteTree
            {
                Table = new() { Id = 1 },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 2 },
                                    new() { Id = 101, TargetTableId = 1, SourceTableId = 2 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 2 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    ),
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 3 },
                                    new() { Id = 102, TargetTableId = 1, SourceTableId = 3 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 3 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    )
                },
            };

            var incoming = new ResultRouteTree
            {
                Table = new() { Id = 4 },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 4 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 5 },
                                    new() { Id = 103, TargetTableId = 4, SourceTableId = 5 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 5 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    )
                },
            };

            // Copy of master
            var master_expected = new ResultRouteTree
            {
                Table = new() { Id = 1 },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 2 },
                                    new() { Id = 101, TargetTableId = 1, SourceTableId = 2 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 2 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    ),
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 3 },
                                    new() { Id = 102, TargetTableId = 1, SourceTableId = 3 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 3 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    )
                },
            };

            // ACT
            var actual = ResultRouteTreeHelpers.TryMergeTreesFromRoot(master, incoming);

            // ASSERT
            actual.Should().BeFalse();
            master.Should().BeEquivalentTo(master_expected);
        }

        /*
         * 
            master      incoming
            1 -> 2      3
              -> 3
         *
         */

        [Fact]
        public void TryMergeTreesFromRoot_ShouldSucceedToMergeIncomingTree_IfRootTableIsInMaster()
        {
            // ARRANGE
            var master = new ResultRouteTree
            {
                Table = new() { Id = 1 },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 2 },
                                    new() { Id = 101, TargetTableId = 1, SourceTableId = 2 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 2 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    ),
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 3 },
                                    new() { Id = 102, TargetTableId = 1, SourceTableId = 3 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 3 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    )
                },
            };

            var incoming = new ResultRouteTree
            {
                Table = new() { Id = 3 },
                Children = new List<(ResultRoute, ResultRouteTree)> { },
            };

            // Copy of master
            var master_expected = new ResultRouteTree
            {
                Table = new() { Id = 1 },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 2 },
                                    new() { Id = 101, TargetTableId = 1, SourceTableId = 2 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 2 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    ),
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 3 },
                                    new() { Id = 102, TargetTableId = 1, SourceTableId = 3 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 3 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    )
                },
            };

            // ACT
            var actual = ResultRouteTreeHelpers.TryMergeTreesFromRoot(master, incoming);

            // ASSERT
            actual.Should().BeTrue();
            master.Should().BeEquivalentTo(master_expected);
        }

        /*
         * 
            master      incoming
            1 -> 2      3 -> 4
              -> 3        -> 5
         *
         */

        [Fact]
        public void TryMergeTreesFromRoot_ShouldSucceedToMergeIncomingTree_AndCopyChildren_IfRootTableIsInMaster()
        {
            // ARRANGE
            var master = new ResultRouteTree
            {
                Table = new() { Id = 1 },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 2 },
                                    new() { Id = 101, TargetTableId = 1, SourceTableId = 2 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 2 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    ),
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 3 },
                                    new() { Id = 102, TargetTableId = 1, SourceTableId = 3 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 3 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    )
                },
            };

            var incoming = new ResultRouteTree
            {
                Table = new() { Id = 3 },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 3 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 4 },
                                    new() { Id = 103, TargetTableId = 3, SourceTableId = 4 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 4 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    ),
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 3 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 5 },
                                    new() { Id = 104, TargetTableId = 3, SourceTableId = 5 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 5 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    ),
                },
            };

            // Copy of master
            var master_expected = new ResultRouteTree
            {
                Table = new() { Id = 1 },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 2 },
                                    new() { Id = 101, TargetTableId = 1, SourceTableId = 2 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 2 },
                            Children = new List<(ResultRoute, ResultRouteTree)> { },
                        }
                    ),
                    (
                        new ResultRoute
                        {
                            Start = new() { Id = 1 },
                            Route = new List<(Table, Constraint)>
                            {
                                (
                                    new() { Id = 3 },
                                    new() { Id = 102, TargetTableId = 1, SourceTableId = 3 }
                                )
                            }
                        },
                        new ResultRouteTree
                        {
                            Table = new() { Id = 3 },
                            Children = new List<(ResultRoute, ResultRouteTree)>
                            {
                                (
                                    new ResultRoute
                                    {
                                        Start = new() { Id = 3 },
                                        Route = new List<(Table, Constraint)>
                                        {
                                            (
                                                new() { Id = 4 },
                                                new() { Id = 103, TargetTableId = 3, SourceTableId = 4 }
                                            )
                                        }
                                    },
                                    new ResultRouteTree
                                    {
                                        Table = new() { Id = 4 },
                                        Children = new List<(ResultRoute, ResultRouteTree)> { },
                                    }
                                ),
                                (
                                    new ResultRoute
                                    {
                                        Start = new() { Id = 3 },
                                        Route = new List<(Table, Constraint)>
                                        {
                                            (
                                                new() { Id = 5 },
                                                new() { Id = 104, TargetTableId = 3, SourceTableId = 5 }
                                            )
                                        }
                                    },
                                    new ResultRouteTree
                                    {
                                        Table = new() { Id = 5 },
                                        Children = new List<(ResultRoute, ResultRouteTree)> { },
                                    }
                                ),
                            },
                        }
                    )
                },
            };

            // ACT
            var actual = ResultRouteTreeHelpers.TryMergeTreesFromRoot(master, incoming);

            // ASSERT
            actual.Should().BeTrue();
            master.Should().BeEquivalentTo(master_expected);
        }

        public class TestClass
        {
            public int Id { get; set; }

        }

        [Fact]
        public void EnumerateTreeDepthFirst_ShouldCallInitiatorAndGeneratorFunctions_WithCorrectArgs_InHierarchicalOrder()
        {
            // ARRANGE
            /*
                1 -> 2
                  -> 3
            */

            /* 1st CHILD */
            var child_1_route = new ResultRoute
            {
                Start = new() { Id = 1 },
                Route = new List<(Table, Constraint)>
                {
                    (
                        new() { Id = 2 },
                        new() { Id = 101, TargetTableId = 1, SourceTableId = 2 }
                    )
                }
            };

            var child_1_tree = new ResultRouteTree
            {
                Table = new() { Id = 2 },
                Children = new List<(ResultRoute, ResultRouteTree)> { },
            };

            /* 2nd CHILD */
            var child_2_route = new ResultRoute
            {
                Start = new() { Id = 1 },
                Route = new List<(Table, Constraint)>
                {
                    (
                        new() { Id = 3 },
                        new() { Id = 102, TargetTableId = 1, SourceTableId = 3 }
                    )
                }
            };

            var child_2_tree = new ResultRouteTree
            {
                Table = new() { Id = 3 },
                Children = new List<(ResultRoute, ResultRouteTree)> { },
            };

            var master = new ResultRouteTree
            {
                Table = new() { Id = 1 },
                Children = new List<(ResultRoute, ResultRouteTree)>
                {
                    ( child_1_route, child_1_tree ),
                    ( child_2_route, child_2_tree ),
                },
            };

            var initiator = A.Fake<Func<ResultRouteTree, TestClass>>();
            var generator = A.Fake<Func<TestClass, ResultRoute, ResultRouteTree, TestClass>>();

            var initiator_result = new TestClass { Id = 1001 };
            var generator_result_child_1 = new TestClass { Id = 1002 };
            var generator_result_child_2 = new TestClass { Id = 1003 };

            A.CallTo(() => initiator(master))
                .Returns(initiator_result);

            A.CallTo(() => generator(initiator_result, child_1_route, child_1_tree))
                .Returns(generator_result_child_1);

            A.CallTo(() => generator(initiator_result, child_2_route, child_2_tree))
                .Returns(generator_result_child_2);

            // ACT
            ResultRouteTreeHelpers.EnumerateTreeDepthFirst(master, initiator, generator);

            // ASSERT
            A.CallTo(() => initiator(master))
                .MustHaveHappenedOnceExactly()
                .Then(
            A.CallTo(() => generator(initiator_result, child_1_route, child_1_tree))
                .MustHaveHappenedOnceExactly())
                .Then(
            A.CallTo(() => generator(initiator_result, child_2_route, child_2_tree))
                .MustHaveHappenedOnceExactly());
        }
    }
}

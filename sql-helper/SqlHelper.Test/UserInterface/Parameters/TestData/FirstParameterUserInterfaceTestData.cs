using SqlHelper.Helpers;
using SqlHelper.Models;
using Xunit;

namespace SqlHelper.Test.UserInterface.Parameters.TestData
{
    public class FirstParameterUserInterfaceTestData: TheoryData<
        List<string>, // instructions passed to interface (strict ordering)
        List<LogRecord>, // expected log records (strict ordering)
        SqlQueryParameters // expected output
        >
    {
        public FirstParameterUserInterfaceTestData()
        {
            #region Tables not found

            Add(
                new()
                {
                    "t not-found",
                    "t doesnot-exist",
                    "table neverheardofit",
                    "table try_something_else",
                    "execute",
                },
                new()
                {
                    new()   { Type = "write",         Content = "> " },
                    new()   { Type = "readline",      Content = "t not-found" },
                    new()   { Type = "writeline",     Content = "table command contains no matches, please try again" },

                    new()   { Type = "write",         Content = "> " },
                    new()   { Type = "readline",      Content = "t doesnot-exist" },
                    new()   { Type = "writeline",     Content = "table command contains no matches, please try again" },

                    new()   { Type = "write",         Content = "> " },
                    new()   { Type = "readline",      Content = "table neverheardofit" },
                    new()   { Type = "writeline",     Content = "table command contains no matches, please try again" },

                    new()   { Type = "write",         Content = "> " },
                    new()   { Type = "readline",      Content = "table try_something_else" },
                    new()   { Type = "writeline",     Content = "table command contains no matches, please try again" },

                    new()   { Type = "write",         Content = "> " },
                    new()   { Type = "readline",      Content = "execute" },
                },

                new SqlQueryParameters
                {
                    Tables = new List<Table>(),
                    Filters = new List<Column>(),
                }
            );

            #endregion
        }
    }
}

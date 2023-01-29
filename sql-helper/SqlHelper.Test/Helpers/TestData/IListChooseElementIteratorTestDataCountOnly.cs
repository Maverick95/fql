using Xunit;

namespace SqlHelper.Test.Helpers.TestData
{
    public class IListChooseElementIteratorTestDataCountOnly: TheoryData<int, IList<IList<int>>>
    {
        public IListChooseElementIteratorTestDataCountOnly()
        {
            #region Edge case of 0
            Add(0, new List<IList<int>>
            {
            
            });
            #endregion

            #region Normal case #1
            Add(3, new List<IList<int>>
            {
                new List<int> { 0 },
                new List<int> { 0 , 1 },
                new List<int> { 0 , 1 , 2 },
                new List<int> { 0 , 2 },
                new List<int> { 1 },
                //
                new List<int> { 1 , 2 },
                new List<int> { 2 },
            });
            #endregion

            #region Normal case #2
            Add(5, new List<IList<int>>
            {
                new List<int> { 0 },
                new List<int> { 0 , 1 },
                new List<int> { 0 , 1 , 2 },
                new List<int> { 0 , 1 , 2 , 3 },
                new List<int> { 0 , 1 , 2 , 3 , 4 },
                //
                new List<int> { 0 , 1 , 2 , 4 },
                new List<int> { 0 , 1 , 3 },
                new List<int> { 0 , 1 , 3 , 4 },
                new List<int> { 0 , 1 , 4 },
                new List<int> { 0 , 2 },
                //
                new List<int> { 0 , 2 , 3 },
                new List<int> { 0 , 2 , 3 , 4 },
                new List<int> { 0 , 2 , 4 },
                new List<int> { 0 , 3 },
                new List<int> { 0 , 3 , 4 },
                //
                new List<int> { 0 , 4 },
                new List<int> { 1 },
                new List<int> { 1 , 2 },
                new List<int> { 1 , 2 , 3 },
                new List<int> { 1 , 2 , 3 , 4 },
                //
                new List<int> { 1 , 2 , 4 },
                new List<int> { 1 , 3 },
                new List<int> { 1 , 3 , 4 },
                new List<int> { 1 , 4 },
                new List<int> { 2 },
                //
                new List<int> { 2 , 3 },
                new List<int> { 2 , 3 , 4 },
                new List<int> { 2 , 4 },
                new List<int> { 3 },
                new List<int> { 3 , 4 },
                //
                new List<int> { 4 },
            });
            #endregion
        }
    }
}

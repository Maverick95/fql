using FakeItEasy;
using FluentAssertions;
using SqlHelper.Config;
using SqlHelper.Helpers;
using Xunit;

namespace SqlHelper.Test.Config
{
    public class AppResourceConfigManagerTests
    {
        private readonly IFileManager _mockFileManager;
        private readonly AppResourceConfigManager _configManager;

        public AppResourceConfigManagerTests()
        {
            _mockFileManager = A.Fake<IFileManager>();
            _configManager = new AppResourceConfigManager(_mockFileManager);
        }


        [Fact]
        public void List_ShouldReturnJsonFilesOnly()
        {
            // ARRANGE
            var wd = Directory.GetCurrentDirectory();
            var files = new List<string>
            {
                $"{wd}\\data\\test1.json",
                $"{wd}\\data\\test2.json",
                $"{wd}\\data\\test3.txt",
                $"{wd}\\data\\test4.txt",
            };
            A.CallTo(() => _mockFileManager.List(A<string>._)).Returns(files);

            var expected = new List<string>
            {
                "test1",
                "test2",
            };

            // ACT
            var actual = _configManager.List();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void List_ShouldReturnFilesInTopDirectoryOnly()
        {
            // ARRANGE
            var wd = Directory.GetCurrentDirectory();
            var files = new List<string>
            {
                $"{wd}\\data\\test1.json",
                $"{wd}\\data\\test2.json",
                $"{wd}\\data\\sub-folder-1\\test3.json",
                $"{wd}\\data\\sub-folder-2\\test4.json",
                $"{wd}\\data\\test5.json"
            };
            A.CallTo(() => _mockFileManager.List(A<string>._)).Returns(files);

            var expected = new List<string>
            {
                "test1",
                "test2",
                "test5",
            };

            // ACT
            var actual = _configManager.List();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
    }
}

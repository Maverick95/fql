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
        private readonly ILocation _mockLocation;
        private readonly AppResourceConfigManager _configManager;

        private const string _location = @"C:\test";

        public AppResourceConfigManagerTests()
        {
            _mockFileManager = A.Fake<IFileManager>();
            _mockLocation = A.Fake<ILocation>();
            A.CallTo(() => _mockLocation.Location()).Returns(_location);
            _configManager = new AppResourceConfigManager(_mockFileManager, _mockLocation);
        }

        [Fact]
        public void List_ShouldReturnJsonFilesOnly()
        {
            // ARRANGE
            var files = new List<string>
            {
                @"C:\test\test1.json",
                @"C:\test\test2.json",
                @"C:\test\test3.txt",
                @"C:\test\test4.txt",
            };
            A.CallTo(() => _mockFileManager.List(@"C:\test")).Returns(files);

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
                @"C:\test\test1.json",
                @"C:\test\test2.json",
                @"C:\test\sub-folder-1\test3.json",
                @"C:\test\sub-folder-2\test4.json",
                @"C:\test\test5.json"
            };
            A.CallTo(() => _mockFileManager.List(@"C:\test")).Returns(files);

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

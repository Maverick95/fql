using FakeItEasy;
using FluentAssertions;
using SqlHelper.Config;
using SqlHelper.Helpers;
using SqlHelper.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
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

        [Fact]
        public void Read_ShouldReadDbDataForExistingConfig()
        {
            // ARRANGE
            var alias = "test";
            var dbDataContent = File.ReadAllText("./TestData/DbData.json");
            A.CallTo(() => _mockFileManager.Read(@"C:\test\test.json"))
                .Returns((true, dbDataContent));

            // ACT
            (var actual_exists, var actual_dbData) = _configManager.Read(alias);

            // ASSERT
            actual_exists.Should().BeTrue();
            actual_dbData?.Tables.Should().NotBeNull();
            actual_dbData?.Columns.Should().NotBeNull();
            actual_dbData?.Constraints.Should().NotBeNull();
        }

        [Fact]
        public void Read_ShouldDetectNonExistingConfig()
        {
            // ARRANGE
            var alias = "test";
            A.CallTo(() => _mockFileManager.Read(@"C:\test\test.json"))
                .Returns((false, null));

            // ACT
            (var actual_exists, var actual_dbData) = _configManager.Read(alias);

            // ASSERT
            actual_exists.Should().BeFalse();
            actual_dbData.Should().BeNull();
        }

        [Fact]
        public void Write_ShouldWriteDataToFile()
        {
            // ARRANGE
            var alias = "test";
            var dbDataContent = File.ReadAllText("./TestData/DbData.json");
            dbDataContent = Regex.Replace(dbDataContent, "\\s", "");
            var dbData = JsonSerializer.Deserialize<DbData>(dbDataContent);

            // ACT
            _configManager.Write(alias, dbData);

            // ASSERT
            A.CallTo(() => _mockFileManager.Write(@"C:\test\test.json", dbDataContent))
                .MustHaveHappened();
        }
    }
}

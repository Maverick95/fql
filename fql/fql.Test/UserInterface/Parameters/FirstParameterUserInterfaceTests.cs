﻿using FakeItEasy;
using FluentAssertions;
using fql.UserInterface.Choices.Formatters;
using fql.UserInterface.Choices.Models;
using fql.UserInterface.Choices.Selectors;
using SqlHelper.Helpers;
using SqlHelper.Models;
using SqlHelper.Test.UserInterface.Parameters.TestData;
using SqlHelper.UserInterface.Parameters;
using SqlHelper.UserInterface.Parameters.Commands;
using System.Text.Json;
using Xunit;

namespace SqlHelper.Test.UserInterface.Parameters
{
    public class FirstParameterUserInterfaceTests
    {
        private readonly IStream _mockStream;
        private readonly LoggerStream _loggerStream;
        private readonly FirstParameterUserInterface _parameterUserInterface;

        public FirstParameterUserInterfaceTests()
        {
            _mockStream = A.Fake<IStream>();
            _loggerStream = new LoggerStream(_mockStream);
            _parameterUserInterface = new FirstParameterUserInterface(_loggerStream,
                A.Fake<IChoiceSelector<string>>(),
                A.Fake<IChoiceFormatter<string>>(),
                new AddFiltersCommandHandler(_loggerStream, A.Fake<IChoiceSelector<FilterChoice>>(), A.Fake<IChoiceFormatter<FilterChoice>>()),
                new AddTablesCommandHandler(_loggerStream, A.Fake<IChoiceSelector<TableChoice>>(), A.Fake<IChoiceFormatter<TableChoice>>()),
                new FinishCommandHandler(_loggerStream));
        }

        [Theory]
        [ClassData(typeof(FirstParameterUserInterfaceTestData))]
        public void GetParameters_ShouldReturn_NoParameters_ForOneTableRequestThatDoesNotExist(
            List<string> instructions,
            List<LogRecord> expected_logs,
            SqlQueryParameters expected_parameters)
        {
            // ARRANGE
            var dbData = JsonSerializer.Deserialize<DbData>(File.ReadAllText("./TestData/DbData.json"));
            A.CallTo(() => _mockStream.ReadLine()).ReturnsNextFromSequence(instructions.ToArray());

            // ACT
            (_, var actual_parameters) = _parameterUserInterface.GetParameters(dbData);
            var actual_logs = _loggerStream.Logs;

            // ASSERT
            actual_parameters.Should().BeEquivalentTo(expected_parameters);
            actual_logs.Should().BeEquivalentTo(expected_logs, options => options.WithStrictOrdering());
        }
    }
}

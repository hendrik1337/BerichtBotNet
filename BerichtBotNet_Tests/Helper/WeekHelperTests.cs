using BerichtBotNet.Helper;

namespace BerichtBotNet_Tests.Helper;

using System;
using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class WeekHelperTests
{
    [Test]
    public void StringArrayToDateTimeList_ReturnsCorrectList()
    {
        // Arrange
        string[] stringDates = { "2023-01-01", "2023-01-07", "2023-01-15" };
        List<DateTime> expectedList = new List<DateTime>
        {
            new (2023, 1, 1),
            new (2023, 1, 7),
            new (2023, 1, 15)
        };

        // Act
        List<DateTime> resultList = WeekHelper.StringArrayToDateTimeList(stringDates);

        // Assert
        CollectionAssert.AreEqual(expectedList, resultList);
    }

    [Test]
    public void DateTimetoCalendarWeek_ReturnsCorrectCalendarWeekString()
    {
        // Arrange
        DateTime[] dates = 
        {
            new (2023, 1, 1),
            new (2023, 12, 31),
            new (2023, 7, 11),
            new (2023, 7, 31)
        };
        string[] expectedCalendarWeekString = {"KW 52 2022", "KW 52 2023", "KW 28 2023", "KW 31 2023"};

        // Act
        for (var i = 0; i < dates.Length; i++)
        {
            string resultCalendarWeekString = WeekHelper.DateTimeToCalendarWeekYearCombination(dates[i]);

            // Assert
            Assert.That(resultCalendarWeekString, Is.EqualTo(expectedCalendarWeekString[i]));
        }
        
    }

    public static IEnumerable<TestCaseData> GetBerichtsheftTestData()
    {
        yield return new TestCaseData(new DateTime(2023, 7, 10), 50);
        yield return new TestCaseData(new DateTime(2023, 7, 11), 50);
        yield return new TestCaseData(new DateTime(2023, 7, 16), 50);
        yield return new TestCaseData(new DateTime(2023, 7, 17), 51);
        yield return new TestCaseData(new DateTime(2023, 7, 18), 51);
        yield return new TestCaseData(new DateTime(2023, 7, 23), 51);
        // Add more test cases as needed
    }

    [Test]
    [TestCaseSource(nameof(GetBerichtsheftTestData))]
    public void GetBerichtsheftNumber_ReturnsCorrectNumber(DateTime currentDate, int expectedNumber)
    {
        // Arrange
        DateTime ausbildungsStart = new DateTime(2022, 8, 3);

        // Act
        int resultNumber = WeekHelper.GetBerichtsheftNumber(ausbildungsStart, currentDate);

        // Assert
        Assert.That(resultNumber, Is.EqualTo(expectedNumber));
    }

}

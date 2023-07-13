using System;
using System.Collections.Generic;
using BerichtBotNet.Data;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BerichtBotNet_Tests.Helper
{
    [TestFixture]
    public class BerichtsheftTests
    {
        private BerichtBotContext _context;

        private ApprenticeRepository _apprenticeRepository;
        private GroupRepository _groupRepository;
        private LogRepository _logRepository;
        private SkippedWeeksRepository _weeksRepository;

        private Berichtsheft _berichtsheft;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<BerichtBotContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .UseLazyLoadingProxies()
                .Options;

            _context = new BerichtBotContext(options);
            _apprenticeRepository = new ApprenticeRepository(_context);
            _groupRepository = new GroupRepository(_context);
            _logRepository = new LogRepository(_context);
            _weeksRepository = new SkippedWeeksRepository(_context);

            _berichtsheft = new Berichtsheft(_apprenticeRepository, _logRepository, _weeksRepository);

            Group group = new Group
            {
                Name = "GroupName", DiscordGroupId = "1434q3q", StartOfApprenticeship = DateTime.Today,
                ReminderTime = DateTime.Now
            };
            _groupRepository.CreateGroup(group);
            _apprenticeRepository.CreateApprentice(new Apprentice
                { Name = "John Doe", Skipped = false, DiscordUserId = "1337", Group = group });

            // Initialize and seed the in-memory database for testing
            // You can use a testing framework like Entity Framework Core InMemory or a mocking framework to set up test data.
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any resources used for testing
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public void GetBerichtsheftWriterOfBerichtsheftNumber_ExistingLog_ReturnsWriter()
        {
            // Arrange
            Apprentice
                expectedWriter =
                    _apprenticeRepository
                        .GetApprenticeByDiscordId("1337");
            Log log = new Log { Apprentice = expectedWriter, BerichtheftNummer = 123 };
            _context.Logs.Add(log);
            _context.SaveChanges();

            // Act
            Apprentice actualWriter =
                _berichtsheft.GetBerichtsheftWriterOfBerichtsheftNumber(log.BerichtheftNummer, "GroupName");

            // Assert
            Assert.That(actualWriter, Is.EqualTo(expectedWriter));
        }

        [Test]
        public void GetBerichtsheftWriterOfBerichtsheftNumber_NoLogs_ThrowsException()
        {
            // Arrange
            int berichtsheftNumber = 1232;

            // Act & Assert
            Assert.Throws<ApprenticeNotFoundException>(() =>
                _berichtsheft.GetBerichtsheftWriterOfBerichtsheftNumber(berichtsheftNumber, "GroupName"));
        }

        [Test]
        public void GetCurrentBerichtsheftWriterOfGroup_NonEmptyGroup_ReturnsWriter()
        {
            // Arrange
            Apprentice expectedWriter = _apprenticeRepository.GetApprenticeByDiscordId("1337");
            // Set up the apprentices in the group in the database

            // Act
            Apprentice actualWriter = _berichtsheft.GetCurrentBerichtsheftWriterOfGroup(expectedWriter.Group.Id);

            // Assert
            Assert.That(actualWriter, Is.EqualTo(expectedWriter));
        }

        [Test]
        public void GetCurrentBerichtsheftWriterOfGroup_EmptyGroup_ThrowsException()
        {
            // Arrange
            // Set up an empty group in the database

            // Act & Assert
            Assert.Throws<GroupIsEmptyException>(() => _berichtsheft.GetCurrentBerichtsheftWriterOfGroup(1337));
        }

        [Test]
        public void CurrentBerichsheftWriterWrote_AddsLogEntryForCurrentWriter()
        {
            // Arrange
            Apprentice currentWriter = _apprenticeRepository.GetApprenticeByDiscordId("1337");
            // Set up the current writer in the database

            // Act
            _berichtsheft.CurrentBerichsheftWriterWrote(currentWriter.Group.Id);

            // Assert
            // Check if a new log entry is added for the current writer
            bool logEntryExists = _context.Logs.Any(log => log.Apprentice.Id == currentWriter.Id);
            Assert.IsTrue(logEntryExists);
        }

        [Test]
        public void GetApprenticesThatNeverWrote_ApprenticesExist_LogsExist_ReturnsApprenticesThatNeverWrote()
        {
            // Arrange
            var apprentice1 = new Apprentice { Id = 2 };
            var apprentice2 = new Apprentice { Id = 3 };
            var apprentice3 = new Apprentice { Id = 4 };
            var apprenticesOfGroup = new List<Apprentice> { apprentice1, apprentice2, apprentice3 };

            var log1 = new Log { Apprentice = apprentice1 };
            var log2 = new Log { Apprentice = apprentice3 };
            var logs = new List<Log> { log1, log2 };

            // Act
            var result = _berichtsheft.GetApprenticesThatNeverWrote(apprenticesOfGroup, logs);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.Contains(apprentice2, result);
        }

        [Test]
        public void GetApprenticesThatNeverWrote_ApprenticesExist_NoLogsExist_ReturnsAllApprentices()
        {
            // Arrange
            var apprentice1 = new Apprentice { Id = 1 };
            var apprentice2 = new Apprentice { Id = 2 };
            var apprentice3 = new Apprentice { Id = 3 };
            var apprenticesOfGroup = new List<Apprentice> { apprentice1, apprentice2, apprentice3 };
            var logs = new List<Log>();

            // Act
            var result = _berichtsheft.GetApprenticesThatNeverWrote(apprenticesOfGroup, logs);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.Contains(apprentice1, result);
            Assert.Contains(apprentice2, result);
            Assert.Contains(apprentice3, result);
        }

        [Test]
        public void GetApprenticesThatNeverWrote_NoApprenticesExist_ReturnsEmptyList()
        {
            // Arrange
            var apprenticesOfGroup = new List<Apprentice>();
            var logs = new List<Log>();

            // Act
            var result = _berichtsheft.GetApprenticesThatNeverWrote(apprenticesOfGroup, logs);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void FilterApprenticesBySkipCount_ApprenticesExist_ReturnsFilteredApprentices()
        {
            // Arrange
            var apprentice1 = new Apprentice { Skipped = true };
            var apprentice2 = new Apprentice { Skipped = false };
            var apprentice3 = new Apprentice { Skipped = true };
            var apprentices = new List<Apprentice> { apprentice1, apprentice2, apprentice3 };
            bool skipped = true;

            // Act
            var result = _berichtsheft.FilterApprenticesBySkipCount(apprentices, skipped);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.Contains(apprentice1, result);
            Assert.Contains(apprentice3, result);
        }

        [Test]
        public void FilterApprenticesBySkipCount_ApprenticesExist_NoFilteredApprentices_ReturnsEmptyList()
        {
            // Arrange
            var apprentice1 = new Apprentice { Skipped = false };
            var apprentice2 = new Apprentice { Skipped = false };
            var apprentice3 = new Apprentice { Skipped = false };
            var apprentices = new List<Apprentice> { apprentice1, apprentice2, apprentice3 };
            bool skipped = true;

            // Act
            var result = _berichtsheft.FilterApprenticesBySkipCount(apprentices, skipped);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void FilterApprenticesBySkipCount_NoApprenticesExist_ReturnsEmptyList()
        {
            // Arrange
            var apprentices = new List<Apprentice>();
            bool skipped = true;

            // Act
            var result = _berichtsheft.FilterApprenticesBySkipCount(apprentices, skipped);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void FilterApprenticesFromLogBySkipped_LogsExist_ReturnsFilteredLogs()
        {
            // Arrange
            var apprentice1 = new Apprentice { Id = 1, Skipped = true };
            var apprentice2 = new Apprentice { Id = 2, Skipped = false };
            var apprentice3 = new Apprentice { Id = 3, Skipped = true };

            var log1 = new Log { Apprentice = apprentice1, Timestamp = DateTime.UtcNow };
            var log2 = new Log { Apprentice = apprentice2, Timestamp = DateTime.UtcNow.AddDays(-1) };
            var log3 = new Log { Apprentice = apprentice3, Timestamp = DateTime.UtcNow.AddDays(-2) };
            var log4 = new Log { Apprentice = apprentice1, Timestamp = DateTime.UtcNow.AddDays(-3) };

            var logs = new List<Log> { log1, log2, log3, log4 };
            bool skipped = true;

            // Act
            var result = _berichtsheft.FilterApprenticesFromLogBySkipped(logs, skipped);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.Contains(log1, result);
            Assert.Contains(log3, result);
        }

        [Test]
        public void FilterApprenticesFromLogBySkipped_LogsExist_NoFilteredLogs_ReturnsEmptyList()
        {
            // Arrange
            var apprentice1 = new Apprentice { Id = 1, Skipped = false };
            var apprentice2 = new Apprentice { Id = 2, Skipped = false };

            var log1 = new Log { Apprentice = apprentice1, Timestamp = DateTime.UtcNow };
            var log2 = new Log { Apprentice = apprentice2, Timestamp = DateTime.UtcNow.AddDays(-1) };

            var logs = new List<Log> { log1, log2 };
            bool skipped = true;

            // Act
            var result = _berichtsheft.FilterApprenticesFromLogBySkipped(logs, skipped);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void FilterApprenticesFromLogBySkipped_NoLogsExist_ReturnsEmptyList()
        {
            // Arrange
            var logs = new List<Log>();
            bool skipped = true;

            // Act
            var result = _berichtsheft.FilterApprenticesFromLogBySkipped(logs, skipped);

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void BerichtsheftOrder_GroupExists_ReturnsCorrectApprenticesOrder()
        {
            // Arrange
            Group group = new Group
            {
                Name = "Example", DiscordGroupId = "4567890", StartOfApprenticeship = DateTime.Today,
                ReminderTime = DateTime.Now
            };
            _groupRepository.CreateGroup(group);

            var apprentice1 = new Apprentice { Skipped = false, Name = "Jeff", DiscordUserId = "12", Group = group };
            var apprentice2 = new Apprentice { Skipped = true, Name = "Bob", DiscordUserId = "13", Group = group };
            var apprentice3 = new Apprentice { Skipped = false, Name = "Carmen", DiscordUserId = "14", Group = group };
            var apprentice4 = new Apprentice { Skipped = true, Name = "Melissa", DiscordUserId = "15", Group = group };

            _apprenticeRepository.CreateApprentice(apprentice1);
            _apprenticeRepository.CreateApprentice(apprentice2);
            _apprenticeRepository.CreateApprentice(apprentice3);
            _apprenticeRepository.CreateApprentice(apprentice4);


            var log1 = new Log
                { Apprentice = apprentice1, Timestamp = DateTime.Now, Group = group, BerichtheftNummer = 11 };
            var log2 = new Log
                { Apprentice = apprentice2, Timestamp = DateTime.Now, Group = group, BerichtheftNummer = 12 };
            var log3 = new Log
                { Apprentice = apprentice3, Timestamp = DateTime.Now, Group = group, BerichtheftNummer = 13 };
            var log4 = new Log
                { Apprentice = apprentice4, Timestamp = DateTime.Now, Group = group, BerichtheftNummer = 14 };

            _logRepository.CreateLog(log1);
            _logRepository.CreateLog(log2);
            _logRepository.CreateLog(log3);
            _logRepository.CreateLog(log4);

            // Act
            var result = _berichtsheft.BerichtsheftOrder(group);

            // Assert
            Assert.AreEqual(2, result.Item1.Count);
            Assert.Contains(apprentice1, result.Item1);
            Assert.Contains(apprentice3, result.Item1);

            Assert.AreEqual(2, result.Item2.Count);
            Assert.Contains(apprentice2, result.Item2);
            Assert.Contains(apprentice4, result.Item2);
        }

        [Test]
        public void CurrentBerichtsheftWriterMessage_WeekNotSkipped_ReturnsCorrectMessage()
        {
            // Arrange
            var group = _groupRepository.GetGroupByName("GroupName");
            var apprentice = new Apprentice
                { Name = "John Doe", DiscordUserId = "123456789", Group = group, Skipped = false };
            var currentCalendarWeek = WeekHelper.DateTimeToCalendarWeekYearCombination(DateTime.Now);
            var berichtsheftNumber = WeekHelper.GetBerichtsheftNumber(group.StartOfApprenticeship, DateTime.Now);
            var berichtsheftNumberPlusCw = $"(Nr: {berichtsheftNumber}, {currentCalendarWeek})";

            _apprenticeRepository.CreateApprentice(apprentice);

            _berichtsheft.GetCurrentBerichtsheftWriterOfGroup(group.Id);

            // Act
            var result = _berichtsheft.CurrentBerichtsheftWriterMessage(group, false);

            // Assert
            Assert.AreEqual($"Azubi: John Doe muss diese Woche {berichtsheftNumberPlusCw} das Berichtsheft schreiben.",
                result);
        }

        [Test]
        public void CurrentBerichtsheftWriterMessage_WeekSkipped_ReturnsSkippedMessage()
        {
            // Arrange
            var group = new Group
            {
                Name = "Group 2", DiscordGroupId = "12345", StartOfApprenticeship = DateTime.UtcNow,
                ReminderTime = new DateTime(1, 1, 1, 8, 0, 0)
            };
            var currentCalendarWeek = WeekHelper.DateTimeToCalendarWeekYearCombination(DateTime.Now);
            var berichtsheftNumber = WeekHelper.GetBerichtsheftNumber(group.StartOfApprenticeship, DateTime.Now);
            var berichtsheftNumberPlusCw = $"(Nr: {berichtsheftNumber}, {currentCalendarWeek})";

            _groupRepository.CreateGroup(group);
            _weeksRepository.Create(new() { SkippedWeek = DateTime.Now, GroupId = group.Id });

            // Act
            var result = _berichtsheft.CurrentBerichtsheftWriterMessage(group, false);

            // Assert
            Assert.AreEqual($"Diese Woche {berichtsheftNumberPlusCw} muss kein Berichtsheft geschrieben werden.",
                result);
        }

        [Test]
        public void CurrentBerichtsheftWriterMessage_NoWriterFound_ReturnsEmptyMessage()
        {
            // Arrange
            var group = new Group
            {
                Name = "Group 3", DiscordGroupId = "12345", StartOfApprenticeship = DateTime.UtcNow,
                ReminderTime = new DateTime(1, 1, 1, 8, 0, 0)
            };
            var currentCalendarWeek = WeekHelper.DateTimeToCalendarWeekYearCombination(DateTime.Now);
            var berichtsheftNumber = WeekHelper.GetBerichtsheftNumber(group.StartOfApprenticeship, DateTime.Now);
            var berichtsheftNumberPlusCw = $"(Nr: {berichtsheftNumber}, {currentCalendarWeek})";

            // Act
            var result = _berichtsheft.CurrentBerichtsheftWriterMessage(group, false);

            // Assert
            Assert.AreEqual(
                $"Es wurde keine Person gefunden, die das Berichtsheft schreiben kann {berichtsheftNumberPlusCw}",
                result);
        }
    }
}
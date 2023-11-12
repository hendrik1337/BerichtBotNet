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

        private BerichtsheftService _berichtsheftService;

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

            _berichtsheftService = new BerichtsheftService(_apprenticeRepository, _logRepository, _weeksRepository);

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
                _berichtsheftService.GetBerichtsheftWriterOfBerichtsheftNumber(log.BerichtheftNummer, "GroupName");

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
                _berichtsheftService.GetBerichtsheftWriterOfBerichtsheftNumber(berichtsheftNumber, "GroupName"));
        }

        [Test]
        public void GetCurrentBerichtsheftWriterOfGroup_NonEmptyGroup_ReturnsWriter()
        {
            // Arrange
            Apprentice expectedWriter = _apprenticeRepository.GetApprenticeByDiscordId("1337");
            // Set up the apprentices in the group in the database

            // Act
            Apprentice actualWriter = _berichtsheftService.GetCurrentBerichtsheftWriterOfGroup(expectedWriter.Group.Id);

            // Assert
            Assert.That(actualWriter, Is.EqualTo(expectedWriter));
        }

        [Test]
        public void GetCurrentBerichtsheftWriterOfGroup_EmptyGroup_ThrowsException()
        {
            // Arrange
            // Set up an empty group in the database

            // Act & Assert
            Assert.Throws<GroupIsEmptyException>(() => _berichtsheftService.GetCurrentBerichtsheftWriterOfGroup(1337));
        }

        [Test]
        public void CurrentBerichsheftWriterWrote_AddsLogEntryForCurrentWriter()
        {
            // Arrange
            Apprentice currentWriter = _apprenticeRepository.GetApprenticeByDiscordId("1337");
            // Set up the current writer in the database

            // Act
            _berichtsheftService.CurrentBerichsheftWriterWrote(currentWriter.Group.Id);

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
            var result = _berichtsheftService.GetApprenticesThatNeverWrote(apprenticesOfGroup, logs);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
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
            var result = _berichtsheftService.GetApprenticesThatNeverWrote(apprenticesOfGroup, logs);

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
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
            var result = _berichtsheftService.GetApprenticesThatNeverWrote(apprenticesOfGroup, logs);

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
            var result = _berichtsheftService.FilterApprenticesBySkipCount(apprentices, skipped);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
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
            var result = _berichtsheftService.FilterApprenticesBySkipCount(apprentices, skipped);

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
            var result = _berichtsheftService.FilterApprenticesBySkipCount(apprentices, skipped);

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
            var result = _berichtsheftService.FilterApprenticesFromLogBySkipped(logs, skipped);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
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
            var result = _berichtsheftService.FilterApprenticesFromLogBySkipped(logs, skipped);

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
            var result = _berichtsheftService.FilterApprenticesFromLogBySkipped(logs, skipped);

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

            var apprentice1 = new Apprentice { Skipped = false, Name = "Hendrik", DiscordUserId = "12", Group = group };
            var apprentice2 = new Apprentice { Skipped = false, Name = "Vathu", DiscordUserId = "13", Group = group };
            var apprentice3 = new Apprentice { Skipped = false, Name = "Heinrich", DiscordUserId = "14", Group = group };
            var apprentice4 = new Apprentice { Skipped = false, Name = "Phil", DiscordUserId = "15", Group = group };
            var apprentice5 = new Apprentice { Skipped = false, Name = "Aaron", DiscordUserId = "16", Group = group };
            var apprentice6 = new Apprentice { Skipped = false, Name = "Jan", DiscordUserId = "17", Group = group };

            _apprenticeRepository.CreateApprentice(apprentice1);
            _apprenticeRepository.CreateApprentice(apprentice2);
            _apprenticeRepository.CreateApprentice(apprentice3);
            _apprenticeRepository.CreateApprentice(apprentice4);
            _apprenticeRepository.CreateApprentice(apprentice5);
            _apprenticeRepository.CreateApprentice(apprentice6);


            var log1 = new Log
                { Apprentice = apprentice1, Timestamp = DateTime.Parse("2023-09-03 20:00:00.353 +0200"), Group = group, BerichtheftNummer = 57 };
            var log2 = new Log
                { Apprentice = apprentice2, Timestamp = DateTime.Parse("2023-09-10 20:00:00.114 +0200"), Group = group, BerichtheftNummer = 58 };
            var log3 = new Log
                { Apprentice = apprentice3, Timestamp = DateTime.Parse("2023-09-17 20:00:00.070 +0200"), Group = group, BerichtheftNummer = 59 };
            var log4 = new Log
                { Apprentice = apprentice4, Timestamp = DateTime.Parse("2023-09-24 20:00:00.072 +0200"), Group = group, BerichtheftNummer = 60 };
            var log5 = new Log
                { Apprentice = apprentice4, Timestamp = DateTime.Parse("2023-10-01 20:00:00.048 +0200"), Group = group, BerichtheftNummer = 61 };
            var log6 = new Log
                { Apprentice = apprentice4, Timestamp = DateTime.Parse("2023-10-22 20:00:00.048 +0200"), Group = group, BerichtheftNummer = 64 };
            var log7 = new Log
                { Apprentice = apprentice4, Timestamp = DateTime.Parse("2023-10-29 19:00:00.048 +0100"), Group = group, BerichtheftNummer = 65 };
            var log8 = new Log
                { Apprentice = apprentice4, Timestamp = DateTime.Parse("2023-11-05 19:00:00.048 +0100"), Group = group, BerichtheftNummer = 66 };

            _logRepository.CreateLog(log1);
            _logRepository.CreateLog(log2);
            _logRepository.CreateLog(log3);
            _logRepository.CreateLog(log4);
            _logRepository.CreateLog(log5);
            _logRepository.CreateLog(log6);
            _logRepository.CreateLog(log7);
            _logRepository.CreateLog(log8);

            // Act
            var result = _berichtsheftService.BerichtsheftOrder(group);

            List<Apprentice> correctOrder = new List<Apprentice>();
            
            
            correctOrder.Add(apprentice6);
            correctOrder.Add(apprentice1);
            correctOrder.Add(apprentice3);
            correctOrder.Add(apprentice4);
            correctOrder.Add(apprentice5);
            correctOrder.Add(apprentice2);
            // Assert
            // Assert
            Assert.That(result.Item1.ToList(), Is.EquivalentTo(correctOrder));
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

            _berichtsheftService.GetCurrentBerichtsheftWriterOfGroup(group.Id);

            // Act
            var result = _berichtsheftService.CurrentBerichtsheftWriterMessage(group, false);

            // Assert
            Assert.That(result, Is.EqualTo($"Azubi: John Doe muss diese Woche {berichtsheftNumberPlusCw} das Berichtsheft schreiben."));
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
            var result = _berichtsheftService.CurrentBerichtsheftWriterMessage(group, false);

            // Assert
            Assert.That(result, Is.EqualTo($"Diese Woche {berichtsheftNumberPlusCw} muss kein Berichtsheft geschrieben werden."));
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
            var result = _berichtsheftService.CurrentBerichtsheftWriterMessage(group, false);

            // Assert
            Assert.That(
                result, Is.EqualTo($"Es wurde keine Person gefunden, die das Berichtsheft schreiben kann {berichtsheftNumberPlusCw}"));
        }
    }
}
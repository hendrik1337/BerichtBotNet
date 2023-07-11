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
                ReminderTime = DateTime.Now, ReminderWeekDay = DayOfWeek.Monday, Id = 1
            };
            _context.Groups.Add(group);
            _context.SaveChanges();
            _context.Apprentices.Add(new Apprentice
                { Name = "John Doe", SkipCount = 0, DiscordUserId = "1337", Group = group, Id = 1});
            _context.SaveChanges();

            // Initialize and seed the in-memory database for testing
            // You can use a testing framework like Entity Framework Core InMemory or a mocking framework to set up test data.
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any resources used for testing
            _context.Dispose();
        }
        
        [Test]
        public void GetBerichtsheftWriterOfBerichtsheftNumber_ExistingLog_ReturnsWriter()
        {
            // Arrange
            Apprentice expectedWriter = _apprenticeRepository.GetApprentice(1);//new Apprentice { Name = "John Doe", SkipCount = 0, Group = group, DiscordUserId = "1337", Id = 1};
            Log log = new Log { ApprenticeId = expectedWriter.Id, BerichtheftNummer = 123 };
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
            int berichtsheftNumber = 123;

            // Act & Assert
            Assert.Throws<ApprenticeNotFoundException>(() =>
                _berichtsheft.GetBerichtsheftWriterOfBerichtsheftNumber(berichtsheftNumber, "GroupName"));
        }

        [Test]
        public void GetCurrentBerichtsheftWriterOfGroup_NonEmptyGroup_ReturnsWriter()
        {
            // Arrange
            Apprentice expectedWriter = _apprenticeRepository.GetApprentice(1);
            // Set up the apprentices in the group in the database

            // Act
            Apprentice actualWriter = _berichtsheft.GetCurrentBerichtsheftWriterOfGroup(1);

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
            Apprentice currentWriter = _apprenticeRepository.GetApprentice(1);
            // Set up the current writer in the database

            // Act
            _berichtsheft.CurrentBerichsheftWriterWrote(1);

            // Assert
            // Check if a new log entry is added for the current writer
            bool logEntryExists = _context.Logs.Any(log => log.ApprenticeId == currentWriter.Id);
            Assert.IsTrue(logEntryExists);
        }
    }
}
using System.Collections.Generic;
using BerichtsheftCreator.Berichtsheft;
using BerichtsheftCreator.Data;
using NUnit.Framework;

namespace BerichtsheftCreator.Tests.Berichtsheft;

[TestFixture]
[TestOf(typeof(BerichtsheftCreationHelper))]
public class BerichtsheftCreationHelperTest
{

    [Test]
    public void CombineEinzelstunden_Test()
    {
        // Arrange
        var lessons = new List<Lesson>
        {
            new Lesson { lesson = "Math", Date = "1.1.2022", Time = "09:00-10:00", Length = "1", NoteText = "First lesson note" },
            new Lesson { lesson = "Math", Date = "1.1.2022", Time = "10:00-11:00", Length = "1", NoteText = "Second lesson note" },
            new Lesson { lesson = "Math", Date = "1.1.2022", Time = "12:00-13:00", Length = "1", NoteText = "Third lesson note" },
            new Lesson { lesson = "Physics", Date = "2.1.2022", Time = "09:00-10:00", Length = "1", NoteText = "Fourth lesson note" }
        };

        // Act
        var combinedLessons = BerichtsheftCreationHelper.CombineEinzelstunden(lessons);

        // Assert
        Assert.That(combinedLessons.Count, Is.EqualTo(3));
        Assert.That(combinedLessons[0].lesson, Is.EqualTo("Math"));
        Assert.That(combinedLessons[0].Length, Is.EqualTo("2"));
        Assert.That(combinedLessons[0].NoteText, Is.EqualTo("First lesson note\nSecond lesson note"));
        
        Assert.That(combinedLessons[1].lesson, Is.EqualTo("Math"));
        Assert.That(combinedLessons[1].Length, Is.EqualTo("1"));
        Assert.That(combinedLessons[1].NoteText, Is.EqualTo("Third lesson note"));
        
        Assert.That(combinedLessons[2].lesson, Is.EqualTo("Physics"));
        Assert.That(combinedLessons[2].Length, Is.EqualTo("1"));
        Assert.That(combinedLessons[2].NoteText, Is.EqualTo("Fourth lesson note"));

    }
}
using BerichtsheftCreator.Data;

namespace BerichtsheftCreator.Berichtsheft;

public class BerichtsheftCreationHelper
{
    public static String DateToWeekday(DateTime dateValue)
    {
        return dateValue.ToString("ddd");
    }

    public static List<Lesson> CombineEinzelstunden(List<Lesson> lessons)
    {
        List<Lesson> combinedLessons = new List<Lesson>();
        lessons.Sort((x, y) => DateTime.Compare(x.DateTime, y.DateTime));
        
        for (var i = 0; i < lessons.Count; i++)
        {
            if (lessons[i].Length.Equals("1"))
            {
                var currentLesson = lessons[i];
                if (i < lessons.Count - 1 && lessons[i + 1].lesson.Equals(currentLesson.lesson))
                {
                    int len = int.Parse(currentLesson.Length);
                    len++;
                    currentLesson.Length = len.ToString();
                    
                    var nextLesson = lessons[i + 1];
                    if (!string.IsNullOrEmpty(nextLesson.NoteText))
                    {
                        currentLesson.NoteText += "\n" + nextLesson.NoteText;
                    }
                    i++;
                }
                
                combinedLessons.Add(currentLesson);
                
            }
            else
            {
                combinedLessons.Add(lessons[i]);
            }
        }

        return combinedLessons;
    }
}
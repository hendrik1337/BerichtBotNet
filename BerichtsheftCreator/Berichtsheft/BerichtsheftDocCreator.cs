using BerichtsheftCreator.Data;

namespace BerichtsheftCreator.Berichtsheft;

using Xceed.Words.NET;

public class BerichtsheftDocCreator
{
    public static void CreateBerichtsheft(List<Lesson> lessons)
    {
        string file = "C:\\Users\\Hendrik\\RiderProjects\\BerichtBotNet\\Berichtsheft_blank.docx";

        using (DocX doc = DocX.Load(file))
        {
            Dictionary<String, int> totalHours = new Dictionary<string, int>();
            foreach (var lesson in lessons)
            {
                String day = BerichtsheftCreationHelper.DateToWeekday(lesson.DateTime);
                int slot = BerichtsheftCreationHelper.TimeToBerichtheftSlot(lesson.Time.Split("-")[0]);
                var text = doc.Bookmarks[day + slot];
                var stunden = doc.Bookmarks[day + "Len" + slot];

                if (lesson.NoteText != null)
                {
                    text.SetText(lesson.lesson + " " + lesson.NoteText);
                    if (lesson.NoteText.Equals("Entfall"))
                    {
                        stunden.SetText("0");
                    }
                    else
                    {
                        stunden.SetText("2");
                        int time;
                        totalHours.TryGetValue(day, out time);
                        time += 2;
                        totalHours[day] = time;
                    }
                }
                
            }
            
            foreach (var keyValuePair in totalHours)
            {
                var totalBm = doc.Bookmarks[keyValuePair.Key + "Total"];
                totalBm.SetText(keyValuePair.Value.ToString());
            }
            
            doc.SaveAs(file + "new.docx");
        }
    }
}
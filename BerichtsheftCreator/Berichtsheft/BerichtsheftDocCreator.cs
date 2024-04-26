using BerichtsheftCreator.Data;

namespace BerichtsheftCreator.Berichtsheft;

using Xceed.Words.NET;

public class BerichtsheftDocCreator
{
    public static void CreateBerichtsheft(List<Lesson> lessons, String ausbildungsjahr, String berichtsheftNummer, String groupName)
    {
        Console.WriteLine("Creating Word Document");
        string basePath = "/app/Berichtshefte";
        string file = $"{basePath}/Berichtsheft_blank.docx";

        lessons.Sort((x, y) => DateTime.Compare(x.DateTime, y.DateTime));

        using (DocX doc = DocX.Load(file))
        {
            Dictionary<String, int> totalHours = new Dictionary<string, int>();
            List<DateTime> dates = new List<DateTime>();

            var ausbildungsjahrBm = doc.Bookmarks["Ausbildungsjahr"];
            var berichtsheftNummerBm = doc.Bookmarks["BerichtsheftNr"];
            ausbildungsjahrBm.SetText(ausbildungsjahr);
            berichtsheftNummerBm.SetText(berichtsheftNummer);

            int count = 1;
            String lastDayValue = "";

            foreach (var lesson in lessons)
            {
                String day = BerichtsheftCreationHelper.DateToWeekday(lesson.DateTime);

                if (!day.Equals(lastDayValue))
                {
                    count = 1;
                    lastDayValue = day;
                }

                var text = doc.Bookmarks[day + count];
                var stunden = doc.Bookmarks[day + "Len" + count];
                dates.Add(lesson.DateTime);

                if (lesson.NoteText != null)
                {
                    
                    if (lesson.NoteText.Equals("ENTFÄLLT"))
                    {
                        text.SetText(lesson.lesson + ": Entfall");
                        stunden.SetText("0");
                    }
                    else
                    {
                        text.SetText(lesson.lesson + ": " + lesson.NoteText);
                        stunden.SetText(lesson.Length);
                        int time;
                        totalHours.TryGetValue(day, out time);
                        time += int.Parse(lesson.Length);
                        totalHours[day] = time;
                    }
                }

                count++;
            }

            foreach (var keyValuePair in totalHours)
            {
                var totalBm = doc.Bookmarks[keyValuePair.Key + "Total"];
                totalBm.SetText(keyValuePair.Value.ToString());
            }

            var startDate = dates.Min();
            var endDate = dates.Max();

            var startBm = doc.Bookmarks["start"];
            var endBm = doc.Bookmarks["end"];

            startBm.SetText(startDate.ToString("dd.MM.yyyy"));
            endBm.SetText(endDate.ToString("dd.MM.yyyy"));

            String fileName = $"Berichtsheft Schule Woche {berichtsheftNummer}.docx";

            foreach (var docBookmark in doc.Bookmarks)
            {
                docBookmark.Remove();
            }

            Directory.CreateDirectory($"{basePath}/{groupName}");
            doc.SaveAs($"{basePath}/{groupName}/{fileName}");
            Console.WriteLine("Document created. Beginning Upload");
            BerichtsheftApiConnector.UploadBerichtsheft($"{basePath}/{groupName}/{fileName}", fileName,
                ausbildungsjahr);
        }
    }
}
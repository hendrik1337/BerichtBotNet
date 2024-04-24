namespace BerichtsheftCreator.Berichtsheft;

public class BerichtsheftCreationHelper
{
    public static String DateToWeekday(DateTime dateValue)
    {
        return dateValue.ToString("ddd");
    }
}
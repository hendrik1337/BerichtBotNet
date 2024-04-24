namespace BerichtsheftCreator.Berichtsheft;

public class BerichtsheftCreationHelper
{
    public static String DateToWeekday(DateTime dateValue)
    {
        return dateValue.ToString("ddd");
    }

    public static int TimeToBerichtheftSlot(String time)
    {
        switch (time)
        {
            case "07:45":
                return 1;
            case "09:35":
                return 2;
            case "11:25":
                return 3;
            case "13:15":
                return 4;
            case "15:05":
                return 5;
            case "16:55":
                return 6;
        }

        return -1;
    }
}
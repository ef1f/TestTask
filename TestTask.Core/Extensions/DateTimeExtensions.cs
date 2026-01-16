namespace TestTask.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime ToUnspecified(this DateTime dateTime)
    {
        if (dateTime.Kind != DateTimeKind.Unspecified)
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
        return dateTime;
    }
}
namespace TalentInsights.Shared.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime UtcNow()
        {
            return DateTimeOffset.UtcNow.DateTime;
        }
    }
}

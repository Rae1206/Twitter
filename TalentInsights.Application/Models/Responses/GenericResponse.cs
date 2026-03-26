using TalentInsights.Shared.Helpers;

namespace TalentInsights.Application.Models.Responses
{
    public class GenericResponse<T>
    {
        public string Message { get; set; }
        public DateTime TimeStamp { get; } = DateTimeHelper.UtcNow();
        public T Data { get; set; }
    }
}

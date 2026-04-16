using System.Threading.Tasks;

namespace Twitter.Application.Interfaces.Services
{
    public interface IEmailTemplateService
    {
        //Task<EmailTemplateDto> GetTemplateAsync(string templateName);
        Task Init();
        Task Restart();
    }
}

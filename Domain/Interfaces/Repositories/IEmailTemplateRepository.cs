using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> GetByNameAsync(string name);
}

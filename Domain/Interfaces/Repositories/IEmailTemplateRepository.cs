using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IEmailTemplateRepository
{
    EmailTemplate? GetByName(string name);
}
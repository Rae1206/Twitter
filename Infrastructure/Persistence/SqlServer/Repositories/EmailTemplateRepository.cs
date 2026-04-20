using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Twitter.Domain.Database.SqlServer.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class EmailTemplateRepository : GenericRepository<EmailTemplate, int>, IEmailTemplateRepository
{
    public EmailTemplateRepository(TwitterDbContext context) : base(context)
    {
    }

    public EmailTemplate? GetByName(string name)
    {
        return _context.Set<EmailTemplate>()
            .FirstOrDefault(t => t.Name == name);
    }
}
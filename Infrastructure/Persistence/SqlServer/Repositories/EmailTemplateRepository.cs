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

    public async Task<EmailTemplate?> GetByNameAsync(string name)
    {
        return await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Name == name);
    }
}
